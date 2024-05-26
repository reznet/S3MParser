namespace S3mToMidi.LilyPond
{
    internal class Clef
    {
        private readonly string clefName;
        private readonly Ottava ottava;
        private int currentOttava;

        // number of times the new clef needs to be seen in a row
        // before switching to it
        private const int clefThreshold = 5;

        private Queue<ClefPlaceholder> clefPlaceholders = new Queue<ClefPlaceholder>();

        private string currentClef;

        private int velocity;

        public Clef(string clefName)
        {
            this.clefName = clefName;
            this.ottava = new Ottava(24, 36, 67, 79); // HACK
        }

        private class ClefPlaceholder
        {
            public string Clef;
            public Placeholder Placeholder;

            public override string ToString()
            {
                return $"{Clef} {Placeholder}";
            }
        }

        public void WriteStaffForChannelPitch(int channelPitch, LilyPondTextWriter writer)
        {
            var midiPitch = ChannelNoteToMidiPitch(channelPitch);

            int newOttava = ottava.GetOttava(midiPitch);
            if (newOttava != currentOttava)
            {
                currentOttava = newOttava;
                //writer.WriteLine("\\ottava #{0}", newOttava);
            }

            string newClef = GetClef(midiPitch);
            var clefPlaceholder = new ClefPlaceholder(){ Clef = newClef, Placeholder = writer.AppendPlaceholder() };
            clefPlaceholders.Enqueue(clefPlaceholder);

            while(clefPlaceholders.Count > clefThreshold)
            {
                clefPlaceholders.Dequeue();
            }

            if (clefPlaceholders.Count == clefThreshold && clefPlaceholders.All(p => p.Clef != currentClef))
            {
                clefPlaceholders.Peek().Placeholder.Text = $"\\clef {newClef} ";
                currentClef = newClef;
            }
        }

        public string GetClef(int midiPitch)
        {
            var newClef = "";
            if (midiPitch < 60)
            {
                return "bass";
            }
            else
            {
                return "treble";
            }
        }

        public void WriteVelocity(int velocity, LilyPondTextWriter writer)
        {
            if (this.velocity != velocity)
            {
                writer.Write($"\\set fontSize = #{GetFontSizeForVelocity(velocity)} ");
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