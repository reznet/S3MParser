using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace S3MParser
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
                //file.OrderCount = ReadByteAsInt(reader);
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
            //Console.Out.WriteLine("Reading at " + ((FileStream)reader.BaseStream).Position);
            return reader.ReadUInt16();
            /*
            short s = reader.ReadInt16();
            Console.Out.WriteLine("PP: " + s);
            return s;
            */
            /*
            byte[] bytes = reader.ReadBytes(2);
            Console.Out.WriteLine("bytes[0] {0}", bytes[0]);
            Console.Out.WriteLine("bytes[1] {0}", bytes[1]);
            //int value1 = Convert.ToInt32(bytes[0]) + (Convert.ToInt32(bytes[1]) * 255);

            //int value1 = bytes[1] << 4 | bytes[0];
            int value1 = bytes[1] << 8 | bytes[0];

            Console.Out.WriteLine("PP: " + value1);

            return value1;
            */
            //int byte0 = Convert.ToInt32(bytes[0]);
            //int byte1 = Convert.ToInt32(bytes[1]);

            //int value2 = byte0 << 4 | byte1;

            //return s;
        }
        /*
        internal static int ReadParapointer(BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(2);
            int value = Convert.ToInt32(bytes[0]) + (Convert.ToInt32(bytes[1]) * 255);

            Console.Out.WriteLine("ParaPointer: [{0}|{1}] = {2}", bytes[0], bytes[1], value);

            return value;
        }*/

        private static IEnumerable<Pattern> ReadPatterns(int[] patternPointers, Stream stream, BinaryReader reader)
        {
            for(int patternIndex = 0; patternIndex < patternPointers.Length; patternIndex++)
            {
                int patternPointer = patternPointers[patternIndex];
                int patternStartIndex = patternPointer * 16;
                Console.Out.WriteLine("PatternPointer is {0}, start index is {1}.  seeking there", patternPointer, patternStartIndex);
                stream.Seek(patternStartIndex, SeekOrigin.Begin);
                Console.Out.WriteLine("Parsing pattern {0} at {1}.", patternIndex + 1, patternStartIndex);
                Pattern pattern = Pattern.Read(stream, reader);
                pattern.PatternNumber = patternIndex+1;
                yield return pattern;
                //break;
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

        private static int[] ReadShortArrayAsIntArray(int byteCount, BinaryReader reader)
        {
            int[] values = new int[byteCount];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = reader.ReadInt16();
            }
            return values;
        }

        private static void ReadSCRM(BinaryReader reader)
        {
            byte[] scrm = reader.ReadBytes(4);
            string scrmString = Encoding.ASCII.GetString(scrm);
            Debug.Assert(scrmString == "SCRM", "Parse Assert", String.Format("Did not find SCRM token.  Instead, found {0}.", scrmString));
        }
        /*
        public static int ReadByteAsInt(BinaryReader reader)
        {
            byte value = reader.ReadByte();
            return Convert.ToInt32(value);
        }

        public static int Read2BytesAsInt(BinaryReader reader)
        {
            byte[] data = reader.ReadBytes(2);
            int byte0 = Convert.ToInt32(data[0]);
            int byte1 = Convert.ToInt32(data[1]);

            int value = byte0 << 4 | byte1;
            return value;
        }

        public static int Read4BytesAsInt(BinaryReader reader)
        {
            byte[] data = reader.ReadBytes(4);
            int byte0 = Convert.ToInt32(data[0]);
            int byte1 = Convert.ToInt32(data[1]);
            int byte2 = Convert.ToInt32(data[2]);
            int byte3 = Convert.ToInt32(data[3]);

            int value = (byte2 << 12) | (byte3) << 8 | (byte0 << 4) | (byte1);
            return value;
        }*/

        private static void CheckStreamPosition(FileStream stream, int targetPosition)
        {
            Debug.Assert(stream.Position == targetPosition, "Parse Assert", String.Format("Stream should be at 0x{0:X2} but is actually at 0x{1:X2}.", targetPosition, stream.Position));
        }

        private static string ReadFileName(BinaryReader reader)
        {
            byte[] buffer = reader.ReadBytes(28);
            return Encoding.ASCII.GetString(buffer).TrimEnd('\0');
        }
    }
}
