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
            //File file = new File();
            //Sequence sequence = new Sequence();

            List<TrackChunk> tracks = new List<TrackChunk>();
            for (int trackIndex = 0; trackIndex < allEvents.Count; trackIndex++)
            {

                List<Event> trackEvents = allEvents[trackIndex];
                var midiEvents = trackEvents
                    .Select(trackEvent => new { Tick = trackEvent.Tick, MidiMessage = Convert(trackEvent) })
                    .Where(midiEvent => midiEvent.MidiMessage != null)
                    .Select(midiEvent => midiEvent.MidiMessage);
                
                TrackChunk track = new TrackChunk(midiEvents);
                //sequence.Add(track);
                tracks.Add(track);
            }

            MidiFile file = new MidiFile(tracks);

            //sequence.Save(path);
            file.Write(path);
        }

        private static MidiEvent Convert(Event e)
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
                    return new NoteOffEvent((SevenBitNumber)ChannelNoteToMidiPitch(note.Pitch), (SevenBitNumber)ChannelVelocityToMidiVolume(note.Velocity))
                    {
                        Channel = (FourBitNumber)note.Channel,
                        DeltaTime = note.Tick
                    };
                }
                else
                {
                    NoteOnEvent noteOnEvent = new NoteOnEvent((SevenBitNumber)ChannelNoteToMidiPitch(note.Pitch), (SevenBitNumber)ChannelVelocityToMidiVolume(note.Velocity));
                    noteOnEvent.Channel = (FourBitNumber)note.Channel;
                    noteOnEvent.DeltaTime = note.Tick;
                    return noteOnEvent;
                }
            }
            else if (e is TempoEvent)
            {
                var tempoEvent = (TempoEvent)e;
                return new SetTempoEvent(60000000 / tempoEvent.TempoBpm)
                {
                    DeltaTime = tempoEvent.Tick
                };
            }
            else if (e is TimeSignatureEvent)
            {
                var timeSignatureEvent = (TimeSignatureEvent)e;
                const byte ClocksPerMetronomeClick = 24;
                const byte ThirtySecondNotesPerQuarterNote = 8;
                return new Melanchall.DryWetMidi.Core.TimeSignatureEvent((byte)timeSignatureEvent.BeatsPerBar,
                                              (byte)timeSignatureEvent.BeatValue,
                                              ClocksPerMetronomeClick,
                                              ThirtySecondNotesPerQuarterNote)
                                              {
                                                DeltaTime = timeSignatureEvent.Tick
                                              };
            }
            else
            {
                Debug.Fail("unknown event type " + e.GetType().Name);
                return null;
            }
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
