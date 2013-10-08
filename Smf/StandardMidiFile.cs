using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Smf
{
    public class StandardMidiFile
    {
        Header header = new Header();

        public void Save(string path)
        {
            using (Stream stream = new FileStream(path, FileMode.OpenOrCreate))
            {
                using (BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII))
                {
                    header.Write(writer);
                }
            }
        }
    }
}
