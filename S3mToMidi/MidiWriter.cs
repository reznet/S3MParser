using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System.Collections.Immutable;
using System.Diagnostics;

namespace S3mToMidi
{
    internal static class MidiWriter
    {
        private const int MAX_MIDI_CHANNEL = 16;
        public static MidiFile Write(Dictionary<int, ImmutableList<Event>> allEvents)
        {
            var channelLastTicks = new Dictionary<int, int>();
            for (int i = 0; i < MAX_MIDI_CHANNEL; i++)
            {
                channelLastTicks[i] = 0;
            }

            List<TrackChunk> tracks = [];
            foreach (var channelNumber in allEvents.Keys)
            {
                var trackEvents = allEvents[channelNumber];
                var midiEvents = trackEvents
                    .OrderBy(trackEvent => trackEvent.Tick)
                    .SelectMany(trackEvent => Convert(trackEvent, channelLastTicks))
                    .Where(midiEvent => midiEvent != null)
                    .Select(midiEvent => midiEvent);

                TrackChunk track = new(midiEvents);
                tracks.Add(track);
            }

            MidiFile file = new(tracks);

            return file;
        }

        private static HashSet<int> initializedChannels = new HashSet<int>();

        private static IEnumerable<MidiEvent?> Convert(Event e, Dictionary<int, int> channelLastTicks)
        {
            if (e is NoteEvent note)
            {
                Console.WriteLine("Converting Tick {0} Channel {1} Instrument {2} Event {3}", note.Tick, note.Channel, note.Instrument, note.Type);

                // ignore channels beyond what MIDI supports
                if (MAX_MIDI_CHANNEL < note.Channel)
                {
                    Console.WriteLine("Ignoring note event {0} because its MIDI channel is greater than the maximum allowed 16.", note);
                    yield return null;
                }

                if (note.Type == NoteEvent.EventType.NoteOff)
                {
                    //Console.Out.WriteLine("Channel {0} NoteOff Pitch {1}", note.Channel, note.Pitch);
                    yield return new NoteOffEvent((SevenBitNumber)ChannelNoteToMidiPitch(note.Pitch), (SevenBitNumber)ChannelVelocityToMidiVolume(note.Velocity))
                    {
                        Channel = (FourBitNumber)(note.Channel - 1),
                        DeltaTime = GetDeltaTimeForChannelTick(note.Channel, note.Tick, channelLastTicks)
                    };
                }
                else
                {
                    if(!initializedChannels.Contains(note.Channel))
                    {
                        initializedChannels.Add(note.Channel);
                        yield return new ProgramChangeEvent(){
                            Channel = (FourBitNumber)(note.Channel - 1),
                            ProgramNumber = (SevenBitNumber)0,
                            DeltaTime = GetDeltaTimeForChannelTick(note.Channel, note.Tick, channelLastTicks)
                        };
                    }
                    //Console.Out.WriteLine("Channel {0} NoteOn Pitch {1}", note.Channel, note.Pitch);
                    yield return new NoteOnEvent((SevenBitNumber)ChannelNoteToMidiPitch(note.Pitch), (SevenBitNumber)ChannelVelocityToMidiVolume(note.Velocity))
                    {
                        Channel = (FourBitNumber)(note.Channel - 1),
                        DeltaTime = GetDeltaTimeForChannelTick(note.Channel, note.Tick, channelLastTicks)
                    };
                }
            }
            else if (e is TempoEvent tempoEvent)
            {
                //Console.Out.WriteLine("TempoEvent Tick {0} Tempo {1} {2}", tempoEvent.Tick, tempoEvent.TempoBpm, 60000000 / tempoEvent.TempoBpm);

                yield return new SetTempoEvent(60000000 / tempoEvent.TempoBpm)
                {
                    // todo how to compute delta for tempo events - which channel?
                    // maybe a pseudo-channel for tempo?
                    DeltaTime = GetDeltaTimeForChannelTick(1, tempoEvent.Tick, channelLastTicks),

                };

            }
            else if (e is TimeSignatureEvent timeSignatureEvent)
            {
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

            }
            else
            {
                Debug.Fail("unknown event type " + e.GetType().Name);
                yield return null;
            }
        }

        private static int GetDeltaTimeForChannelTick(int channel, int tick, Dictionary<int, int> channelLastTicks)
        {
            var lastTick = channelLastTicks[channel];
            var delta = tick - lastTick;

            channelLastTicks[channel] = tick;

            return delta;
        }

        private static int ChannelNoteToMidiPitch(int note)
        {
            // C5 = 64 = octave 5 + step 0
            int step = note & 15;
            int octave = 1 + (note >> 4);

            return (octave * 12) + step;
        }

        private static int ChannelVelocityToMidiVolume(int velocity)
        {
            return velocity == 0 ? 0 : (velocity * 2) - 1;
        }
    }
}
