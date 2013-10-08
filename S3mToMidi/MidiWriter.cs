using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanford.Multimedia.Midi;
using S3M;

namespace S3MParser
{
    class MidiWriter
    {
        private S3MFile File { get; set; }

        public MidiWriter(S3MFile file)
        {
            this.File = file;
        }

        internal void Save(string path)
        {
            Sequence sequence = new Sequence();

            Dictionary<int, Track> tracks = new Dictionary<int, Track>();
            Dictionary<int, CellConverter> converters = new Dictionary<int, CellConverter>();
            int patternStartTick = 0;

            foreach (var pattern in this.File.Patterns)
            {
                Console.Out.WriteLine("Writing pattern " + pattern.PatternNumber);
                foreach (var channel in pattern.Channels)
                {
                    Console.Out.WriteLine("Writing channel " + channel.ChannelNumber);
                    Track track;
                    CellConverter converter;
                    if (!tracks.Keys.Contains(channel.ChannelNumber))
                    {
                        track = new Track();
                        tracks.Add(channel.ChannelNumber, track);
                        sequence.Add(track);
                        converters.Add(channel.ChannelNumber, new CellConverter());
                    }
                    track = tracks[channel.ChannelNumber];
                    converter = converters[channel.ChannelNumber];

                    foreach (var midiEvent in Convert(patternStartTick, converter.Convert(Pattern.ConvertToCells(channel.ChannelEvents, pattern.Rows.Count, channel.ChannelNumber))))
                    {
                        track.Insert(midiEvent.AbsoluteTicks, midiEvent.MidiMessage);
                    }
                }
            }
            sequence.Save(path);
        }

        private IEnumerable<MidiEvent> Convert(int startTick, IEnumerable<NoteEvent> notes)
        {
            foreach (var note in notes)
            {
                Console.Out.WriteLine(note.ToString());
                if (note.Type == NoteEvent.EventType.NoteOff)
                {
                    ChannelMessageBuilder b = new ChannelMessageBuilder();
                    b.MidiChannel = note.Channel;
                    b.Command = ChannelCommand.NoteOff;
                    b.Data1 = note.Pitch;
                    b.Data2 = note.Velocity;
                    b.Build();
                    MidiEvent me = new MidiEvent(this, startTick + note.Tick, b.Result);
                    yield return me;
                }
                else
                {
                    ChannelMessageBuilder b = new ChannelMessageBuilder();
                    b.MidiChannel = note.Channel;
                    b.Command = ChannelCommand.NoteOn;
                    b.Data1 = note.Pitch;
                    b.Data2 = note.Velocity;
                    b.Build();
                    b.Build();
                    MidiEvent me = new MidiEvent(this, startTick + note.Tick, b.Result);
                    yield return me;
                }
            }
        }
    }
}
