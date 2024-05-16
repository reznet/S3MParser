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

        public override string ToString()
        {
            return string.Format("NoteWithDurationEvent Tick:{0} Duration:{1} Pitch:{2} Velocity:{3}", Tick, Duration, Pitch, Velocity);
        }
    }
}