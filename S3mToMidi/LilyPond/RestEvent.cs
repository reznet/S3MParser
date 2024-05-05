namespace S3mToMidi.LilyPond
{
            internal class RestEvent : DurationEvent
        {
            public RestEvent(int tick, int duration) : base(tick, duration)
            {
            }

            public override int Pitch => -1;
        }
}