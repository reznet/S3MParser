using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S3MParser
{
    interface IChannel
    {
        int ChannelNumber
        {
            get;
        }

        IEnumerable<ChannelEvent> Events
        {
            get;
        }
    }
}
