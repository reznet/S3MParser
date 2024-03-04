using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using S3M;
using CommandLine;

namespace S3MParser
{
    class Program
    {

        public class Options
        {
            [Option('f', "file", Required = true, HelpText = "Path to the file to convert.")]
            public string InputFile { get; set; }
            [Option("channels-from-patterns", Required = false, Default = false, HelpText = "ScreamTracker pattern channels to MIDI channels.  Otherwise, instruments or samples map to MIDI channels.")]
            public bool ChannelsFromPatterns { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       S3MFile file = S3MFile.Parse(o.InputFile);

                       MidiWriter2.Save(NoteEventGenerator.Generate(file, new NoteEventGeneratorOptions() { ChannelsFromPatterns = o.ChannelsFromPatterns }).ToList(), Path.GetFileName(Path.ChangeExtension(o.InputFile, ".mid")));
                   });
        }
    }
}
