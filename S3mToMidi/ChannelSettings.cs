namespace S3mToMidi
{
    public class ChannelSettings(int channelNumber)
    {
        public int ChannelNumber { get; set; } = channelNumber;
        public int Volume { get; set; }
        public int Pitch { get; set; }
    }
}
