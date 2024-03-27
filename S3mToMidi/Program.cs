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

            [Option("exclude-channel", Required = false, HelpText = "ScreamTracker channels to exclude from the output file.  Use this to exclude drum channels.")]
            public IEnumerable<int> ExcludeChannels { get; set; }
        }

        static void Main(string[] args)
        {

            var parser = new Parser(with => {
                with.AllowMultiInstance = true;
            });
            parser.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       S3MFile file = S3MFile.Parse(o.InputFile);

                       var noteEvents = 
                            NoteEventGenerator.Generate(
                                file, 
                                new NoteEventGeneratorOptions() 
                                    { 
                                        ChannelsFromPatterns = o.ChannelsFromPatterns,
                                        Pattern = o.Pattern,
                                        StartOrder = o.StartOrder,
                                    });

                       MidiWriter2.Save(noteEvents, 
                            Path.GetFileName(Path.ChangeExtension(o.InputFile, ".mid")), 
                            new MidiExportOptions(){ ExcludedChannels = o.ExcludeChannels.ToHashSet() });
                   });
        }
    }
}
