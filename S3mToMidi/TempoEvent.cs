using System.Diagnostics;

namespace S3mToMidi
{
    internal class TempoEvent : Event
    {
        public TempoEvent(int tick, int tempoBpm)
            : base(tick)
        {
            Debug.Assert(tempoBpm > 0, "tempo must be a positive integer: " + tempoBpm);
            TempoBpm = tempoBpm;
        }

        public int TempoBpm { get; }
    }
}
