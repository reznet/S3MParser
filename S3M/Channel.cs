namespace S3M
{
    public class Channel
    {
        public int ChannelNumber { get; set; }
        public IEnumerable<ChannelEvent> ChannelEvents { get; set; }

        public Channel(int channelNumber, IEnumerable<ChannelEvent> events)
        {
            this.ChannelNumber = channelNumber;
            this.ChannelEvents = events;
        }
    }
}
