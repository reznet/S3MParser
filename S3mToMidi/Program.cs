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

            [Option("pattern", Required = false, HelpText = "The single pattern to export.")]
            public int? Pattern { get; set; }

            [Option("start-order", Required = false, HelpText = "The order in the song to start at.  Use this to skip patterns in the beginning of the song with time signatures that do not render well in MIDI programs.")]
            public int? StartOrder { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       S3MFile file = S3MFile.Parse(o.InputFile);

                       MidiWriter2.Save(
                            NoteEventGenerator.Generate(
                                file, 
                                new NoteEventGeneratorOptions() 
                                    { 
                                        ChannelsFromPatterns = o.ChannelsFromPatterns,
                                        Pattern = o.Pattern,
                                        StartOrder = o.StartOrder,
                                    })
                            .ToList(),
                            Path.GetFileName(Path.ChangeExtension(o.InputFile, ".mid")));
                   });
        }
    }
}
