using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace S3MParser
{
    public static class TextWriterExtensions
    {
        public static void WriteS3M(this TextWriter writer, S3MFile file)
        {
            PrintS3MFile(file);
            PrintPatterns(file.Patterns);
        }

        static char[] Notes = { 'C', 'D', 'D', 'E', 'E', 'F', 'G', 'G', 'A', 'A', 'B', 'B' };
        static int[] Alter = { 0, -1, 0, -1, 0, 0, -1, 0, -1, 0, -1, 0 };

        static string[] Pitches = { "C-", "C#", "D-", "D#", "E-", "F-", "F#", "G-", "G#", "A-", "A#", "B-" };

        private static void PrintPatterns(IEnumerable<Pattern> patterns)
        {
            foreach (Pattern pattern in patterns)
            {
                Console.Out.WriteLine("Pattern " + pattern.PatternNumber);
                HashSet<int> channels = new HashSet<int>();
                foreach (Row row in pattern.Rows)
                {
                    foreach (ChannelEvent channelEvent in row.ChannelEvents)
                    {
                        if (channelEvent == null) continue;
                        if (!channels.Contains(channelEvent.ChannelNumber))
                        {
                            channels.Add(channelEvent.ChannelNumber);
                        }
                    }
                }
                List<int> channelHeaders = new List<int>(channels.ToList());
                channelHeaders.Sort();

                foreach (int channelHeader in channelHeaders)
                {
                    Console.Out.Write("Channel {0,5}|", channelHeader);
                }
                Console.Out.WriteLine();

                foreach (Row row in pattern.Rows)
                {
                    int channelIndex = 0;
                    for (int cIndex = 0; cIndex < row.ChannelEvents.Length; cIndex++)
                    {
                        ChannelEvent evt = row.ChannelEvents[cIndex];
                        if (evt == null) continue;
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
            if (volume == -1) { return "  "; }
            return volume.ToString("00");
        }

        private static object InstrumentToString(int instrument)
        {
            if (instrument == -1) { return "  "; }
            return instrument.ToString("X2");
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

        private static string CommandValueToString(int command, int data)
        {
            if (command == -1) { return "   "; }
            return ((char)(((int)'A') + (command - 1))).ToString() + data.ToString("X2");
        }

        private static void PrintS3MFile(S3MFile file)
        {
            Type type = typeof(S3MFile);
            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                if (fieldInfo.FieldType.IsArray)
                {
                    Array values = (Array)fieldInfo.GetValue(file);
                    StringBuilder sb = new StringBuilder();
                    sb.Append("[");
                    foreach (object obj in values)
                    {
                        sb.AppendFormat("{0};", obj);
                    }
                    sb.Append("]");
                    Console.Out.WriteLine("{0}:{1}", fieldInfo.Name, sb.ToString());
                }
                else
                {
                    Console.Out.WriteLine("{0}:{1}", fieldInfo.Name, fieldInfo.GetValue(file));
                }
            }
        }

        private static void PrintS3MFile(string file)
        {
            Console.Out.Write("      |");
            for (int i = 0; i < 0x10; i++)
            {
                Console.Out.Write("{0:X3}|", i);
            }
            Console.Out.WriteLine();
            using (FileStream stream = new FileStream(file, FileMode.Open))
            {
                int bufferSize = 16;
                int readCount = 0;
                byte[] buffer = new byte[bufferSize];
                int streamIndex = 0;

                while (true)
                {
                    readCount = stream.Read(buffer, 0, bufferSize);

                    Console.Out.Write("{0:X6}|", streamIndex);

                    WriteBufferLineHex(buffer, readCount);
                    Console.Out.WriteLine();
                    Console.Out.Write("      |");
                    WriteBufferLineDecimal(buffer, readCount);
                    Console.Out.WriteLine();
                    Console.Out.Write("      |");
                    WriteBufferLineChar(buffer, readCount);
                    Console.Out.WriteLine();
                    Console.Out.WriteLine();

                    streamIndex += readCount;

                    if (readCount < bufferSize)
                    {
                        break;
                    }
                    if (stream.Position > bufferSize * 4)
                    {
                        //break;
                    }
                }
            }
        }

        private static void WriteBufferLineHex(byte[] buffer, int readCount)
        {
            for (int bufferIndex = 0; bufferIndex < readCount; bufferIndex++)
            {
                Console.Out.Write("{0:X3}|", buffer[bufferIndex]);
            }
            for (int leftOverCount = readCount; leftOverCount < buffer.Length; leftOverCount++)
            {
                Console.Out.Write("  ");
            }
        }

        private static void WriteBufferLineDecimal(byte[] buffer, int readCount)
        {
            for (int bufferIndex = 0; bufferIndex < readCount; bufferIndex++)
            {
                Console.Out.Write("{0:D3}|", buffer[bufferIndex]);
            }
            for (int leftOverCount = readCount; leftOverCount < buffer.Length; leftOverCount++)
            {
                Console.Out.Write("  ");
            }
        }

        private static void WriteBufferLineChar(byte[] buffer, int readCount)
        {
            for (int bufferIndex = 0; bufferIndex < readCount; bufferIndex++)
            {
                char c = (char)buffer[bufferIndex];
                byte b = buffer[bufferIndex];
                if (b < 32 || 126 < b)
                {
                    Console.Out.Write("???|");
                }
                else
                {
                    Console.Out.Write("{0,3}|", c);
                }
            }
            for (int leftOverCount = readCount; leftOverCount < buffer.Length; leftOverCount++)
            {
                Console.Out.Write("  ");
            }
        }
    }
}
