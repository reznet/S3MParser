using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Diagnostics;

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

        private static string ConvertToPdf(string lilyPondFile)
        {
            string pdfFile = Path.ChangeExtension(lilyPondFile, ".pdf");

            Process lilypond = Process.Start(@"C:\Program Files (x86)\LilyPond\usr\bin\lilypond.exe", "\"" + lilyPondFile + "\"");
            lilypond.WaitForExit();

            return pdfFile;
        }

        private static string ConvertToLilyPond(string path)
        {
            string lilyPondFile = Path.ChangeExtension(path, ".ly");

            Process musicxml2ly = Process.Start(@"C:\Program Files (x86)\LilyPond\usr\bin\musicxml2ly.py", "-o \"" + lilyPondFile + "\" \"" + path + "\"");
            musicxml2ly.WaitForExit();

            return lilyPondFile;
        }
    }
}
