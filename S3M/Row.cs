using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace S3M
{
    [DebuggerDisplay("RowNumber={RowNumber} Pattern={Pattern.PatternNumber}")]
    public class Row
    {
        public int RowNumber
        {
            get;
            internal set;
        }

        internal Pattern Pattern;

        public List<ChannelEvent> ChannelEvents = new List<ChannelEvent>();

        internal static Row Parse(System.IO.Stream stream, System.IO.BinaryReader reader)
        {
            Row row = new Row();

            while (true)
            {
                ChannelEvent channelEvent = ChannelEvent.Parse(stream, reader);
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
