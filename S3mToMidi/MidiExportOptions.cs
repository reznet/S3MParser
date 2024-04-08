namespace S3mToMidi
{
    public class MidiExportOptions
    {
        public HashSet<int> ExcludedChannels { get; set; } = new HashSet<int>();
    }
}
