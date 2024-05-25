namespace S3mToMidi.LilyPond
{
    public abstract class Subdivider
    {
        private readonly int beatsPerBar;
        private readonly int beatValue;

        private readonly int beatDuration;

        public Subdivider(int beatsPerBar, int beatValue)
        {
            this.beatsPerBar = beatsPerBar;
            this.beatValue = beatValue;
            this.beatDuration = Durations.WholeNote / beatValue;
        }

        public int BeatsPerBar { get { return beatsPerBar; } }
        public int BeatValue { get { return beatValue; } }

        public int BeatDuration { get { return beatDuration; } }

        public int MeasureDuration { get { return beatsPerBar * BeatDuration; } }

        public abstract int GetNextSubdivision(int startTime, int endTime);
    }
}