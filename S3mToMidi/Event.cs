using System.Diagnostics;

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
