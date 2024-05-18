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
            (TICKS_PER_QUARTERNOTE / 12, "32" ), // 8 ticks, thirtysecond note triplets
            (TICKS_PER_QUARTERNOTE / 24, "64" ), // 4 ticks, sixtyforth note triplets
        };

        public bool SetTimeSignature(int beatsPerBar, int beatValue)
        {
            if (this.beatsPerBar == beatsPerBar && this.beatValue == beatValue) { return false; }
            this.beatsPerBar = beatsPerBar;
            this.beatValue = beatValue;
            TicksPerMeasure = TICKS_PER_QUARTERNOTE * 4 / beatValue * beatsPerBar;
            TicksSinceLastTimeSignatureChange = 0;
            return true;
        }

        public void AddTime(int duration)
        {
            //Tick += duration;
            TicksSinceLastTimeSignatureChange += duration;
            var measure = TicksSinceLastTimeSignatureChange / TicksPerMeasure;
            TickInMeasure = TicksSinceLastTimeSignatureChange % TicksPerMeasure;
            Console.Out.WriteLine("Added {0} ticks to time.  Measure is {1}. TickInMeasure is now {2}", duration, measure, TickInMeasure);
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
                return new int[] { duration };
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

        public int[] GetNoteTiesOld(int delta)
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

        internal (int subdivisionDuration, bool[] subdivisions) GetSubdivisionCells(int subdivision, int tickInMeasure, int duration)
        {
            // x/1 => whole note beat
            // x/2 => half note beat
            // x/4 => quarter note beat
            int measureDuration = this.beatsPerBar * Durations.WholeNote / this.beatValue;

            // whole note subdivision = 0, subdivisions = 1;
            // half note subdivision = 1, subdivisions = 2;
            // quarter note subdivision = 2, subdivisions = 4;
            int subdivisionDuration = Durations.WholeNote / (int)Math.Pow(2, subdivision);
            int numberOfSubdivisionsInMeasure = (int)Math.Ceiling((decimal)measureDuration / subdivisionDuration);

            bool[] subdivisions = DurationSubdivider.GetSubdivisionCells(numberOfSubdivisionsInMeasure, subdivisionDuration, tickInMeasure, duration);

            return (subdivisionDuration, subdivisions);
        }



        public int[] GetNoteTies(int duration)
        {
            Debug.Assert(0 < duration, "trying to get note ties for zero duration");
            List<int> ties = new List<int>();

            var subdivisionCells = new Dictionary<int, (int subdivisionDuration, bool[] subdivisionCells)>();

            for (int subdivision = 0; subdivision < 8; subdivision++)
            {
                subdivisionCells.Add(subdivision, GetSubdivisionCells(subdivision, TickInMeasure, duration));
            }

            int sum = 0;
            int loopCount = 0;
            int maxLoop = 100;
            int minCellDuration = int.MaxValue;
            int offset = 0;

            while (offset < duration && sum < duration && loopCount++ < maxLoop)
            {
                bool foundCell = false;
                foreach (var pair in subdivisionCells)
                {
                    (int subdivisionDuration, bool[] cells) = pair.Value;
                    minCellDuration = Math.Min(minCellDuration, subdivisionDuration);
                    int index = (offset + sum) / subdivisionDuration;
                    if (cells[index])
                    {
                        ties.Add(subdivisionDuration);
                        sum += subdivisionDuration;
                        foundCell = true;
                        break;
                    }
                }
                if (!foundCell)
                {
                    offset += minCellDuration;
                }
            }

            Debug.Assert(loopCount < maxLoop, "infinite loop");

            Console.Out.WriteLine("Split duration {0} inside a measure starting at {1} into [{2}]", duration, TickInMeasure, string.Join(", ", ties));

            Debug.Assert(0 < ties.Count, "GetNoteTies is not returning any durations");
            return ties.ToArray();

            /*
                        // minimum note is 128th note.  32 per quarter note
                        var index = 0;
                        var cells = new bool[128];
                        var cellDuration = TICKS_PER_QUARTERNOTE / 32;
                        for (int i = 0; i < tickInMeasure; i++)
                        {
                            index ++;;
                            i += cellDuration;
                        }

                        Debug.Assert(index < cells.Length);
                        while (index < cells.Length && 0 < duration)
                        {
                            cells[index] = true;
                            duration -= cellDuration;
                        }
                        */
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