namespace S3M
{
    public class Channel(int channelNumber, IEnumerable<ChannelEvent> events)
    {
        public int ChannelNumber { get; set; } = channelNumber;
        public IEnumerable<ChannelEvent> ChannelEvents { get; set; } = events;
    }
}
