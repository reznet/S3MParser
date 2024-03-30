namespace S3MParser
{
    public class NoteEventGeneratorOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether each midi channel is mapped to 
        /// an instrument (the default) or is mapped to a pattern.
        /// </summary>
        public bool ChannelsFromPatterns { get; set; }

        /// <summary>
        /// Gets or sets the single pattern to export.
        /// </summary>
        public int? Pattern { get; set; }

        public int? StartOrder { get; set; }
    }
}
