using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanford.Multimedia.Midi;

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

            foreach (var pattern in this.File.Patterns.Take(2))
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

                    //var converter = new CellConverter();
                    foreach (var midiEvent in Convert(patternStartTick, converter.Convert(Pattern.ConvertToCells(channel.ChannelEvents, pattern.Rows.Count, channel.ChannelNumber))))
                    {
                        track.Insert(midiEvent.AbsoluteTicks, midiEvent.MidiMessage);
                    }
                }
                //patternStartTick += pattern.Rows.Count * this.File.InitialSpeed;
            }
            /*
            Track track = new Track();
            sequence.Add(track);
            Pattern firstPattern = this.File.Patterns[0];

            var d = from a in this.File.Patterns
                    from b in a.EventsByChannel
                    where b.ChannelNumber == 2
                    select b;

            var e = from a in this.File.Patterns
                    from b in a.GetCells(2)
                    select b;

            var converter = new CellConverter();
            foreach (var midiEvent in Convert(converter.Convert(e)))
            {
                track.Insert(midiEvent.AbsoluteTicks, midiEvent.MidiMessage);
            }
            */
            sequence.Save(path);
        }

        ///// <summary>
        ///// Create a contiguous sequence of events from a sparse sequence
        ///// </summary>
        ///// <param name="events">The sparse event sequence.</param>
        ///// <param name="rows">The number of events that should be returned.</param>
        ///// <returns></returns>
        //private IEnumerable<ChannelEvent> Fill(IEnumerable<ChannelEvent> events, int eventCount)
        //{
        //    var enumerator = events.GetEnumerator();
        //    int sparseEventCount = 0;
        //    for (int eventIndex; eventIndex < eventCount; eventIndex++)
        //    {
        //        if (!enumerator.MoveNext())
        //        {
        //            break;
        //        }
        //        sparseEventCount++;

        //    }

        //    while (sparseEventCount < eventCount)
        //    {

        //    }
        //}

        

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
