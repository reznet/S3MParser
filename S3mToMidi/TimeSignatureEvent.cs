namespace S3mToMidi
{
    internal class TimeSignatureEvent(int tick, int beatsPerBar, int beatValue) : Event(tick)
    {
        public int BeatsPerBar { get; } = beatsPerBar;

        public int BeatValue { get; } = beatValue;
    }
}
