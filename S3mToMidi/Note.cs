namespace S3MParser
{
    class Note
    {
        public enum NoteType
        {
            Pitched,
            NoteOff,
            Empty
        }

        public int Octave { get; private set; }
        public string Pitch { get; private set; }
        public NoteType Type { get; private set; }

        private static string[] pitches = { "C-", "C#", "D-", "D#", "E-", "F-", "F#", "G-", "G#", "A-", "A#", "B-" };

        public Note(int value)
        {
            switch (value)
            {
                case 255:
                    this.Type = NoteType.Empty;
                    break;
                case 254:
                    this.Type = NoteType.NoteOff;
                    break;
                default:
                    this.Type = NoteType.Pitched;
                    break;
            }
            if (this.Type == NoteType.Pitched)
            {
                this.Octave = 1 + (value >> 4);
                this.Pitch = pitches[value & 15];
            }
        }

    }
}
