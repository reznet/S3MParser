namespace S3mToMidi.LilyPond
{
    public class Notehead
    {
        private static Dictionary<int, string> noteHeadStyles = new Dictionary<int, string>()
        {
            { 1, "default"},
            /*
            { 2, "harmonic" },
            { 3, "diamond" },
            { 4, "cross" },
            */
        };

        public string Default
        {
            get
            {
                return noteHeadStyles[1];
            }
        }

        public string GetNotehead(int instrument)
        {
            var noteHeadStyle = "default";
            if (noteHeadStyles.ContainsKey(instrument))
            {
                noteHeadStyle = noteHeadStyles[instrument];
            }

            return noteHeadStyle;
        }
    }
}