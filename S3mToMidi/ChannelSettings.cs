﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S3MParser
{
    public class ChannelSettings
    {
        public int ChannelNumber { get; set; }
        public int Volume { get; set; }
        public int Pitch { get; set; }

        public ChannelSettings(int channelNumber)
        {
            this.ChannelNumber = channelNumber;
        }
    }
}
