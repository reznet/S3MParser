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
            //PrintS3MFile("01-v-drac.s3m");
            Console.Out.WriteLine();
            //PrintS3MFile("02-v-bewm.s3m");
            //Console.Out.WriteLine();
            //PrintS3MFile("06-v-cf2.s3m");
            //Console.Out.WriteLine();

            S3MFile file = S3MFile.Parse("01-v-drac.s3m");
            //S3MFile file = S3MFile.Parse("simple.s3m");
            //Console.Out.WriteLine(file.Name);
            Printer.PrintPatterns(from p in file.Patterns where p.PatternNumber == 4 select p);

            MidiWriter writer = new MidiWriter(file);
            writer.Save("out.mid");

        }
    }
}
