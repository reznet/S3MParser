using S3M;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3MParser
{
    static class NoteEventGenerator
    {
        public static List<List<Event>> Generate(S3MFile file, NoteEventGeneratorOptions options)
        {
            Dictionary<int, Channel> channels = new Dictionary<int, Channel>();

            Console.WriteLine("initial speed {0}", file.InitialSpeed);
            int speed = file.InitialSpeed;
            int tick = 0;
            int rowSkip = 0;

            Channel firstChannel = GetChannel(channels, 1);
            TempoEvent tempoEvent = new TempoEvent(tick, file.InitialTempo);
            firstChannel.AddNoteEvent(tempoEvent);

            TimeSignatureEvent currentTimeSignatureEvent = new TimeSignatureEvent(tick, 4, 4);
            firstChannel.AddNoteEvent(currentTimeSignatureEvent);

            foreach (var order in file.Orders)
            {
                if (order == 255)
                {
                    break;
                }

                var pattern = file.Patterns[order];
                int patternStartTick = tick;
                int rowIndex = rowSkip;
                for (; rowIndex < pattern.Rows.Count; rowIndex++)
                {
                    rowSkip = 0;
                    bool breakPatternToRow = false;
                    Row row = pattern.Rows[rowIndex];

                    foreach (var channelEvent in row.ChannelEvents)
                    {
                        Channel channel = GetChannel(channels, channelEvent.ChannelNumber);

                        bool needNoteOff = channel.IsPlayingNote && channelEvent.NoteAction != NoteAction.None;

                        if (needNoteOff)
                        {
                            channel.AddNoteEvent(GenerateNoteOffEvent(channel, tick));
                        }

                        if (channelEvent.NoteAction == NoteAction.Start)
                        {
                            var delay = 0;

                            if (pattern.PatternNumber == 8 && channelEvent.Command != CommandType.None)
                            {
                                Console.WriteLine("Pattern {0} Row {1} Channel {2} {3}", pattern.PatternNumber, row.RowNumber, channelEvent.ChannelNumber, channelEvent.Command);
                            }

                            if (channelEvent.Command == CommandType.Notedelay)
                            {
                                if (208 <= channelEvent.Data && channelEvent.Data <= 214)  // HACK: 214 is a guess at the top range of SD commands
                                {
                                    delay = channelEvent.Data - 208;
                                    Debug.Assert(delay >= 0);
                                }
                            }
                            channel.AddNoteEvent(GenerateNoteOnEvent(channel, channelEvent, tick + delay, options));
                        }

                        if (channelEvent.Data != 0)
                        {
                            channel.Data = channelEvent.Data;
                        }

                        if (channelEvent.Command == CommandType.BreakPatternToRow)
                        {
                            breakPatternToRow = true;
                            rowSkip = channelEvent.Data;

                            break;
                        }
                        else if (channelEvent.Command == CommandType.SetSpeed)
                        {
                            speed = channelEvent.Data;
                        }
                    }

                    tick += speed;

                    if (breakPatternToRow)
                    {
                        // just finished last row in this pattern
                        // because we are jumping to a new pattern
                        var modulo = (rowIndex + 1) % 32;
                        if (modulo != 0)
                        {
                            // TODO: rework time signature changes
                            // we need to figure out what the initial time signature is
                            // and return to it and not just assume 4/4
                            // alternatively, for each pattern, figure out the key signature
                            // and insert it at the start tick if it's different from the previous pattern

                            // 8 only happens to work because we've been assuming the song was in 4/4
                            // a counter example is v-option which is in a compound meter
                            var m8 = (rowIndex + 1) % 8;
                            if (m8 == 0)
                            {
                                firstChannel.AddNoteEvent(new TimeSignatureEvent(patternStartTick, (rowIndex + 1) / 8, 4));
                                firstChannel.AddNoteEvent(new TimeSignatureEvent(tick, 4, 4));
                            }

                        }

                        // now go to next pattern
                        break;
                    }
                }
            }

            // finalize any leftover note on events
            foreach (var key in channels.Keys)
            {
                Channel channel = channels[key];
                if (channel.CurrentNote != null)
                {
                    channel.AddNoteEvent(GenerateNoteOffEvent(channel, tick));
                }
            }

            List<List<Event>> allEvents = new List<List<Event>>();
            foreach (var key in channels.Keys.OrderBy(i => i))
            {
                allEvents.Add(channels[key].NoteEvents);
            }
            return allEvents;
        }

        private static NoteEvent GenerateNoteOnEvent(Channel channel, ChannelEvent channelEvent, int tick, NoteEventGeneratorOptions options)
        {
            int volume = channelEvent.HasVolume ? channelEvent.Volume : channel.DefaultVolume;
            channel.Instrument = channelEvent.HasInstrument ? channelEvent.Instrument : channel.Instrument;
            int noteChannel = options.ChannelsFromPatterns ? channelEvent.ChannelNumber : channel.Instrument - 1;
            NoteEvent noteOnEvent = new NoteEvent(tick, NoteEvent.EventType.NoteOn, noteChannel, channelEvent.Note, volume);
            channel.CurrentNote = noteOnEvent;
            return noteOnEvent;
        }

        private static NoteEvent GenerateNoteOffEvent(Channel channel, int tick)
        {
            NoteEvent offEvent = channel.CurrentNote.Clone(tick);
            offEvent.Type = NoteEvent.EventType.NoteOff;
            channel.CurrentNote = null;
            return offEvent;
        }

        private static Channel GetChannel(Dictionary<int, Channel> channels, int channelNumber)
        {
            if(!channels.ContainsKey(channelNumber))
            {
                Console.WriteLine("Initializing channel {0}", channelNumber);
                channels.Add(channelNumber, new Channel()
                    {
                        DefaultVolume = 64,
                    });
            }
            return channels[channelNumber];
        }

        private class Channel
        {
            public NoteEvent CurrentNote;
            public int DefaultVolume;
            public int Instrument;
            public int Data;
            public List<Event> NoteEvents = new List<Event>();

            public bool IsPlayingNote { get { return CurrentNote != null; } }

            public void AddNoteEvent(Event noteEvent)
            {
                NoteEvents.Add(noteEvent);
            }
        }
    }
}
