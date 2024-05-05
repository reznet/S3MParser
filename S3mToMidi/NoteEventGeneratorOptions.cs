using System.Collections.Immutable;

namespace S3mToMidi
{
    internal class NoteEventGeneratorOptions
    {
        /// <summary>
        /// Gets or sets the patterns to export.
        /// </summary>
        public ImmutableHashSet<int> PatternsToExport { get; set; }

        /// <summary>
        /// Gets or sets the index of the Orders where the export should start.
        /// </summary>
        public int? StartOrder { get; set; }

        /// <summary>
        /// Gets or sets the set of channels to exclude from note generation.
        /// </summary>
        /// <remarks>
        /// Note that excluded channels are still processed for meta events such as time signature and pattern breaks.
        /// </remarks>
        public HashSet<int> ExcludedChannels { get; set; } = new HashSet<int>();

        /// <summary>
        /// Gets or sets a value indicating how to output channel instruments.
        /// </summary>
        public ChannelInstrumentOutputBehavior ChannelInstrumentOutputBehavior { get; set; }
    }
}
