namespace S3mToMidi
{
    internal class Note
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

        private static readonly string[] pitches = ["C-", "C#", "D-", "D#", "E-", "F-", "F#", "G-", "G#", "A-", "A#", "B-"];

        public Note(int value)
        {
            Type = value switch
            {
                255 => NoteType.Empty,
                254 => NoteType.NoteOff,
                _ => NoteType.Pitched,
            };
            if (Type == NoteType.Pitched)
            {
                Octave = 1 + (value >> 4);
                Pitch = pitches[value & 15];
            }
        }

    }
}
