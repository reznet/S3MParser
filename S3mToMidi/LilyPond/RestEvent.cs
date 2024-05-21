namespace S3mToMidi.LilyPond
{
    internal class RestEvent : DurationEvent
    {
        public RestEvent(int tick, int duration) : base(tick, duration)
        {
        }

        public override int Pitch => -1;

        public override int Velocity => -1;

        public override int Instrument => -1;

        public override string ToString()
        {
            return string.Format("RestEvent Tick:{0} Duration:{1}", Tick, Duration);
        }
    }
}