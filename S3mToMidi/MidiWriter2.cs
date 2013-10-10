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
        public static void Save(List<List<Event>> allEvents, string path)
        {
            Sequence sequence = new Sequence();

            Dictionary<int, Track> tracks = new Dictionary<int, Track>();

            for (int trackIndex = 0; trackIndex < allEvents.Count; trackIndex++)
            {
                Track track = new Track();
                sequence.Add(track);
                List<Event> trackEvents = allEvents[trackIndex];
                foreach (var midiEvent in trackEvents.Select(trackEvent => Convert(trackEvent, track)))
                {
                    track.Insert(midiEvent.AbsoluteTicks, midiEvent.MidiMessage);
                }
            }

            sequence.Save(path);
        }

        private static MidiEvent Convert(Event e, Track track)
        {
            Console.Out.WriteLine(e.ToString());
            
            if (e is NoteEvent)
            {
                NoteEvent note = (NoteEvent)e;
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
            else if (e is TempoEvent)
            {
                TempoEvent tempoEvent = (TempoEvent)e;
                TempoChangeBuilder builder = new TempoChangeBuilder();
                // convert BPM to microseconds
                builder.Tempo = 60000000 / tempoEvent.TempoBpm;
                builder.Build();
                return new MidiEvent(track, e.Tick, builder.Result);
            }
            else
            {
                Debug.Fail("unknown event type " + e.GetType().Name);
                return null;
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
