using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using S3M;

namespace S3MParser
{
    class Program
    {
        static void Main(string[] args)
        {
            string filename = "V-BOGEY.S3M";
            S3MFile file = S3MFile.Parse(filename);

            MidiWriter2.Save(NoteEventGenerator.Generate(file).Take(3).ToList(), Path.ChangeExtension(filename, ".mid"));
        }
    }
}
