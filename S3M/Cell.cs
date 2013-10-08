using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S3M
{
    public class Cell
    {
        public Cell(int row, int channel, ChannelEvent evt)
        {
            this.Row = row;
            this.Channel = channel;
            this.Event = evt;
        }

        public int Row { get; set; }
        public int Channel { get; set; }
        public ChannelEvent Event;
    }
}
