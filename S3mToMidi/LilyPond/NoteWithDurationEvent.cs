namespace S3mToMidi.LilyPond
{
    internal class NoteWithDurationEvent : DurationEvent
    {
        private readonly int pitch;
        private readonly int velocity;

        public NoteWithDurationEvent(NoteEvent noteOn, NoteEvent noteOff) : base(noteOn.Tick, noteOff.Tick - noteOn.Tick)
        {
            pitch = noteOff.Pitch;
            velocity = noteOn.Velocity;
        }

        public NoteWithDurationEvent(int startTick, int duration, int pitch, int velocity) : base(startTick, duration)
        {
            this.pitch = pitch;
            this.velocity = velocity;
        }

        public override int Pitch => pitch;

        public override int Velocity => velocity;
    }
}