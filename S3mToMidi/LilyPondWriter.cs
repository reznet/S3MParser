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

            var channelLastTicks = new Dictionary<int, int>();

            foreach (var channelNumber in allEvents.Keys.Take(1))
            {
                var trackEvents = allEvents[channelNumber];
                var sortedEvents = trackEvents
                    .OrderBy(trackEvent => trackEvent.Tick)
                    .ToList();
                
                writer.Write("{ ");

                WriteClef("bass");

                var collapsedEvents = CollapseNoteEvents(sortedEvents).ToImmutableList();
                for(int i = 0; i < collapsedEvents.Count; i++)
                {
                    ProcessEvent(collapsedEvents, i);
                }
                writer.Write("}");
                writer.WriteLine();
            }

            return writer.ToString();
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

        private class NoteWithDurationEvent : Event
        {
            public readonly NoteEvent NoteOn;
            public readonly NoteEvent NoteOff;

            public NoteWithDurationEvent(NoteEvent noteOn, NoteEvent noteOff) : base(noteOn.Tick)
            {
                NoteOn = noteOn;
                NoteOff = noteOff;
            }

            public int GetDurationTicks()
            {
                return NoteOff.Tick - NoteOn.Tick;
            }
        }

        private void ProcessEvent(ImmutableList<Event> events, int eventIndex)
        {
            var e = events[eventIndex];
            if (e is NoteWithDurationEvent myNote)
            {
                var durationTicks = myNote.GetDurationTicks();
                Console.Out.WriteLine("Processing note duration {0}", durationTicks);

                for(int tupletDurationIndex = 0; tupletDurationIndex < TupletDurations.Count; tupletDurationIndex++)
                {
                    var tupletBaseDuration = TupletDurations[tupletDurationIndex].Item1;
                    if (durationTicks % tupletBaseDuration == 0 && (durationTicks / tupletBaseDuration) < 3)
                    {
                        // found a tuplet rhythm
                        Console.Out.WriteLine("Duration {0} appears to be part of a tuplet with base duration {1}", durationTicks, tupletBaseDuration);
                        var tupletNotes = new List<NoteWithDurationEvent>();
                        tupletNotes.Add(myNote);

                        int tupletValue = durationTicks / TupletDurations[tupletDurationIndex].Item1;
                        int tupletDuration = (TupletDurations[tupletDurationIndex].Item1 * 3);
                        int remainingTupletDuration = tupletDuration - durationTicks;

                        for(int tupletIndex = eventIndex; tupletIndex < events.Count && tupletNotes.Sum(t => t.Tick) < tupletDuration; tupletIndex++)
                        {
                            if(events[tupletIndex] is NoteWithDurationEvent nextNote)
                            {
                                if(nextNote.GetDurationTicks() %tupletBaseDuration != 0)
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
                            writer.WriteLine("\\set fontSize = #-{0}", (64 - tupletNote.NoteOn.Velocity) % (64 / 6));
                            writer.WriteLine("{0}{1} ", ChannelNoteToLilyPondPitch(tupletNote.NoteOn.Pitch), ConvertToLilyPondDuration(tupletNote.GetDurationTicks()));
                        }
                        
                        writer.WriteLine(" }");
                        return;
                    }
                }

                writer.WriteLine("\\set fontSize = #-{0}", (64 - myNote.NoteOn.Velocity) % (64 / 6));
                writer.WriteLine("{0}{1} ", ChannelNoteToLilyPondPitch(myNote.NoteOn.Pitch), ConvertToLilyPondDuration(myNote.GetDurationTicks()));
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
            ( TICKS_PER_QUARTERNOTE * 3 / 2, "4." ),
            ( TICKS_PER_QUARTERNOTE, "4" ),
            ( TICKS_PER_QUARTERNOTE * 3 / 4, "8."),
            ( TICKS_PER_QUARTERNOTE / 2, "8" ),
            ( TICKS_PER_QUARTERNOTE * 5 / 4, "4~ 16" ),
            ( TICKS_PER_QUARTERNOTE / 4, "16" ),
            ( TICKS_PER_QUARTERNOTE / 8, "32" ),
            ( TICKS_PER_QUARTERNOTE / 16, "64" ),
        };

        private static List<(int, string)>TupletDurations = new List<(int, string)>
        {
            ((int)(TICKS_PER_QUARTERNOTE / 1.5), "4" ), // 64 ticks, quarter note triplets
            (TICKS_PER_QUARTERNOTE / 3, "8" ), // 32 ticks, eighth note triplets
            (TICKS_PER_QUARTERNOTE / 6, "16" ), // 16 ticks, sixteenth note triplets
        };

        private static string ConvertToLilyPondDuration(int delta)
        {
            foreach(var (tupletDuration, lilyPondDuration) in TupletDurations)
            {
                if(tupletDuration == delta)
                {
                    return lilyPondDuration;
                }
            }
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
        }

        private static string[] PitchNames = ["c", "c-sharp", "d", "d-sharp", "e", "f", "f-sharp", "g", "g-sharp", "a", "a-sharp", "b"];

        private static string ChannelNoteToLilyPondPitch(int note)
        {
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

            return PitchNames[step] + octaveName;
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
    }
}