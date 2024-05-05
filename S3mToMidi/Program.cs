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

            [Option("pattern", Required = false, HelpText = "A pattern to export.  Can be specified multiple times.")]
            public IEnumerable<int>? Pattern { get; set; }

            [Option("start-order", Required = false, HelpText = "The order in the song to start at.  Use this to skip patterns in the beginning of the song with time signatures that do not render well in MIDI programs.")]
            public int? StartOrder { get; set; }

            [Option("exclude-channel", Required = false, HelpText = "ScreamTracker channels to exclude from the output file.  Use this to exclude drum channels.")]
            public IEnumerable<int>? ExcludeChannels { get; set; }

            [Option("minimum-volume", Required = false, HelpText = "The minimum volume a note must have to be written to the output file.  Use this to remove echos and simplify a part.")]
            public int? MinimumVolume { get; set; }

            [Option("explode-channels-by-instrument", Required = false, HelpText = "By default all instruments in a channel are written to the same output midi channel.  Use this to write each instrument in a channel to its own midi channel.")]
            public bool ExplodeChannelsByInstrument { get; set; }

            [Option("exporter", Required = false, Default = "midi", HelpText = "Which export format to use.")]
            public string Exporter { get; set; } = "midi";
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

                       switch (o.Exporter.ToLowerInvariant())
                       {
                           case "midi":
                               ExportMidi(file, o);
                               break;
                           case "lilypond":
                               ExportLilyPond(file, o);
                               break;
                           default:
                               Console.Error.WriteLine("Unrecognized exporter {0}", o.Exporter);
                               break;
                       }
                   })
                   .WithNotParsed((errors) =>
                   {
                       Environment.Exit(1);
                   });
        }

        private static void ExportLilyPond(S3MFile file, Options o)
        {
            Dictionary<int, ImmutableList<Event>> noteEvents =
                new NoteEventGenerator(new NoteEventGeneratorOptions()
                {
                    PatternsToExport = o.Pattern?.ToImmutableHashSet(),
                    StartOrder = o.StartOrder,
                    ExcludedChannels = o.ExcludeChannels.ToHashSet(),
                    ChannelInstrumentOutputBehavior = o.ExplodeChannelsByInstrument ? ChannelInstrumentOutputBehavior.Explode : ChannelInstrumentOutputBehavior.Collapse
                },
                    (channelNumber) => new LilyPondOutputChannel(channelNumber))
                .Generate(file);

            using (var stringWriter = new StringWriter())
            {
                var writer = new LilyPondWriter(stringWriter);
                writer.Write(noteEvents);

                Console.Out.WriteLine(stringWriter.ToString());
                var outputFilename = Path.ChangeExtension(o.InputFile, ".ly");
                if (File.Exists(outputFilename))
                {
                    File.Delete(outputFilename);
                }
                File.WriteAllText(outputFilename, stringWriter.ToString());
            }
        }

        private static void ExportMidi(S3MFile file, Options o)
        {
            Dictionary<int, ImmutableList<Event>> noteEvents =
                new NoteEventGenerator(new NoteEventGeneratorOptions()
                {
                    PatternsToExport = o.Pattern?.ToImmutableHashSet(),
                    StartOrder = o.StartOrder,
                    ExcludedChannels = o.ExcludeChannels.ToHashSet(),
                    ChannelInstrumentOutputBehavior = o.ExplodeChannelsByInstrument ? ChannelInstrumentOutputBehavior.Explode : ChannelInstrumentOutputBehavior.Collapse
                },
                    (channelNumber) => new MidiOutputChannel(channelNumber))
                .Generate(file);

            var midiFile = new MidiWriter().Write(noteEvents);

            midiFile.Write(Path.GetFileName(Path.ChangeExtension(o.InputFile, ".mid")), overwriteFile: true);
        }
    }
}
