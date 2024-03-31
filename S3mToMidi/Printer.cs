using S3M;

namespace S3mToMidi
{
    internal class Printer
    {
        private static readonly string[] Pitches = ["C-", "C#", "D-", "D#", "E-", "F-", "F#", "G-", "G#", "A-", "A#", "B-"];

        public static void PrintPatterns(IEnumerable<Pattern> patterns)
        {
            foreach (Pattern pattern in patterns)
            {
                Console.Out.WriteLine("Pattern " + pattern.PatternNumber);
                HashSet<int> channels = [];
                foreach (Row row in pattern.Rows)
                {
                    foreach (ChannelEvent channelEvent in row.ChannelEvents)
                    {
                        if (!channels.Contains(channelEvent.ChannelNumber))
                        {
                            _ = channels.Add(channelEvent.ChannelNumber);
                        }
                    }
                }
                List<int> channelHeaders = new(channels.ToList());
                channelHeaders.Sort();

                foreach (int channelHeader in channelHeaders)
                {
                    Console.Out.Write("Channel {0,5}|", channelHeader);
                }
                Console.Out.WriteLine();

                foreach (Row row in pattern.Rows)
                {
                    int channelIndex = 0;
                    for (int cIndex = 0; cIndex < row.ChannelEvents.Count; cIndex++)
                    {
                        ChannelEvent evt = row.ChannelEvents[cIndex];
                        while (channelIndex < channelHeaders.Count && channelHeaders[channelIndex] < evt.ChannelNumber)
                        {
                            Console.Out.Write("___ __ __ ___|");
                            channelIndex++;
                        }

                        Console.Out.Write("{0} {1} {2} {3}|", NoteValueToString(evt.Note), InstrumentToString(evt.Instrument), VolumeToString(evt.Volume), CommandValueToString(evt.Command, evt.Data));
                        channelIndex++;
                    }
                    while (channelIndex < channelHeaders.Count)
                    {
                        Console.Out.Write("___ __ __ ___|");
                        channelIndex++;
                    }
                    Console.Out.WriteLine();
                }

                Console.Out.WriteLine();
                Console.Out.WriteLine();

            }
        }

        private static object VolumeToString(int volume)
        {
            return volume == -1 ? "  " : (object)volume.ToString("00");
        }

        private static object InstrumentToString(int instrument)
        {
            return instrument == -1 ? "  " : (object)instrument.ToString("X2");
        }

        private static string NoteValueToString(int note)
        {
            if (note == -1) { return "   "; }
            if (note == 0xFF) { return "nil"; }
            if (note == 0xFE) { return "off"; }
            int step = note & 15;
            int octave = note >> 4;

            return Pitches[step] + (octave + 1).ToString();
        }

        private static string CommandValueToString(CommandType command, int data)
        {
            return command == CommandType.None ? "   " : ((char)('A' + ((int)command - 1))).ToString() + data.ToString("X2");
        }
    }
}
