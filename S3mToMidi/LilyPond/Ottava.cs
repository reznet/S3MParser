using System.Diagnostics;

namespace S3mToMidi.LilyPond
{
    internal class Ottava
    {
        private readonly int minimumPitch;
        private readonly int maximumPitch;

        public Ottava(int mininumPitch, int maximumPitch)
        {
            this.minimumPitch = mininumPitch;
            this.maximumPitch = maximumPitch;
        }

        public int GetOttava(int pitch)
        {
            Debug.Assert(0 < pitch);
            int newOttava = 0;

            if (maximumPitch < pitch)
            {
                newOttava = 1;
            }
            else if (pitch < minimumPitch)
            {
                newOttava = -1;
            }

            return newOttava;
        }
    }
}