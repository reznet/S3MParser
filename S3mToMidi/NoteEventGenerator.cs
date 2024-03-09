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
            int finalRow = 0;

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

                if (options.Pattern.HasValue && options.Pattern.Value != pattern.PatternNumber)
                {
                    Console.WriteLine("Skipping pattern {0} because pattern filter \"{1}\" was specified.", pattern.PatternNumber, options.Pattern.Value);
                    continue;
                }

                int patternStartTick = tick;
                int rowIndex = rowSkip;
                for (; rowIndex < pattern.Rows.Count; rowIndex++)
                {
                    finalRow = rowIndex;
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
                        // finished processing channels on this row
                        // now go to next pattern
                        break;
                    }
                }

                // add 1 because our row index is 0 based, so the last row is 63
                int numerator = 1 + finalRow;
                int denominator = pattern.Rows.Count;
                // TODO: figure out how to avoid wacky time signatures like 1/1
                while (numerator % 2 == 0 && denominator % 2 == 0)
                {
                    denominator = denominator / 2;
                    numerator = numerator / 2;
                }
                if (1 == numerator && numerator == denominator)
                {
                    numerator = 4;
                    denominator = 4;
                }
                Console.WriteLine("Pattern {0} is time signature {1}/{2}", pattern.PatternNumber, numerator, denominator);
                firstChannel.AddNoteEvent(new TimeSignatureEvent(patternStartTick, numerator, denominator));
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
            if (!channels.ContainsKey(channelNumber))
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
