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
            S3MFile file = S3MFile.Parse("V-FALCON.S3M");

            MidiWriter2.Save(NoteEventGenerator.Generate(file).Take(3).ToList(), "out4.mid");
        }
    }
}
