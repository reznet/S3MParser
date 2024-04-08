using System.Diagnostics;

namespace S3M
{
    [DebuggerDisplay("RowNumber={RowNumber} Pattern={Pattern.PatternNumber}")]
    public class Row
    {
        public int RowNumber
        {
            get;
            set;
        }

        internal Pattern Pattern;

        public List<ChannelEvent> ChannelEvents = new List<ChannelEvent>();

        internal static Row Parse(BinaryReader reader)
        {
            Row row = new Row();

            while (true)
            {
                ChannelEvent channelEvent = ChannelEvent.Parse(reader);
                if (channelEvent == null)
                {
                    break;
                }
                channelEvent.Row = row;
                row.ChannelEvents.Add(channelEvent);
            }

            return row;
        }
    }
}
