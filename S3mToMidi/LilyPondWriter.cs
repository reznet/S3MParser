using System.Collections.Immutable;
using System.Text;
using System.IO;
using System.Collections;
using S3M;
using System.Diagnostics;
using Melanchall.DryWetMidi.Core;
using System.Net.Mail;

namespace S3mToMidi
{
    internal class LilyPondWriter
    {
        private readonly TextWriter writer;

        internal LilyPondWriter(TextWriter writer)
        {
            if(writer == null){ throw new ArgumentNullException(nameof(writer)); }
            this.writer = writer;
        }

        public string Write(Dictionary<int, ImmutableList<Event>> allEvents)
        {
            WriteVersion();
            WriteLanguage();

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
                    .ToList();
                var collapsedEvents = CollapseNoteEvents(sortedEvents).ToImmutableList();
                if(!collapsedEvents.Any())
                { 
                    Console.Out.WriteLine("Could not find any complete notes in channel {0}", channelNumber);
                    continue;
                }
                var withRests = WithRestEvents(collapsedEvents).ToImmutableList();

                writer.Write("\\new Staff ");
                
                writer.Write("{ ");

                // hack key signature
                WriteKeySignature("c", "minor");

                WriteClef("bass");

                var time = new Time();

                for(int i = 0; i < withRests.Count; i++)
                {
                    ProcessEvent(withRests, i, time);
                }
                writer.Write("}");
                writer.WriteLine();
            }

            writer.WriteLine(">>");

            return writer.ToString();
        }

        private class Time
        {
            public int Tick;

            public int[] GetBarlineTies(int duration)
            {
                //TODO: figure out or track the start of the measure
                // figure out which measure we're currently in, assume 4/4 for now
                var ticksPerMeasure = TICKS_PER_QUARTERNOTE * 4;
                var measure = Tick / ticksPerMeasure;
                var tickInMeasure = Tick % ticksPerMeasure;
                var ticksRemainingInMeasure = ticksPerMeasure - tickInMeasure;

                Console.Out.WriteLine("seems we're currently {0}/{1} ticks into measure {2}.  there are {3} ticks left in the measure.", tickInMeasure, ticksPerMeasure, measure, ticksRemainingInMeasure);

                Debug.Assert(0 < ticksRemainingInMeasure);
                Debug.Assert(0 < duration);

                if(duration <= ticksRemainingInMeasure)
                {
                    return new int[]{ duration };
                }
                else
                {
                    List<int> durations = new List<int>();
                    while(0 < duration)
                    {
                        var nextMaxDuration = ticksPerMeasure - tickInMeasure;  // the next duration cannot be longer than this value
                        var nextDuration = Math.Min(nextMaxDuration, duration);
                        durations.Add(nextDuration);
                        duration -= nextDuration;
                        tickInMeasure = (tickInMeasure + nextDuration) % ticksPerMeasure;
                    }
                    Console.Out.WriteLine("Split duration {0} across bar lines into [{1}]", duration, string.Join(", ", durations));
                    return durations.ToArray();
                }
            }
        }

        private IEnumerable<Event> WithRestEvents(ImmutableList<Event> events)
        {
            var time = 0;
            for(int i = 0; i < events.Count; i++)
            {
                var @event = events[i];

                // get the easy stuff out of the way
                if (@event is TempoEvent || @event is TimeSignatureEvent)
                {
                    yield return @event;
                    continue;
                } 
                if (@event is NoteWithDurationEvent noteEvent)
                {
                    var restDuration = noteEvent.Tick - time;
                    if (0 < restDuration)
                    {
                        yield return new RestEvent(time, restDuration);
                        time += restDuration;
                    }
                    time += noteEvent.Duration;
                    yield return noteEvent;
                }
                else
                {
                    Debug.Fail("boom");
                }
            }
        }

        private IEnumerable<Event> CollapseNoteEvents(List<Event> events)
        {
            for(int i = 0; i < events.Count; i++)
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
                    yield return myNote;
                    continue;
                }
            }
        }

        private abstract class DurationEvent : Event
        {
            public readonly int Duration;

            public abstract int Pitch { get; }
            public DurationEvent(int tick, int duration) : base(tick)
            {
                Duration = duration;
            }
        }

        private class RestEvent : DurationEvent
        {
            public RestEvent(int tick, int duration) : base(tick, duration)
            {
            }

            public override int Pitch => -1;
        }

        private class NoteWithDurationEvent : DurationEvent
        {
            public readonly NoteEvent NoteOn;
            public readonly NoteEvent NoteOff;

            public NoteWithDurationEvent(NoteEvent noteOn, NoteEvent noteOff) : base(noteOn.Tick, noteOff.Tick - noteOn.Tick)
            {
                NoteOn = noteOn;
                NoteOff = noteOff;
            }

            public override int Pitch => NoteOn.Pitch;

            public int Velocity => NoteOn.Velocity;
        }

        private void ProcessEvent(ImmutableList<Event> events, int eventIndex, Time time)
        {
            var e = events[eventIndex];
            if (e is DurationEvent myNote)
            {
                var durationTicks = myNote.Duration;
                Console.Out.WriteLine("Processing note duration {0}", durationTicks);

                for(int tupletDurationIndex = 0; tupletDurationIndex < TupletDurations.Count; tupletDurationIndex++)
                {
                    var tupletBaseDuration = TupletDurations[tupletDurationIndex].Item1;
                    if (durationTicks % tupletBaseDuration == 0 && (durationTicks / tupletBaseDuration) < 3)
                    {
                        // found a tuplet rhythm
                        Console.Out.WriteLine("Duration {0} appears to be part of a tuplet with base duration {1}", durationTicks, tupletBaseDuration);
                        var tupletNotes = new List<DurationEvent>();
                        tupletNotes.Add(myNote);

                        int tupletValue = durationTicks / TupletDurations[tupletDurationIndex].Item1;
                        int tupletDuration = (TupletDurations[tupletDurationIndex].Item1 * 3);
                        int remainingTupletDuration = tupletDuration - durationTicks;

                        for(int tupletIndex = eventIndex; tupletIndex < events.Count && tupletNotes.Sum(t => t.Tick) < tupletDuration; tupletIndex++)
                        {
                            if(events[tupletIndex] is DurationEvent nextNote)
                            {
                                if(nextNote.Duration %tupletBaseDuration != 0)
                                {
                                    //Debug.Fail("needed next note to be part of tuplet but it wasn't");
                                    break;
                                }
                                tupletNotes.Add(nextNote);
                            }
                        }

                        // emit tuplet
                        writer.WriteLine("\\tuplet 3/2 { ");

                        foreach(var tupletNote in tupletNotes)
                        {
                            //writer.WriteLine("\\set fontSize = #-{0}", (64 - tupletNote.NoteOn.Velocity) % (64 / 6));
                            var adjustedTupletDurationForDisplay = tupletNote.Duration * 3 / 2;
                            writer.WriteLine("{0}{1} ", ChannelNoteToLilyPondPitch(tupletNote.Pitch), ConvertToLilyPondDuration(adjustedTupletDurationForDisplay));
                        }
                        
                        writer.WriteLine(" }");

                        time.Tick += myNote.Duration;

                        return;
                    }
                }

                var durations = time.GetBarlineTies(myNote.Duration);
                time.Tick += myNote.Duration;

                if(myNote is RestEvent)
                {
                    foreach(var subDuration in durations)
                    {
                        var rests = GetNoteTies(subDuration);
                        foreach(var rest in rests)
                        {
                            writer.WriteLine("r{0} ", ConvertToLilyPondDuration(rest));
                        }
                    }
                }
                else if(myNote is NoteWithDurationEvent notWithDuration)
                {
                    writer.WriteLine("\\set fontSize = #-{0}", (64 - notWithDuration.Velocity) % (64 / 6));
                    writer.Write(ChannelNoteToLilyPondPitch(notWithDuration.Pitch));
                    writer.WriteLine(string.Join("~ ", durations.SelectMany(GetNoteTies).Select(ConvertToLilyPondDuration)));
                }
            }
            else if (e is NoteEvent note)
            {
                Debug.Fail("should have collapsed note events");
            }
            else if (e is TempoEvent tempoEvent)
            {
                //Console.Out.WriteLine("TempoEvent Tick {0} Tempo {1} {2}", tempoEvent.Tick, tempoEvent.TempoBpm, 60000000 / tempoEvent.TempoBpm);

/*
                yield return new SetTempoEvent(60000000 / tempoEvent.TempoBpm)
                {
                    // todo how to compute delta for tempo events - which channel?
                    // maybe a pseudo-channel for tempo?
                    DeltaTime = GetDeltaTimeForChannelTick(1, tempoEvent.Tick, channelLastTicks),

                };
*/
            }
            else if (e is TimeSignatureEvent timeSignatureEvent)
            {
                /*
                // Console.WriteLine("TimeSignatureEvent Tick {0} {1}/{2}", timeSignatureEvent.Tick, timeSignatureEvent.BeatsPerBar, timeSignatureEvent.BeatValue);
                const byte ClocksPerMetronomeClick = 24;
                const byte ThirtySecondNotesPerQuarterNote = 8;
                yield return new Melanchall.DryWetMidi.Core.TimeSignatureEvent((byte)timeSignatureEvent.BeatsPerBar,
                                              (byte)timeSignatureEvent.BeatValue,
                                              ClocksPerMetronomeClick,
                                              ThirtySecondNotesPerQuarterNote)
                {
                    // todo: how to compute delta for events that don't have a channel
                    DeltaTime = GetDeltaTimeForChannelTick(1, timeSignatureEvent.Tick, channelLastTicks)
                };
*/
            }
            else
            {
                Debug.Fail("unknown event type " + e.GetType().Name);
                // no-op
            }
        }

        private static int GetDurationForChannelTick(int channel, int tick, Dictionary<int, int> channelLastTicks)
        {
            var lastTick = channelLastTicks[channel];
            var delta = tick - lastTick;

            channelLastTicks[channel] = tick;

            return delta;
        }

        const int TICKS_PER_QUARTERNOTE = 96;

        private static List<(int, string)> LilyPondDurations = new List<(int, string)>
        {
            ( TICKS_PER_QUARTERNOTE * 4, "1" ),
            ( TICKS_PER_QUARTERNOTE * 3, "2." ),
            ( TICKS_PER_QUARTERNOTE * 2, "2" ),
            ( TICKS_PER_QUARTERNOTE * 15 / 8, "4~ 8~ 16." ),
            ( TICKS_PER_QUARTERNOTE * 3 / 2, "4." ),
            ( TICKS_PER_QUARTERNOTE, "4" ),
            ( TICKS_PER_QUARTERNOTE * 7 / 8, "8~ 16." ),
            ( TICKS_PER_QUARTERNOTE * 3 / 4, "8."),
            ( TICKS_PER_QUARTERNOTE / 2, "8" ),
            ( TICKS_PER_QUARTERNOTE * 5 / 4, "4~ 16" ),
            ( TICKS_PER_QUARTERNOTE * 5 / 8, "8~ 32" ),
            ( TICKS_PER_QUARTERNOTE * 3 / 8, "16." ),
            ( TICKS_PER_QUARTERNOTE / 4, "16" ),
            ( TICKS_PER_QUARTERNOTE / 8, "32" ),
            ( TICKS_PER_QUARTERNOTE / 16, "64" ),
        };

        private static List<(int, string)> LilyPondSimpleDurations = new List<(int, string)>
        {
            ( TICKS_PER_QUARTERNOTE * 4, "1" ),
            ( TICKS_PER_QUARTERNOTE * 2, "2" ),
            ( TICKS_PER_QUARTERNOTE * 1, "4" ),
            ( TICKS_PER_QUARTERNOTE / 2, "8" ),
            ( TICKS_PER_QUARTERNOTE / 4, "16" ),
            ( TICKS_PER_QUARTERNOTE / 8, "32" ),
            ( TICKS_PER_QUARTERNOTE / 16, "64" ),
            ( TICKS_PER_QUARTERNOTE / 32, "128" ),
        };

        private static int[] GetNoteTies(int delta)
        {
            Debug.Assert(delta <= TICKS_PER_QUARTERNOTE * 4, "delta is too large to fit in 4/4 measure"); // todo handle other time signatures
            List<int> parts = new List<int>();
            for(int i = 0; i < LilyPondSimpleDurations.Count; i++)
            {
                var (ticks, duration) = LilyPondSimpleDurations[i];
                if (ticks <= delta) 
                { 
                    parts.Add(ticks); 
                    delta -= ticks;
                }
            }

            return ((IEnumerable<int>)parts).Reverse().ToArray();
        }

        private static List<(int, string)>TupletDurations = new List<(int, string)>
        {
            ((int)(TICKS_PER_QUARTERNOTE / 1.5), "4" ), // 64 ticks, quarter note triplets
            (TICKS_PER_QUARTERNOTE / 3, "8" ), // 32 ticks, eighth note triplets
            (TICKS_PER_QUARTERNOTE / 6, "16" ), // 16 ticks, sixteenth note triplets
        };

        private static string ConvertToLilyPondDuration(int delta)
        {
            foreach(var (ticks, name) in LilyPondSimpleDurations)
            {
                if (delta == ticks)
                {
                    return name;
                }
            }
            Debug.Fail(string.Format("don't know how to convert duration {0} to LilyPond duration", delta));
            Console.Out.WriteLine("don't know how to convert duration {0} to LilyPond duration", delta);
            return "4";
            /*
            foreach(var (tupletDuration, lilyPondDuration) in TupletDurations)
            {
                if(tupletDuration == delta)
                {
                    return lilyPondDuration;
                }
            }
            */

            //return ConvertToLilyPondDuration2(delta);
            /*
            foreach(var (duration, lilyPondDuration) in LilyPondDurations)
            {
                if(delta % duration == 0)
                {
                    if(delta == duration)
                    {
                        return lilyPondDuration;
                    }

                    var multiple = delta / duration;

                }
            }
            
            //Debug.Fail(string.Format("don't know how to convert duration {0} to LilyPond duration", delta));
            Console.Out.WriteLine("don't know how to convert duration {0} to LilyPond duration", delta);
            return "4";
            */
        }

        private static string[] PitchNamesSharps = ["c", "c-sharp", "d", "d-sharp", "e", "f", "f-sharp", "g", "g-sharp", "a", "a-sharp", "b"];
        private static string[] PitchNamesFlats = ["c", "d-flat", "d", "e-flat", "e", "f", "g-flat", "g", "a-flat", "a", "b-flat", "b"];

        private static string ChannelNoteToLilyPondPitch(int note)
        {
            if(note == -1){ return "r"; }
            // C5 = 64 = octave 5 + step 0
            int step = note & 15;
            int octave = 1 + (note >> 4);
            string octaveName = "";

            if(5 < octave)
            {
                octaveName = new string('\'', octave - 5);
            }
            else if (5 == octave)
            {
                // no-op
            }
            else if (octave < 5)
            {
                octaveName = new string(',', 5 - octave);
            }

            return PitchNamesFlats[step] + octaveName;
        }

        private void WriteVersion()
        {
            writer.WriteLine("\\version \"2.24.3\"");
        }

        private void WriteLanguage()
        {
            writer.WriteLine("\\language \"english\"");
        }

        private void WriteClef(string clef)
        {
            writer.WriteLine("\\clef {0}", clef);
        }

        private void WriteKeySignature(string key, string mode)
        {
            writer.WriteLine("\\key {0} \\{1}", key, mode);
        }
    }
}