namespace S3mToMidi.LilyPond
{
    internal class Pitch
    {
        private static string[] PitchNamesSharps = ["c", "c-sharp", "d", "d-sharp", "e", "f", "f-sharp", "g", "g-sharp", "a", "a-sharp", "b"];
        private static string[] PitchNamesFlats = ["c", "d-flat", "d", "e-flat", "e", "f", "g-flat", "g", "a-flat", "a", "b-flat", "b"];

        public string ChannelNoteToLilyPondPitch(int note)
        {
            // byte 0 - Note; hi=oct, lo=note, 255=empty note,
            // 254=key off (used with adlib, with samples stops smp)
            if(note == -1){ return "r"; }
            // C5 = 64 = octave 5 + step 0
            int octave = (note >> 4) + 1;
            int step = note & 15;
            
            string octaveName = "";

            if(4 < octave)
            {
                octaveName = new string('\'', octave - 4);
            }
            else if (4 == octave)
            {
                // no-op
            }
            else if (octave < 4)
            {
                octaveName = new string(',', 4 - octave);
            }

            return PitchNamesFlats[step] + octaveName;
        }
    }
}