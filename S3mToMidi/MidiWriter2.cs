using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3MParser
{
    static class MidiWriter2
    {
        private const int MAX_MIDI_CHANNEL = 16;
        public static void Save(List<List<Event>> allEvents, string path)
        {
            var channelLastTicks = new Dictionary<int, int>();
            for(int i = 0; i < MAX_MIDI_CHANNEL; i++){
                channelLastTicks[i] = 0;
            }

            List<TrackChunk> tracks = new List<TrackChunk>();
            for (int trackIndex = 0; trackIndex < allEvents.Count; trackIndex++)
            {

                List<Event> trackEvents = allEvents[trackIndex];
                var midiEvents = trackEvents
                    .OrderBy(trackEvent => trackEvent.Tick)
                    .Select(trackEvent => new { Tick = trackEvent.Tick, MidiMessage = Convert(trackEvent, channelLastTicks) })
                    .Where(midiEvent => midiEvent.MidiMessage != null)
                    .Select(midiEvent => midiEvent.MidiMessage);
                
                TrackChunk track = new TrackChunk(midiEvents);
                tracks.Add(track);
            }

            MidiFile file = new MidiFile(tracks);

            file.Write(path);
        }

        private static MidiEvent Convert(Event e, Dictionary<int, int> channelLastTicks)
        {
            if (e is NoteEvent)
            {
                NoteEvent note = (NoteEvent)e;

                // ignore channels beyond what MIDI supports
                if(MAX_MIDI_CHANNEL <= note.Channel) 
                {
                    Console.WriteLine("Ignoring note event {0} because its MIDI channel is greater than the maximum allowed 16.", note);
                    return null; 
                }

                if (note.Type == NoteEvent.EventType.NoteOff)
                {
                    Console.Out.WriteLine("Channel {0} NoteOff Pitch {1}", note.Channel, note.Pitch);
                    return new NoteOffEvent((SevenBitNumber)ChannelNoteToMidiPitch(note.Pitch), (SevenBitNumber)ChannelVelocityToMidiVolume(note.Velocity))
                    {
                        Channel = (FourBitNumber)note.Channel,
                        DeltaTime = GetDeltaTimeForChannelTick(note.Channel, note.Tick, channelLastTicks)
                    };
                }
                else
                {
                    Console.Out.WriteLine("Channel {0} NoteOn Pitch {1}", note.Channel, note.Pitch);
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

            int adjustedDelta = delta * 4;
            adjustedDelta = delta * 4;// todo: why does multiplying by 4 work?
            Console.Out.WriteLine("Channel {0} Tick {1} Delta {2} Adj {3}", channel, tick, delta, adjustedDelta);
            return adjustedDelta;
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
