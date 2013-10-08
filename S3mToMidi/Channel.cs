using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S3MParser
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
