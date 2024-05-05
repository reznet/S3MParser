namespace S3mToMidi.LilyPond
{
    internal class NoteWithDurationEvent : DurationEvent
    {
        public readonly NoteEvent NoteOn;
        public readonly NoteEvent NoteOff;

        public NoteWithDurationEvent(NoteEvent noteOn, NoteEvent noteOff) : base(noteOn.Tick, noteOff.Tick - noteOn.Tick)
        {
            NoteOn = noteOn;
            NoteOff = noteOff;
        }

        public override int Pitch => NoteOn.Pitch;

        public int Velocity => NoteOn.Velocity;
    }
}