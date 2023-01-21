using Sanford.Multimedia.Midi;
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
            Sequence sequence = new Sequence();

            Dictionary<int, Track> tracks = new Dictionary<int, Track>();

            for (int trackIndex = 0; trackIndex < allEvents.Count; trackIndex++)
            {
                Track track = new Track();
                sequence.Add(track);
                List<Event> trackEvents = allEvents[trackIndex];
                foreach (var midiEvent in trackEvents.Select(trackEvent => new { Tick = trackEvent.Tick, MidiMessage = Convert(trackEvent, track) }).Where(midiEvent => midiEvent.MidiMessage != null))
                {
                    track.Insert(midiEvent.Tick, midiEvent.MidiMessage);
                }
            }

            sequence.Save(path);
        }

        private static IMidiMessage Convert(Event e, Track track)
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
                    ChannelMessageBuilder b = new ChannelMessageBuilder();
                    b.MidiChannel = note.Channel;
                    b.Command = ChannelCommand.NoteOff;
                    b.Data1 = ChannelNoteToMidiPitch(note.Pitch);
                    b.Data2 = ChannelVelocityToMidiVolume(note.Velocity);
                    b.Build();
                    return b.Result;
                }
                else
                {
                    ChannelMessageBuilder b = new ChannelMessageBuilder();
                    b.MidiChannel = note.Channel;
                    b.Command = ChannelCommand.NoteOn;
                    b.Data1 = ChannelNoteToMidiPitch(note.Pitch);
                    b.Data2 = ChannelVelocityToMidiVolume(note.Velocity);
                    b.Build();
                    return b.Result;
                }
            }
            else if (e is TempoEvent)
            {
                TempoEvent tempoEvent = (TempoEvent)e;
                TempoChangeBuilder builder = new TempoChangeBuilder();
                // convert BPM to microseconds
                builder.Tempo = 60000000 / tempoEvent.TempoBpm;
                builder.Build();
                return builder.Result;
            }
            else if (e is TimeSignatureEvent)
            {
                TimeSignatureEvent timeSignatureEvent = (TimeSignatureEvent)e;
                TimeSignatureBuilder builder = new TimeSignatureBuilder();
                builder.Numerator = (byte)timeSignatureEvent.BeatsPerBar;
                builder.Denominator = (byte)timeSignatureEvent.BeatValue;
                builder.ClocksPerMetronomeClick = 24;
                builder.ThirtySecondNotesPerQuarterNote = 8;
                builder.Build();
                return builder.Result;
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
