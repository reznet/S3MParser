using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3MParser
{
    class BinaryFilePrinter
    {
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
