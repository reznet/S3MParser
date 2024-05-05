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

        public ChannelEvent[] ChannelEvents = new ChannelEvent[S3MFile.CHANNEL_COUNT];

        internal static Row Parse(BinaryReader reader)
        {
            Row row = new Row();

            bool hasMoreChannels = true;

            for (int i = 0; i < S3MFile.CHANNEL_COUNT; i++)
            {
                if (hasMoreChannels)
                {
                    ChannelEvent channelEvent = ChannelEvent.Parse(reader);
                    if (channelEvent == null)
                    {
                        hasMoreChannels = false;
                    }
                    else
                    {
                        channelEvent.Row = row;
                        row.ChannelEvents[i] = channelEvent;
                    }
                }
                else
                {
                    // TODO : put anything in the array?
                }
            }

            return row;
        }
    }
}
