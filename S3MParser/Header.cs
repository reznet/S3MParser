using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace S3MParser
{
    public class Header
    {
        public string SongName { get; set; }
        public int Type { get; set; }
        public int OrderCount { get; set; }
        public int InstrumentCount { get; set; }
        public int PatternCount { get; set; }
        public int GlobalVolume { get; set; }
        public int InitialSpeed { get; set; }
        public int InitialTempo { get; set; }
        public int MasterVolume { get; set; }
        public int UltraClickRemoval { get; set; }
        public int LoadChannelPanSettings { get; set; }

        public int[] ChannelSettings;
        public int[] Orders;
        public int[] InstrumentPointers;
        public int[] PatternPointers;

        public static Header Parse(BinaryReader reader)
        {
            Header header = new Header();

            header.SongName = ReadFileName(reader);
            // skip one byte (it's always 1A)
            reader.ReadByte();
            header.Type = reader.ReadByte();
            // skip to next row
            reader.BaseStream.Seek(2, SeekOrigin.Current);
            //file.OrderCount = ReadByteAsInt(reader);
            header.OrderCount = reader.ReadInt16();
            header.InstrumentCount = reader.ReadInt16();
            header.PatternCount = reader.ReadInt16();
            // skip flags
            //reader.ReadInt16();
            reader.BaseStream.Seek(0x02, SeekOrigin.Current);
            // skip version info
            //reader.ReadInt16();
            reader.BaseStream.Seek(0x02, SeekOrigin.Current);
            // skip file format info
            reader.ReadInt16();
            ReadSCRM(reader);
            header.GlobalVolume = reader.ReadByte();
            header.InitialSpeed = reader.ReadByte();
            header.InitialTempo = reader.ReadByte();
            header.MasterVolume = reader.ReadByte();
            header.UltraClickRemoval = reader.ReadByte();
            header.LoadChannelPanSettings = reader.ReadByte();
            reader.BaseStream.Seek(0x40, SeekOrigin.Begin);
            header.ChannelSettings = ReadByteArrayAsIntArray(Pattern.ChannelCount, reader);
            header.Orders = ReadByteArrayAsIntArray(header.OrderCount, reader);
            header.InstrumentPointers = ReadParapointers(header.InstrumentCount, reader);
            header.PatternPointers = ReadParapointers(header.PatternCount, reader);

            return header;
        }

        private static string ReadFileName(BinaryReader reader)
        {
            byte[] buffer = reader.ReadBytes(28);
            return Encoding.ASCII.GetString(buffer).TrimEnd('\0');
        }

        private static void ReadSCRM(BinaryReader reader)
        {
            byte[] scrm = reader.ReadBytes(4);
            string scrmString = Encoding.ASCII.GetString(scrm);
            Debug.Assert(scrmString == "SCRM", "Parse Assert", String.Format("Did not find SCRM token.  Instead, found {0}.", scrmString));
        }

        private static int[] ReadByteArrayAsIntArray(int byteCount, BinaryReader reader)
        {
            int[] values = new int[byteCount];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = reader.ReadByte();
            }
            return values;
        }

        private static int[] ReadShortArrayAsIntArray(int byteCount, BinaryReader reader)
        {
            int[] values = new int[byteCount];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = reader.ReadUInt16();
            }
            return values;
        }

        private static int[] ReadParapointers(int pointerCount, BinaryReader reader)
        {
            int[] values = new int[pointerCount];

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = reader.ReadUInt16();
            }
            return values;
        }
    }
}
