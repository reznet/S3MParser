using System.Diagnostics;

namespace S3mToMidi.LilyPond
{
    internal class Time
    {
        //private int Tick;
        private int TickInMeasure;
        public int TicksPerMeasure;
        private int TicksSinceLastTimeSignatureChange;

        private int beatsPerBar;
        private int beatValue;

        public const int TICKS_PER_QUARTERNOTE = 96;

        private static List<(int, string)> LilyPondDurations = new List<(int, string)>
        {
            ( TICKS_PER_QUARTERNOTE * 4 * 3 / 2, "1." ),
            ( TICKS_PER_QUARTERNOTE * 4, "1" ),
            ( TICKS_PER_QUARTERNOTE * 2 * 3 / 2, "2." ),
            ( TICKS_PER_QUARTERNOTE * 2, "2" ),
            ( TICKS_PER_QUARTERNOTE * 1 * 3 / 2, "4." ),
            ( TICKS_PER_QUARTERNOTE * 1, "4" ),
            ( TICKS_PER_QUARTERNOTE / 2 * 3 / 2, "8." ),
            ( TICKS_PER_QUARTERNOTE / 2, "8" ),
            ( TICKS_PER_QUARTERNOTE / 4 * 3 / 2, "16." ),
            ( TICKS_PER_QUARTERNOTE / 4, "16" ),
            ( TICKS_PER_QUARTERNOTE / 8 * 3 / 2, "32." ),
            ( TICKS_PER_QUARTERNOTE / 8, "32" ),
            ( TICKS_PER_QUARTERNOTE / 16 * 3 / 2, "64." ),
            ( TICKS_PER_QUARTERNOTE / 16, "64" ),
            ( TICKS_PER_QUARTERNOTE / 32 * 3 / 2, "128." ),
            ( TICKS_PER_QUARTERNOTE / 32, "128" ),
        };



        // HACK: make this public until the tuplet logic can be refactored
        public static List<(int, string)> TupletDurations = new List<(int, string)>
        {
            ((int)(TICKS_PER_QUARTERNOTE / 1.5), "4" ), // 64 ticks, quarter note triplets
            (TICKS_PER_QUARTERNOTE / 3, "8" ), // 32 ticks, eighth note triplets
            (TICKS_PER_QUARTERNOTE / 6, "16" ), // 16 ticks, sixteenth note triplets
        };

        public void SetTimeSignature(int beatsPerBar, int beatValue)
        {
            this.beatsPerBar = beatsPerBar;
            this.beatValue = beatValue;
            TicksPerMeasure = TICKS_PER_QUARTERNOTE * 4 / beatValue * beatsPerBar;
            TicksSinceLastTimeSignatureChange = 0;
        }

        public void AddTime(int duration)
        {
            //Tick += duration;
            TicksSinceLastTimeSignatureChange += duration;
        }

        public int[] GetBarlineTies(int duration)
        {
            //TODO: figure out or track the start of the measure
            // figure out which measure we're currently in, assume 4/4 for now
            //var ticksPerMeasure = TicksPerMeasure;
            var measure = TicksSinceLastTimeSignatureChange / TicksPerMeasure;
            var tickInMeasure = TicksSinceLastTimeSignatureChange % TicksPerMeasure;
            var ticksRemainingInMeasure = TicksPerMeasure - tickInMeasure;

            Console.Out.WriteLine("seems we're currently {0}/{1} ticks into measure {2}.  there are {3} ticks left in the measure.", tickInMeasure, TicksPerMeasure, measure, ticksRemainingInMeasure);

            Debug.Assert(0 < ticksRemainingInMeasure);
            Debug.Assert(0 < duration);

            if (duration <= ticksRemainingInMeasure)
            {
                return new int[] { duration, 0 };
            }
            else
            {
                List<int> durations = new List<int>();
                durations.Add(ticksRemainingInMeasure);
                durations.Add(duration - ticksRemainingInMeasure);
                Console.Out.WriteLine("Split duration {0} across bar lines into [{1}]", duration, string.Join(", ", durations));
                return durations.ToArray();
            }
        }

        public int[] GetNoteTies(int delta)
        {
            Debug.Assert(delta <= TicksPerMeasure, $"duration {delta} is too large to fit in a measure of ${beatsPerBar}/${beatValue}");
            List<int> parts = new List<int>();
            for (int i = 0; i < LilyPondDurations.Count; i++)
            {
                var (ticks, duration) = LilyPondDurations[i];
                if (ticks <= delta)
                {
                    parts.Add(ticks);
                    delta -= ticks;
                }
            }

            return ((IEnumerable<int>)parts).Reverse().ToArray();
        }

        public string ConvertToLilyPondDuration(int delta)
        {
            foreach (var (ticks, name) in LilyPondDurations)
            {
                if (delta == ticks)
                {
                    return name;
                }
                else if (delta == ticks * 1.5)
                {
                    return name + ".";
                }
            }
            Debug.Fail(string.Format("don't know how to convert duration {0} to LilyPond duration", delta));
            Console.Out.WriteLine("don't know how to convert duration {0} to LilyPond duration", delta);
            return "4";
        }
    }
}