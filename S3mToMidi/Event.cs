using System.Diagnostics;

namespace S3mToMidi
{
    internal class Event
    {
        public Event(int tick)
        {
            Tick = tick;

            Debug.Assert(0 <= Tick, "negative tick");
        }

        public int Tick { get; private set; }
    }
}
