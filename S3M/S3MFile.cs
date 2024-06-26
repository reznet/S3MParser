﻿using System.Text;
using System.Diagnostics;

namespace S3M
{
    public class S3MFile
    {
        public string Name;
        public int Type;
        public int OrderCount;
        public int InstrumentCount;
        public int PatternCount;
        public int GlobalVolume;
        public int InitialSpeed;
        public int InitialTempo;
        public int MasterVolume;
        public int UltraClickRemoval;
        public int LoadChannelPanSettings;
        public int[] ChannelSettings;
        public int[] Orders;
        public int[] InstrumentPointers;
        public int[] PatternPointers;

        public List<Pattern> Patterns = new List<Pattern>();

        public static S3MFile Parse(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Open);
            using (BinaryReader reader = new BinaryReader(stream))
            {
                S3MFile file = new S3MFile();
                file.Name = ReadFileName(reader);
                CheckStreamPosition(stream, 28);
                // skip one byte (it's always 1A)
                stream.ReadByte();
                file.Type = reader.ReadBytesAsInt(1);
                CheckStreamPosition(stream, 30);
                // skip to next row
                stream.Seek(2, SeekOrigin.Current);
                CheckStreamPosition(stream, 0x20);
                file.OrderCount = reader.ReadInt16();
                file.InstrumentCount = reader.ReadInt16();
                file.PatternCount = reader.ReadInt16();
                // skip flags
                reader.ReadInt16();
                // skip version info
                reader.ReadInt16();
                // skip file format info
                reader.ReadInt16();
                ReadSCRM(reader);
                CheckStreamPosition(stream, 0x30);
                file.GlobalVolume = reader.ReadBytesAsInt(1);
                file.InitialSpeed = reader.ReadBytesAsInt(1);
                file.InitialTempo = reader.ReadBytesAsInt(1);
                file.MasterVolume = reader.ReadBytesAsInt(1);
                file.UltraClickRemoval = reader.ReadBytesAsInt(1);
                file.LoadChannelPanSettings = reader.ReadBytesAsInt(1);
                stream.Seek(0x40, SeekOrigin.Begin);
                file.ChannelSettings = ReadByteArrayAsIntArray(32, reader);
                file.Orders = ReadByteArrayAsIntArray(file.OrderCount, reader);
                file.InstrumentPointers = ReadParapointers(file.InstrumentCount, reader);
                file.PatternPointers = ReadParapointers(file.PatternCount, reader);

                file.Patterns.AddRange(ReadPatterns(file.PatternPointers, stream, reader));

                return file;
            }
        }

        private static int[] ReadParapointers(int pointerCount, BinaryReader reader)
        {
            int[] values = new int[pointerCount];

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = ReadParapointer(reader);
            }
            return values;
        }

        internal static int ReadParapointer(BinaryReader reader)
        {
            return reader.ReadUInt16();
        }

        private static IEnumerable<Pattern> ReadPatterns(int[] patternPointers, Stream stream, BinaryReader reader)
        {
            for (int patternIndex = 0; patternIndex < patternPointers.Length; patternIndex++)
            {
                int patternPointer = patternPointers[patternIndex];
                int patternStartIndex = patternPointer * 16;
                stream.Seek(patternStartIndex, SeekOrigin.Begin);
                // Console.Out.WriteLine("Parsing pattern {0} at {1}.", patternIndex + 1, patternStartIndex);
                Pattern pattern = Pattern.Read(stream, reader);
                pattern.PatternNumber = patternIndex;
                yield return pattern;
            }
        }

        private static int[] ReadByteArrayAsIntArray(int byteCount, BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(byteCount);
            int[] values = new int[bytes.Length];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = Convert.ToInt32(bytes[i]);
            }
            return values;
        }

        private static void ReadSCRM(BinaryReader reader)
        {
            byte[] scrm = reader.ReadBytes(4);
            string scrmString = Encoding.ASCII.GetString(scrm);
            Debug.Assert(scrmString == "SCRM", "Parse Assert", String.Format("Did not find SCRM token.  Instead, found {0}.", scrmString));
        }

        private static void CheckStreamPosition(FileStream stream, int targetPosition)
        {
            Debug.Assert(stream.Position == targetPosition, "Parse Assert", String.Format("Stream should be at 0x{0:X2} but is actually at 0x{1:X2}.", targetPosition, stream.Position));
        }

        private static string ReadFileName(BinaryReader reader)
        {
            byte[] buffer = reader.ReadBytes(28);
            int nullIndex = Array.IndexOf(buffer, (byte)'\0');
            if (-1 == nullIndex)
            {
                return Encoding.ASCII.GetString(buffer);
            }
            else
            {
                return Encoding.ASCII.GetString(buffer, 0, nullIndex);
            }
        }
    }
}
