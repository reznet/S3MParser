namespace S3M
{
    public class Cell
    {
        public Cell(int row, int channel, ChannelEvent evt)
        {
            Row = row;
            Channel = channel;
            Event = evt;
        }

        public int Row { get; set; }
        public int Channel { get; set; }
        public ChannelEvent Event;
    }
}
