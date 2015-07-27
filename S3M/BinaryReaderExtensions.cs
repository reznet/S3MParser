using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace S3M
{
    public static class BinaryReaderExtensions
    {
        public static int ReadBytesAsInt(this BinaryReader reader, int length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException("length", "length must be greater than zero");
            }
            if (length == 1)
            {
                return ReadByteAsInt(reader);
            }
            else if (length == 2)
            {
                return ReadShortAsInt(reader);
            }
            else if (length == 4)
            {
                return ReadLongAsInt(reader);
            }
            throw new ArgumentOutOfRangeException("length", "length must be 1, 2, or 4");
        }

        private static int ReadByteAsInt(this BinaryReader reader)
        {
            byte value = reader.ReadByte();
            return Convert.ToInt32(value);
        }

        private static int ReadShortAsInt(this BinaryReader reader)
        {
            byte[] data = reader.ReadBytes(2);
            int byte0 = Convert.ToInt32(data[0]);
            int byte1 = Convert.ToInt32(data[1]);

            int value = byte1 << 4 | byte0;
            return value;
        }

        private static int ReadLongAsInt(BinaryReader reader)
        {
            byte[] data = reader.ReadBytes(4);
            int byte0 = Convert.ToInt32(data[0]);
            int byte1 = Convert.ToInt32(data[1]);
            int byte2 = Convert.ToInt32(data[2]);
            int byte3 = Convert.ToInt32(data[3]);

            int value = (byte2 << 12) | (byte3) << 8 | (byte0 << 4) | (byte1);
            return value;
        }
    }
}
