using S3M;
using CommandLine;
using System.Runtime.CompilerServices;
using System.Collections.Immutable;

[assembly: InternalsVisibleTo("S3mToMidi.Tests")]

namespace S3mToMidi
{
    internal class Program
    {

        public class Options
        {
            [Option('f', "file", Required = true, HelpText = "Path to the file to convert.")]
            public string? InputFile { get; set; }

            [Option("pattern", Required = false, HelpText = "The single pattern to export.")]
            public int? Pattern { get; set; }

            [Option("start-order", Required = false, HelpText = "The order in the song to start at.  Use this to skip patterns in the beginning of the song with time signatures that do not render well in MIDI programs.")]
            public int? StartOrder { get; set; }

            [Option("exclude-channel", Required = false, HelpText = "ScreamTracker channels to exclude from the output file.  Use this to exclude drum channels.")]
            public IEnumerable<int>? ExcludeChannels { get; set; }

            [Option("minimum-volume", Required = false, HelpText = "The minimum volume a note must have to be written to the output file.  Use this to remove echos and simplify a part.")]
            public int? MinimumVolume { get; set; }
        }

        private static void Main(string[] args)
        {

            Parser parser = new(with =>
            {
                with.AllowMultiInstance = true;
                with.AutoHelp = true;
                with.AutoVersion = true;
                with.HelpWriter = Console.Error;
            });
            _ = parser.ParseArguments<Options>(args).WithParsed(o =>
                   {
                       S3MFile file = S3MFile.Parse(o.InputFile);

                       if (o.MinimumVolume.HasValue)
                       {
                           new VolumeFilter() { VolumeThreshold = o.MinimumVolume.Value }.Apply(file);
                       }

                       Dictionary<int, ImmutableList<Event>> noteEvents =
                            new NoteEventGenerator(new NoteEventGeneratorOptions()
                                {
                                    Pattern = o.Pattern,
                                    StartOrder = o.StartOrder,
                                    ExcludedChannels = o.ExcludeChannels.ToHashSet()
                                })
                                .Generate(file);

                       var midiFile = MidiWriter.Write(noteEvents);

                        midiFile.Write(Path.GetFileName(Path.ChangeExtension(o.InputFile, ".mid")), overwriteFile: true);
                   })
                   .WithNotParsed((errors) =>
                   {
                       Environment.Exit(1);
                   });
        }
    }
}
