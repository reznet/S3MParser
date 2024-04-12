namespace S3mToMidi
{
    public class NoteEventGeneratorOptions
    {
        /// <summary>
        /// Gets or sets the single pattern to export.
        /// </summary>
        public int? Pattern { get; set; }

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
    }
}
