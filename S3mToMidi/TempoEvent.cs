using System.Diagnostics;

namespace S3MParser
{
    class TempoEvent : Event
    {
        private readonly int _tempoBpm;

        public TempoEvent(int tick, int tempoBpm)
            : base(tick)
        {
            Debug.Assert(tempoBpm > 0, "tempo must be a positive integer: " + tempoBpm);
            _tempoBpm = tempoBpm;
        }

        public int TempoBpm { get { return _tempoBpm; } }
    }
}
