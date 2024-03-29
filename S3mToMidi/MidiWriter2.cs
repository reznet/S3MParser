using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System.Diagnostics;

namespace S3MParser
{
    static class MidiWriter2
    {
        private const int MAX_MIDI_CHANNEL = 16;
        public static void Save(Dictionary<int, List<Event>> allEvents, string path, MidiExportOptions exportOptions)
        {
            var channelLastTicks = new Dictionary<int, int>();
            for(int i = 0; i < MAX_MIDI_CHANNEL; i++){
                channelLastTicks[i] = 0;
            }

            List<TrackChunk> tracks = new List<TrackChunk>();
            foreach(var channelNumber in allEvents.Keys)
            {
                if (exportOptions.ExcludedChannels.Contains(channelNumber))
                {
                    Console.WriteLine("Excluding channel {0} from output midi file.", channelNumber);
                    continue;
                }
                List<Event> trackEvents = allEvents[channelNumber];
                var midiEvents = trackEvents
                    .Where(trackEvent => !exportOptions.ExcludedChannels.Contains(channelNumber))
                    .OrderBy(trackEvent => trackEvent.Tick)
                    .Select(trackEvent => new { Tick = trackEvent.Tick, MidiMessage = Convert(trackEvent, channelLastTicks) })
                    .Where(midiEvent => midiEvent.MidiMessage != null)
                    .Select(midiEvent => midiEvent.MidiMessage);
                
                TrackChunk track = new TrackChunk(midiEvents);
                tracks.Add(track);
            }

            MidiFile file = new MidiFile(tracks);

            file.Write(path, overwriteFile: true);
        }

        private static MidiEvent Convert(Event e, Dictionary<int, int> channelLastTicks)
        {
            if (e is NoteEvent)
            {
                NoteEvent note = (NoteEvent)e;

                // ignore channels beyond what MIDI supports
                if(MAX_MIDI_CHANNEL < note.Channel) 
                {
                    Console.WriteLine("Ignoring note event {0} because its MIDI channel is greater than the maximum allowed 16.", note);
                    return null; 
                }

                if (note.Type == NoteEvent.EventType.NoteOff)
                {
                    //Console.Out.WriteLine("Channel {0} NoteOff Pitch {1}", note.Channel, note.Pitch);
                    return new NoteOffEvent((SevenBitNumber)ChannelNoteToMidiPitch(note.Pitch), (SevenBitNumber)ChannelVelocityToMidiVolume(note.Velocity))
                    {
                        Channel = (FourBitNumber)note.Channel,
                        DeltaTime = GetDeltaTimeForChannelTick(note.Channel, note.Tick, channelLastTicks)
                    };
                }
                else
                {
                    //Console.Out.WriteLine("Channel {0} NoteOn Pitch {1}", note.Channel, note.Pitch);
                    return new NoteOnEvent((SevenBitNumber)ChannelNoteToMidiPitch(note.Pitch), (SevenBitNumber)ChannelVelocityToMidiVolume(note.Velocity))
                    {
                        Channel = (FourBitNumber)note.Channel,
                        DeltaTime = GetDeltaTimeForChannelTick(note.Channel, note.Tick, channelLastTicks)
                    };
                }
            }
            else if (e is TempoEvent)
            {
                
                
                var tempoEvent = (TempoEvent)e;
                Console.Out.WriteLine("TempoEvent Tick {0} Tempo {1} {2}", tempoEvent.Tick, tempoEvent.TempoBpm, 60000000 / tempoEvent.TempoBpm);
                
                return new SetTempoEvent(60000000 / tempoEvent.TempoBpm)
                {
                    // todo how to compute delta for tempo events - which channel?
                    // maybe a pseudo-channel for tempo?
                    DeltaTime = GetDeltaTimeForChannelTick(1, tempoEvent.Tick, channelLastTicks),
                    
                };
                
            }
            else if (e is TimeSignatureEvent)
            {
                /*
                Console.Out.WriteLine("Skipping TimeSignature event");
                return null;
                */

                
                var timeSignatureEvent = (TimeSignatureEvent)e;
                Console.WriteLine("TimeSignatureEvent Tick {0} {1}/{2}", timeSignatureEvent.Tick, timeSignatureEvent.BeatsPerBar, timeSignatureEvent.BeatValue);
                const byte ClocksPerMetronomeClick = 24;
                const byte ThirtySecondNotesPerQuarterNote = 8;
                return new Melanchall.DryWetMidi.Core.TimeSignatureEvent((byte)timeSignatureEvent.BeatsPerBar,
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
                return null;
            }
        }

        private static int GetDeltaTimeForChannelTick(int channel, int tick, Dictionary<int, int> channelLastTicks)
        {
            var lastTick = channelLastTicks[channel];
            var delta = tick - lastTick;
            channelLastTicks[channel] = tick;

            return delta ;
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
