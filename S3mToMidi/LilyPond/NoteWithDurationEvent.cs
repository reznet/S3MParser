namespace S3mToMidi.LilyPond
{
    internal class NoteWithDurationEvent : DurationEvent
    {
        private readonly int pitch;
        private readonly int velocity;
        private readonly int instrument;

        public NoteWithDurationEvent(NoteEvent noteOn, NoteEvent noteOff) : base(noteOn.Tick, noteOff.Tick - noteOn.Tick)
        {
            pitch = noteOn.Pitch;
            velocity = noteOn.Velocity;
            instrument = noteOn.Instrument;
        }

        public NoteWithDurationEvent(int startTick, int duration, int pitch, int velocity, int instrument) : base(startTick, duration)
        {
            this.pitch = pitch;
            this.velocity = velocity;
            this.instrument = instrument;
        }

        public override int Pitch => pitch;

        public override int Velocity => velocity;

        public override int Instrument => instrument;

        public override string ToString()
        {
            return string.Format("NoteWithDurationEvent Tick:{0} Duration:{1} Instrument:{4} Pitch:{2} Velocity:{3}", Tick, Duration, Pitch, Velocity, Instrument);
        }
    }
}