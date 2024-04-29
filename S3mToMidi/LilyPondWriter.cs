using System.Collections.Immutable;
using System.Text;
using System.IO;
using System.Collections;
using S3M;

namespace S3mToMidi
{
    internal class LilyPondWriter
    {
        private readonly TextWriter writer = new StringWriter();
        private readonly NoteEventGeneratorOptions options;

        public LilyPondWriter(NoteEventGeneratorOptions options)
        {
            this.options = options;
        }

        public string Write(S3MFile file)
        {
            WriteVersion();
            WriteLanguage();

            const int TICKS_PER_QUARTERNOTE = 96;
            static int rowSpeedToTicks(int speed)
            {
                return speed == 0 ? speed : TICKS_PER_QUARTERNOTE * speed / 24;
            }

            int speed = file.InitialSpeed;
            int tick = 0;
            int rowSkip = 0;
            int finalRow = 0;

            ChannelMultiplexer firstChannel = GetChannel(1);
            TempoEvent initialTempoEvent = new(tick, file.InitialTempo);
            bool hasAddedInitialTempo = false;


            int takePatterns = int.MaxValue;
            int orderIndex = 0;
            foreach (var order in file.Orders.Skip(options.StartOrder ?? 0).Take(takePatterns))
            {
                orderIndex++;
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

                Console.WriteLine("Order {0} Pattern {1}", orderIndex, pattern.PatternNumber);

                int rowIndex = rowSkip;
                for (; rowIndex < pattern.Rows.Count; rowIndex++)
                {
                    //Console.WriteLine("Pattern {0} Row {1} patternTrackerTicks {2} trackerTicks {3}", pattern.PatternNumber, rowIndex, patternTrackerTicks, tick);
                    finalRow = rowIndex;
                    rowSkip = 0;
                    bool breakPatternToRow = false;
                    Row row = pattern.Rows[rowIndex];

                    foreach (var channelEvent in row.ChannelEvents)
                    {
                        ChannelMultiplexer channel = GetChannel(channelEvent.ChannelNumber);

                        if (channel.IsPlayingNote && channelEvent.HasVolume && channelEvent.Volume == 0)
                        {
                            //Console.WriteLine("Pattern {0} Channel {1} ending previous note at tick {2}", pattern.PatternNumber, channelEvent.ChannelNumber, tick);
                            channel.NoteOff(tick);
                        }

                        if (channelEvent.NoteAction == NoteAction.Start)
                        {
                            var delay = 0;

                            if (channelEvent.Command == CommandType.Notedelay)
                            {
                                delay = channelEvent.Data;
                            }
                            int time = tick + rowSpeedToTicks(delay);
                            if(channel.IsPlayingNote)
                            {
                                channel.NoteOff(time);
                            }
                            //int noteChannel = options.ChannelsFromPatterns ? channel.ChannelNumber : channel.Instrument - 1;
                            channel.NoteOn(time, channelEvent.Instrument.Value, channelEvent.Note, channelEvent.Volume);
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
                            Console.WriteLine("Pattern {0} speed is now {1}", pattern.PatternNumber, speed);
                        }
                        else if (channelEvent.Command == CommandType.SetTempo)
                        {
                            TempoEvent setTempoEvent = new(tick, channelEvent.Data);
                            if (!hasAddedInitialTempo)
                            {
                                initialTempoEvent = setTempoEvent;
                            }
                            else
                            {
                                Console.WriteLine("NoteEventGenerator AddEvent TempoEvent");
                                firstChannel.AddEvent(setTempoEvent);
                            }
                        }
                    }
                }
            }

            return writer.ToString();
        }

        private void WriteVersion()
        {
            writer.WriteLine("\\version \"2.24.3\"");
        }

        private void WriteLanguage()
        {
            writer.WriteLine("\\language \"english\"");
        }

        private SortedDictionary<int, ChannelMultiplexer> channels = []; 
        private ChannelMultiplexer GetChannel(int channelNumber)
        {
            const int DefaultVolume = 64;

            if (!channels.TryGetValue(channelNumber, out ChannelMultiplexer? value))
            {
                value = new ChannelMultiplexer(
                    channelNumber, 
                DefaultVolume, 
                options.ExcludedChannels.Contains(channelNumber), 
                GetNextAvailableMidiChannel, 
                options.ChannelInstrumentOutputBehavior == ChannelInstrumentOutputBehavior.Collapse ? GetChannelKeyForSharedOutputChannel : GetChannelKeyForSeparateOutputChannels,
                (_) => new LilyPondOutputChannel());
                channels.Add(channelNumber, value);
            }

            return value;
        }

        private int nextMidiChannel = 1;
        private int GetNextAvailableMidiChannel()
        {
            if (16 < nextMidiChannel)
            {
                throw new Exception("No more available midi channels.");
            }
            return nextMidiChannel++;
        }

        private int GetChannelKeyForSharedOutputChannel(int channelNumber, int _)
        {
            return channelNumber;
        }

        private int GetChannelKeyForSeparateOutputChannels(int channelNumber, int instrument)
        {
            // S3M supports max 100 instruments
            // channel and instrument numbers start at 1
            return (channelNumber - 1) << 8 | instrument;
        }
    }
}