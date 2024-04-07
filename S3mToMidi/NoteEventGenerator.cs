using S3M;

namespace S3mToMidi
{
    internal static class NoteEventGenerator
    {
        public static Dictionary<int, List<Event>> Generate(S3MFile file, NoteEventGeneratorOptions options)
        {
            const int TICKS_PER_QUARTERNOTE = 96;
            // a pattern has max 64 rows
            // tracker tempo is ALWAYS based on 6 ticks per row, 4 rows per beat = 24 ticks per beat
            // when speed = 6 and tempo = 60, 4 rows = 1 quarter note
            // speed = 3, 4 rows = 8th note
            // speed = 12, 1 row = 8th note
            // speed = 24, 1 row = quarter note
            // real lookup table:
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

            SortedDictionary<int, Channel> channels = [];

            Console.WriteLine("initial speed {0}", file.InitialSpeed);
            int speed = file.InitialSpeed;
            int tick = 0;
            int rowSkip = 0;
            int finalRow = 0;

            Channel firstChannel = GetChannel(channels, 1);
            TempoEvent initialTempoEvent = new(tick, file.InitialTempo);
            bool hasAddedInitialTempo = false;

            Dictionary<int, int> lastInstrumentForChannel = new Dictionary<int, int>();
            Dictionary<int, HashSet<Channel>> trackerChannelsAndMidiChannels = new Dictionary<int, System.Collections.Generic.HashSet<Channel>>();

            TimeSignatureEvent currentTimeSignatureEvent = new(tick, 4, 4);
            firstChannel.AddNoteEvent(currentTimeSignatureEvent);

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
                    //Console.WriteLine("Pattern {0} Row {1} patternTrackerTicks {2}", pattern.PatternNumber, rowIndex, patternTrackerTicks);
                    finalRow = rowIndex;
                    rowSkip = 0;
                    bool breakPatternToRow = false;
                    Row row = pattern.Rows[rowIndex];

                    foreach (var channelEvent in row.ChannelEvents)
                    {
                        if (channelEvent.ChannelNumber > 2){ continue;}
                        int instrument = channelEvent.Instrument;
                        if (!channelEvent.HasInstrument && lastInstrumentForChannel.ContainsKey(channelEvent.ChannelNumber)){
                            instrument = lastInstrumentForChannel[channelEvent.ChannelNumber];
                        }

                        int virtualChannelNumber = (channelEvent.ChannelNumber * 10) + instrument;
                        Channel channel = GetChannel(channels, virtualChannelNumber);

                        if (!trackerChannelsAndMidiChannels.ContainsKey(channelEvent.ChannelNumber))
                        {
                            trackerChannelsAndMidiChannels[channelEvent.ChannelNumber] = new HashSet<Channel>();
                        }

                        if (!trackerChannelsAndMidiChannels[channelEvent.ChannelNumber].Contains(channel)){
                            trackerChannelsAndMidiChannels[channelEvent.ChannelNumber].Add(channel);
                        }

                        if (channelEvent.HasInstrument){
                            lastInstrumentForChannel[channelEvent.ChannelNumber] = channelEvent.Instrument;
                        }

                        if (channel.IsPlayingNote && channelEvent.HasVolume && channelEvent.Volume == 0)
                        {
                            //Console.WriteLine("Pattern {0} Channel {1} ending previous note at tick {2}", pattern.PatternNumber, channelEvent.ChannelNumber, tick);
                            channel.AddNoteEvent(GenerateNoteOffEvent(channel, tick));
                        }

                        if (channelEvent.NoteAction == NoteAction.Start)
                        {
                            var delay = 0;

                            if (channelEvent.Command == CommandType.Notedelay)
                            {
                                delay = channelEvent.Data;
                            }
                            foreach(var virtualChannel in trackerChannelsAndMidiChannels[channelEvent.ChannelNumber]){
                                if (virtualChannel.IsPlayingNote){
                                    virtualChannel.AddNoteEvent(GenerateNoteOffEvent(virtualChannel, tick + rowSpeedToTicks(delay)));
                                }
                            }
                            channel.AddNoteEvent(GenerateNoteOnEvent(channel, channelEvent, tick + rowSpeedToTicks(delay), options));
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
                                firstChannel.AddNoteEvent(setTempoEvent);
                            }
                        }
                    }

                    if (rowIndex == 0 && !hasAddedInitialTempo)
                    {
                        firstChannel.AddNoteEvent(initialTempoEvent);
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

                    Console.WriteLine("Pattern {0} could be {1}/{2} with remainder {3}", pattern.PatternNumber, lastNumerator, lastDenominator, remainder);

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

                Console.WriteLine("Pattern {0} at {3} is time signature {1}/{2} and should end at {4}", pattern.PatternNumber, numerator, denominator, patternStartTick, tick);
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

            Dictionary<int, List<Event>> channelEvents = [];

            foreach (var key in channels.Keys.OrderBy(i => i))
            {
                channelEvents[key] = channels[key].NoteEvents;
            }

            return channelEvents;
        }

        private static NoteEvent GenerateNoteOnEvent(Channel channel, ChannelEvent channelEvent, int tick, NoteEventGeneratorOptions options)
        {
            int volume = channelEvent.HasVolume ? channelEvent.Volume : channel.DefaultVolume;
            channel.Instrument = channelEvent.HasInstrument ? channelEvent.Instrument : channel.Instrument;
            int noteChannel = options.ChannelsFromPatterns ? channel.ChannelNumber : channel.Instrument - 1;
            NoteEvent noteOnEvent = new(tick, NoteEvent.EventType.NoteOn, noteChannel, channelEvent.Note, volume);
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

        static Dictionary<int, int> virtualChannels = new Dictionary<int, int>();
        private static Channel GetChannel(SortedDictionary<int, Channel> channels, int channelNumber)
        {
            // virtual channel (35) -> midi channel 2 -> channel 2
            if (virtualChannels.ContainsKey(channelNumber)){
                return channels[virtualChannels[channelNumber]];
            } else {
                int nextMidiChannel = virtualChannels.Count + 1;

                Console.WriteLine("Initializing virtual channel {0} with midi channel {1}", channelNumber, nextMidiChannel);
                Channel newChannel = new Channel()
                {
                    ChannelNumber = nextMidiChannel,
                    DefaultVolume = 64,
                };

                virtualChannels[channelNumber] = nextMidiChannel;
                channels[nextMidiChannel] = newChannel;
                return newChannel;
            }

/*
            if (!channels.TryGetValue(channelNumber, out Channel? value))
            {
                channels.Add(channelNumber, value);
            }
            return value;
            */
        }

        private class Channel
        {
            public int ChannelNumber { get; set; }
            public NoteEvent? CurrentNote;
            public int DefaultVolume;
            public int Instrument;
            public int Data;
            public List<Event> NoteEvents = [];

            public bool IsPlayingNote => CurrentNote != null;

            public void AddNoteEvent(Event noteEvent)
            {
                NoteEvents.Add(noteEvent);
            }
        }
    }
}
