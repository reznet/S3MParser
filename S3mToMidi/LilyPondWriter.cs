using System.Collections.Immutable;
using System.Text;
using System.IO;
using System.Collections;
using S3M;
using System.Diagnostics;
using Melanchall.DryWetMidi.Core;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using S3mToMidi.LilyPond;

namespace S3mToMidi
{
    internal class LilyPondWriter
    {
        private readonly TextWriter writer;

        internal LilyPondWriter(TextWriter writer)
        {
            if (writer == null) { throw new ArgumentNullException(nameof(writer)); }
            this.writer = writer;
        }

        public string Write(Dictionary<int, ImmutableList<Event>> allEvents)
        {
            WriteVersion();
            WriteLanguage();
            WriteScoreStart();


            /*
                        writer.WriteLine(@"\paper {
              page-breaking = #ly:one-line-auto-height-breaking
            }");
            */

            writer.WriteLine("<<");

            var channelLastTicks = new Dictionary<int, int>();

            foreach (var channelNumber in allEvents.Keys)
            {
                var trackEvents = allEvents[channelNumber];
                var sortedEvents = trackEvents
                    .OrderBy(trackEvent => trackEvent.Tick)
                    .ThenBy(trackEvent =>
                    {
                        if (trackEvent is TimeSignatureEvent) { return -1; }
                        else if (trackEvent is TempoEvent) { return 0; }
                        else { return 1; }
                    })
                    .ToList();
                var collapsedEvents = CollapseNoteEvents(sortedEvents).ToImmutableList();
                if (!collapsedEvents.Any())
                {
                    Console.Out.WriteLine("Could not find any complete notes in channel {0}", channelNumber);
                    continue;
                }
                var withRests = WithRestEvents(collapsedEvents).ToImmutableList();
                var withSplits = SplitNotesByTimeSignature2(withRests).ToImmutableList();

                withRests
                .Where(e => e is TempoEvent || e is TimeSignatureEvent)
                .Concat(withSplits)
                .OrderBy(trackEvent => trackEvent.Tick)
                .ThenBy(trackEvent =>
                {
                    if (trackEvent is TimeSignatureEvent) { return -1; }
                    else if (trackEvent is TempoEvent) { return 0; }
                    else { return 1; }
                })
                .ToImmutableList();

                new StaffWriter(writer).Write(withRests);
            }

            writer.WriteLine(">>");

            WriteLayout();
            WriteMidi();
            WriteScoreEnd();

            return writer.ToString();
        }

        private void WriteMidi()
        {
            writer.WriteLine(@"\midi {}");
        }

        private void WriteScoreStart()
        {
            /*
                \score {
                … music …
                \layout { }
                \midi { }
                }
            */
            writer.WriteLine(@"\score {");
        }

        private void WriteScoreEnd()
        {
            writer.WriteLine("}");
        }

        private void WriteLayout()
        {
            writer.WriteLine(@"\layout {
  \context {
    \Voice
    \remove Note_heads_engraver
    \consists Completion_heads_engraver
    \remove Rest_engraver
    \consists Completion_rest_engraver
  }
}");
        }

        private void WriteVersion()
        {
            writer.WriteLine("\\version \"2.24.3\"");
        }

        private void WriteLanguage()
        {
            writer.WriteLine("\\language \"english\"");
        }

        private IEnumerable<Event> SplitNotesByTimeSignature2(ImmutableList<Event> events)
        {
            var durations = events.Where(e => e is DurationEvent).Cast<DurationEvent>().ToList();
            var durationIndex = 0;


            var timeSignatures = events.Where(e => e is TimeSignatureEvent).Cast<TimeSignatureEvent>().ToList();
            var initialTimeSignature = timeSignatures.First();
            Time time = new Time();
            time.SetTimeSignature(initialTimeSignature.BeatsPerBar, initialTimeSignature.BeatValue);
            var remainingTimeSignatures = timeSignatures.Skip(1);


            foreach (var timeSignatureChange in timeSignatures)
            {
                time.SetTimeSignature(timeSignatureChange.BeatsPerBar, timeSignatureChange.BeatValue);
                while (durationIndex < durations.Count)
                {
                    var duration = durations[durationIndex];
                    durationIndex++;
                }
            }
            

            // no more time signature changes so process remaining notes
            while (durationIndex < durations.Count)
            {
                var duration = durations[durationIndex];
                int subDurationTick = duration.Tick;
                var remainingTime = duration.Duration;

                while (0 < remainingTime)
                {
                    var ties = time.GetBarlineTies(remainingTime);

                    if(duration is NoteWithDurationEvent note)
                    {
                        yield return new NoteWithDurationEvent(subDurationTick, ties[0], duration.Pitch, duration.Velocity, duration.Instrument);
                    } 
                    else if (duration is RestEvent rest)
                    {
                        yield return new RestEvent(subDurationTick, ties[0]);

                    }
                    
                    subDurationTick += ties[0];
                    remainingTime = ties[1];
                    time.AddTime(ties[0]);
                }

                durationIndex++;
            }
        }

        private IEnumerable<Event> SplitNotesByTimeSignature(ImmutableList<Event> events)
        {
            var timeSignatures = events.Where(e => e is TimeSignatureEvent).Cast<TimeSignatureEvent>().ToList();
            var timeSignatureIndex = 0;
            var durations = events.Where(e => e is DurationEvent).Cast<DurationEvent>().ToList();
            var durationIndex = 0;

            var hasMoreTimeSignatures = timeSignatureIndex < timeSignatures.Count;
            var currentTimeSignature = timeSignatures[timeSignatureIndex];
            var nextTimeSignature = (TimeSignatureEvent)null;
            // figure out when the next timesignature is

            if (hasMoreTimeSignatures)
            {
                timeSignatureIndex++;
                hasMoreTimeSignatures = timeSignatureIndex < timeSignatures.Count;
                nextTimeSignature = timeSignatures[timeSignatureIndex];
            }
            else
            {
                // return remaining notes
                foreach (var e in durations.Skip(durationIndex))
                {
                    yield return e;
                }
            }
        }

        private IEnumerable<Event> WithRestEvents(ImmutableList<Event> events)
        {
            var time = 0;
            RestEvent firstRest = null;
            for (int i = 0; i < events.Count; i++)
            {
                var @event = events[i];

                // get the easy stuff out of the way
                if (@event is TempoEvent)
                {
                    yield return @event;
                    continue;
                }
                else if (@event is TimeSignatureEvent timeSignatureEvent)
                {
                    // create rest as a multiple of the outgoing time signature
                    var restDuration = timeSignatureEvent.Tick - time;
                    if (0 < restDuration)
                    {
                        var rest = new RestEvent(time, restDuration);
                        if (firstRest == null)
                        {
                            firstRest = rest;
                        }
                        yield return rest;
                        time += restDuration;
                    }
                    //time += timeSignatureEvent.Duration;
                    yield return timeSignatureEvent;
                }
                else if (@event is NoteWithDurationEvent noteEvent)
                {
                    var restDuration = noteEvent.Tick - time;
                    if (0 < restDuration)
                    {
                        var rest = new RestEvent(time, restDuration);
                        if (firstRest == null)
                        {
                            firstRest = rest;
                        }
                        yield return rest;
                        time += restDuration;
                    }
                    time += noteEvent.Duration;
                    yield return noteEvent;
                }
                else if (@event is SongEndEvent songEnd)
                {
                    if (firstRest == null)
                    {
                        yield return new RestEvent(0, songEnd.Tick);
                    }
                }
                else
                {
                    Debug.Fail("boom");
                }
            }
        }

        private IEnumerable<Event> CollapseNoteEvents(List<Event> events)
        {
            for (int i = 0; i < events.Count; i++)
            {
                var @event = events[i];

                // get the easy stuff out of the way
                if (@event is TempoEvent || @event is TimeSignatureEvent)
                {
                    yield return @event;
                    continue;
                }

                if (@event is NoteEvent noteEvent)
                {
                    if (noteEvent.Type == NoteEvent.EventType.NoteOff)
                    {
                        Debug.Fail("got a note off event but don't have its note on event");
                        yield return @event;
                        continue;
                    }

                    // try to find the matching note end
                    var noteOffIndex = events.FindIndex(i, (e) => e is NoteEvent && ((NoteEvent)e).Type == NoteEvent.EventType.NoteOff);
                    Debug.Assert(0 <= noteOffIndex, "Could not find matching note off event");
                    var noteEnd = events[noteOffIndex];

                    var myNote = new NoteWithDurationEvent(noteEvent, (NoteEvent)noteEnd);
                    events.RemoveAt(noteOffIndex);
                    if (0 < myNote.Duration)
                    {
                        yield return myNote;
                    }
                    continue;
                }
            }
        }
    }
}