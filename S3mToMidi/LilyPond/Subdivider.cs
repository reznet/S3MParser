namespace S3mToMidi.LilyPond
{
    public abstract class Subdivider
    {
        private readonly int beatsPerBar;
        private readonly int beatValue;

        public Subdivider(int beatsPerBar, int beatValue)
        {
            this.beatsPerBar = beatsPerBar;
            this.beatValue = beatValue;
        }

        public int BeatsPerBar { get { return beatsPerBar; } }
        public int BeatValue { get { return beatValue; } }

        public abstract int GetNextSubdivision(int startTime, int endTime);
    }
}