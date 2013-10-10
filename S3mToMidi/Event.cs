using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3MParser
{
    class Event
    {
        public Event(int tick)
        {
            Tick = tick;

            Debug.Assert(0 <= this.Tick, "negative tick");
        }

        public int Tick { get; private set; }
    }
}
