namespace S3mToMidi.LilyPond
{
    internal class Clef
    {
        private readonly string clefName;
        private readonly Ottava ottava;
        private int currentOttava;

        private int velocity;

        public Clef(string clefName)
        {
            this.clefName = clefName;
            this.ottava = new Ottava(24, 36, 67, 79); // HACK
        }

        public void WriteStaffForChannelPitch(int channelPitch, TextWriter writer)
        {
            var midiPitch = ChannelNoteToMidiPitch(channelPitch);

            int newOttava = ottava.GetOttava(midiPitch);
            if (newOttava != currentOttava)
            {
                currentOttava = newOttava;
                writer.WriteLine("\\ottava #{0}", newOttava);
            }
        }

        public void WriteVelocity(int velocity, TextWriter writer)
        {
            if (this.velocity != velocity)
            {
                writer.Write("\\set fontSize = #{0} ", GetFontSizeForVelocity(velocity));
                this.velocity = velocity;
            }
        }

        public static int GetFontSizeForVelocity(int velocity)
        {
            return -1 * (8 - Math.Min(8, (velocity + 8) / 8));
        }

        private static int ChannelNoteToMidiPitch(int note)
        {
            // C5 = 64 = octave 5 + step 0
            int step = note & 15;
            int octave = 1 + (note >> 4);

            return (octave * 12) + step;
        }
    }
}