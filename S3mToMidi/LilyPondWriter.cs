using System.Collections.Immutable;
using System.Text;
using System.IO;
using System.Collections;
using S3M;
using System.Diagnostics;

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
                    .OrderBy(trackEvent => trackEvent.Tick);
                
                writer.Write("{ ");
                foreach(var e in sortedEvents)
                {
                    ProcessEvent(e, channelLastTicks);
                }
                writer.Write("}");
                writer.WriteLine();
            }

            return writer.ToString();
        }

        private void ProcessEvent(Event e, Dictionary<int, int> channelLastTicks)
        {
            if (e is NoteEvent note)
            {
                //Console.WriteLine("Converting Tick {0} Channel {1} Instrument {2} Event {3}", note.Tick, note.Channel, note.Instrument, note.Type);

                if(!channelLastTicks.ContainsKey(note.Channel))
                {
                    channelLastTicks.Add(note.Channel, 0);
                }

                if (note.Type == NoteEvent.EventType.NoteOff)
                {
                    writer.Write("{0}{1} ", ChannelNoteToLilyPondPitch(note.Pitch), ConvertToLilyPondDuration(GetDurationForChannelTick(note.Channel, note.Tick, channelLastTicks)));
                }
                else
                {
                    GetDurationForChannelTick(note.Channel, note.Tick, channelLastTicks);
                }
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

        private static Dictionary<int, string> LilyPondDurations = new Dictionary<int, string>
        {
            { TICKS_PER_QUARTERNOTE / 16, "64" },
            { TICKS_PER_QUARTERNOTE / 8, "32" },
            { TICKS_PER_QUARTERNOTE / 4, "16" },
            { TICKS_PER_QUARTERNOTE / 2, "8" },
            { TICKS_PER_QUARTERNOTE, "4" },
            { TICKS_PER_QUARTERNOTE * 2, "2" },
            { TICKS_PER_QUARTERNOTE * 4, "1" },
        };

        private static string ConvertToLilyPondDuration(int delta)
        {
            if(!LilyPondDurations.ContainsKey(delta))
            {
                Debug.Fail(string.Format("don't know how to convert duration {0} to LilyPond duration", delta));
                return "4";
            }

            return LilyPondDurations[delta];
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
    }
}