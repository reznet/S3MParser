namespace S3mToMidi.LilyPond
{
    internal abstract class DurationEvent : Event
    {
        public readonly int Duration;

        public abstract int Pitch { get; }
        
        public abstract int Velocity { get; }
        public DurationEvent(int tick, int duration) : base(tick)
        {
            Duration = duration;
        }
    }
}