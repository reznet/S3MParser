using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S3MParser
{
    class Row
    {
        internal int RowNumber;
        internal Pattern Pattern;

        internal List<ChannelEvent> ChannelEvents = new List<ChannelEvent>();

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
