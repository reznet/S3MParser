using System.Collections.Immutable;
using S3M;

namespace S3mToMidi
{
    internal class NoteEventGenerator
    {
        private readonly NoteEventGeneratorOptions options;

        private readonly Func<int, OutputChannel> getNewOutputChannel;

        private int nextMidiChannel = 1;

        public NoteEventGenerator(NoteEventGeneratorOptions options, Func<int, OutputChannel> getNewOutputChannel)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.options = options;
            this.getNewOutputChannel = getNewOutputChannel;
        }
        public Dictionary<int, ImmutableList<Event>> Generate(S3MFile file)
        {
            const int TICKS_PER_QUARTERNOTE = 96;
            // a pattern has max 64 rows
            // tracker tempo is ALWAYS based on 6 ticks per row, 4 rows per beat = 24 ticks per beat
            // when speed = 6 and tempo = 60, 4 rows = 1 quarter note
            // speed = 24,1 row = quarter note (TICKS_PER_QUARTERNOTE / 1)
            // speed = 12,1 row = 8th note (TICKS_PER_QUARTERNOTE / 2)
            // speed = 6, 1 row = 16th note (TICKS_PER_QUARTERNOTE / 4)
            // speed = 5, 1 row = ??th note (TICKS_PER_QUARTERNOTE / (24 / 5))
            // speed = 4, 1 row = ?? th note (TICKS_PER_QUARTERNOTE / (24 / 4))
            // speed = 3, 1 row = 32nd note (TICKS_PER_QUARTERNOTE / 8)
            // speed = 2, 1 row = ??nd note (TICKS_PER_QUARTERNOTE / (24 / 2))
            // speed = 1, 1 row = ?? nd note (TICKS_PER_QUARTERNOTE / 24) [4 midi ticks]

            static int rowSpeedToTicks(int speed)
            {
                return speed == 0 ? speed : TICKS_PER_QUARTERNOTE * speed / 24;
            }

            Console.WriteLine("initial speed {0}", file.InitialSpeed);
            int speed = file.InitialSpeed;
            int tick = 0;
            int rowSkip = 0;
            int finalRow = 0;

            ChannelMultiplexer firstChannel = GetChannel(1);
            TempoEvent initialTempoEvent = new(tick, file.InitialTempo);
            bool hasAddedInitialTempo = false;

            Dictionary<int, int> lastInstrumentForChannel = new Dictionary<int, int>();
            Dictionary<int, HashSet<Channel>> trackerChannelsAndMidiChannels = new Dictionary<int, System.Collections.Generic.HashSet<Channel>>();

            Console.WriteLine("writing initial time signature event at tick {0}", tick);
            TimeSignatureEvent currentTimeSignatureEvent = new(tick, 4, 4);
            firstChannel.AddEvent(currentTimeSignatureEvent);

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

                int patternStartTick = tick;
                int rowIndex = rowSkip;
                int patternTrackerTicks = 0;
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

                        if (channelEvent.NoteAction == NoteAction.Start || channelEvent.HasInstrument)
                        {
                            int note = channelEvent.Note;
                            var delay = 0;

                            if (channelEvent.Command == CommandType.Notedelay)
                            {
                                delay = channelEvent.Data;
                            }
                            int time = tick + rowSpeedToTicks(delay);
                            if(channel.IsPlayingNote)
                            {
                                if(channelEvent.NoteAction == NoteAction.None)
                                {
                                    // get pitch from previous note start
                                    note = channel.PlayingPitch;
                                }
                                channel.NoteOff(time);
                            }
                            //int noteChannel = options.ChannelsFromPatterns ? channel.ChannelNumber : channel.Instrument - 1;
                            channel.NoteOn(time, channelEvent.Instrument.Value, note, channelEvent.Volume);
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

                    if (rowIndex == 0 && !hasAddedInitialTempo)
                    {
                        Console.WriteLine("NoteEventGenerator AddEvent InitialTempoEvent");
                        firstChannel.AddEvent(initialTempoEvent);
                        hasAddedInitialTempo = true;
                    }

                    tick += rowSpeedToTicks(speed);
                    patternTrackerTicks += speed;


                    if (breakPatternToRow)
                    {
                        // finished processing channels on this row
                        // now go to next pattern
                        break;
                    }


                }
                /*
                Console.WriteLine("Pattern {0} ending tracker ticks is {1} and midi ticks is {2}", pattern.PatternNumber, patternTrackerTicks, tick);

                Console.WriteLine("Pattern {0} is {1} tracker ticks long", pattern.PatternNumber, (float)patternTrackerTicks);
                Console.WriteLine("Pattern {0} is {1} quarter notes long", pattern.PatternNumber, patternTrackerTicks / 24.0);
                Console.WriteLine("Pattern {0} is {1} eighth notes long", pattern.PatternNumber, patternTrackerTicks / 12.0);
                Console.WriteLine("Pattern {0} is {1} 16th notes long", pattern.PatternNumber, patternTrackerTicks / 6.0);
                Console.WriteLine("Pattern {0} is {1} 32nd notes long", pattern.PatternNumber, patternTrackerTicks / 3.0);
                */
                // 32nd  - / 3.0
                // 96th - / 1.0
                // half notes / 48
                // whole notes / 96
                // every pattern tick is 24th of a quarter note
                // (1 quarter note / 24) * 64 = 

                int denominator = 0;
                int numerator = 0;
                int remainder = 0;
                int lastNumerator = 0;
                int lastDenominator = 0;
                int ticksPerDenominator = 0;
                for (int i = 0; i < 5; i++)
                {
                    ticksPerDenominator = 24 / (int)Math.Pow(2, i);
                    lastDenominator = (int)Math.Pow(2, i + 2);
                    int pow = (int)Math.Pow(2, i);
                    lastNumerator = patternTrackerTicks / ticksPerDenominator;
                    remainder = patternTrackerTicks % ticksPerDenominator;

                    // Console.WriteLine("Pattern {0} could be {1}/{2} with remainder {3}", pattern.PatternNumber, lastNumerator, lastDenominator, remainder);

                    if (remainder == 0)
                    {
                        denominator = lastDenominator;
                        numerator = lastNumerator;

                        // if numerator is a multiple of denominator
                        if (numerator % denominator == 0)
                        {
                            // assign the denominator to the numerator to simplify measures
                            // since measures don't exist in MIDI, there's no difference between
                            // 8/4 and 4/4, so let's go with the simplest time signature
                            numerator = denominator;
                        }
                        break;
                    }
                }

                if (0 < remainder)
                {
                    throw new Exception("The time signature in pattern {0} cannot be expressed with MIDI.");
                }

                //Console.WriteLine("Pattern {0} at {3} is time signature {1}/{2} and should end at {4}", pattern.PatternNumber, numerator, denominator, patternStartTick, tick);
                firstChannel.AddEvent(new TimeSignatureEvent(patternStartTick, numerator, denominator));
            }

            // finalize any leftover note on events
            foreach (var key in channels.Keys)
            {
                var channel = channels[key];
                if (channel.IsPlayingNote)
                {
                    channel.NoteOff(tick);
                }
            }

            Dictionary<int, ImmutableList<Event>> channelEvents = [];

            foreach (var outputChannel in channels.Values.SelectMany(c => c.OutputChannels))
            {
                channelEvents[outputChannel.ChannelNumber] = outputChannel.GetEvents();
            }

            return channelEvents;
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
                    getNewOutputChannel);
                channels.Add(channelNumber, value);
            }

            return value;
        }

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
