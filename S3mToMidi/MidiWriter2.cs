using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3MParser
{
    static class MidiWriter2
    {
        public static void Save(List<List<NoteEvent>> allEvents, string path)
        {
            Sequence sequence = new Sequence();

            Dictionary<int, Track> tracks = new Dictionary<int, Track>();

            for (int trackIndex = 0; trackIndex < allEvents.Count; trackIndex++)
            {
                Track track = new Track();
                sequence.Add(track);
                List<NoteEvent> trackEvents = allEvents[trackIndex];
                foreach (var midiEvent in trackEvents.Select(trackEvent => Convert(trackEvent, track)))
                {
                    track.Insert(midiEvent.AbsoluteTicks, midiEvent.MidiMessage);
                }
            }

            sequence.Save(path);
        }

        private static MidiEvent Convert(NoteEvent note, Track track)
        {
            Console.Out.WriteLine(note.ToString());
            if (note.Type == NoteEvent.EventType.NoteOff)
            {
                ChannelMessageBuilder b = new ChannelMessageBuilder();
                b.MidiChannel = note.Channel;
                b.Command = ChannelCommand.NoteOff;
                b.Data1 = ChannelNoteToMidiPitch(note.Pitch);
                b.Data2 = ChannelVelocityToMidiVolume(note.Velocity);
                b.Build();
                MidiEvent me = new MidiEvent(track, note.Tick, b.Result);
                return me;
            }
            else
            {
                ChannelMessageBuilder b = new ChannelMessageBuilder();
                b.MidiChannel = note.Channel;
                b.Command = ChannelCommand.NoteOn;
                b.Data1 = ChannelNoteToMidiPitch(note.Pitch);
                b.Data2 = ChannelVelocityToMidiVolume(note.Velocity);
                b.Build();
                MidiEvent me = new MidiEvent(track, note.Tick, b.Result);
                return me;
            }
        }

        private static int ChannelNoteToMidiPitch(int note)
        {
            int step = note & 15;
            int octave = note >> 4;

            return (octave * 12) + step;
        }

        private static int ChannelVelocityToMidiVolume(int velocity)
        {
            return velocity == 0 ? 0 : (velocity * 2) - 1;
        }
    }
}
