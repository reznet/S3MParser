using System.Diagnostics;

namespace S3mToMidi.LilyPond
{
    internal class Time
    {
        //private int Tick;
        private int TickInMeasure;
        public int TicksPerMeasure;
        private int TicksSinceLastTimeSignatureChange;

        private int TotalTicks;

        private int beatsPerBar;
        private int beatValue;

        public const int TICKS_PER_QUARTERNOTE = 96;

        private static List<(int, string)> LilyPondDurations = new List<(int, string)>
        {
            ( Durations.DottedWholeNote, "1." ),
            ( Durations.WholeNote, "1" ),
            ( Durations.WholeNoteTriplet, "\\tuplet 3/2 { 1 }"),
            ( Durations.DottedHalfNote, "2." ),
            ( Durations.HalfNote, "2" ),
            ( Durations.HalfNoteTriplet, "\\tuplet 3/2 { 2 }"),
            ( Durations.DottedQuarterNote, "4." ),
            ( Durations.QuarterNote * 1, "4" ),
            ( Durations.QuarterNoteTriplet, "\\tuplet 3/2 { 4 }"),
            ( Durations.DottedEighthNote, "8." ),
            ( Durations.EighthNote, "8" ),
            ( Durations.EighthNoteTriplet, "\\tuplet 3/2 { 8 }"),
            ( Durations.DottedSixteenthNote, "16." ),
            ( Durations.SixteenthNote, "16" ),
            ( Durations.SixteenthNoteTriplet, "\\tuplet 3/2 { 16 }"),
            ( Durations.DottedThirtySecondNote, "32." ),
            ( Durations.ThirtySecondNote, "32" ),
            ( Durations.ThirtySecondNoteTriplet, "\\tuplet 3/2 { 32 }"),
            ( Durations.DottedSixtyFourthNote, "64." ),
            ( Durations.SixtyFourthNote, "64" ),
            ( Durations.SixtyFourthNoteTriplet, "\\tuplet 3/2 { 64 }"),
            ( Durations.DottedOneTwentyEighthNote * 3 / 2, "128." ),
            ( Durations.OneTwentyEighthNote, "128" ),
            ( Durations.OneTwentyEighthNoteTriplet, "\\tuplet 3/2 { 128 }"),
        };



        // HACK: make this public until the tuplet logic can be refactored
        public static List<(int, string)> TupletDurations = new List<(int, string)>
        {
            (Durations.QuarterNoteTriplet, "4" ), // 64 ticks, quarter note triplets
            (Durations.EighthNoteTriplet, "8" ), // 32 ticks, eighth note triplets
            (Durations.SixteenthNoteTriplet, "16" ), // 16 ticks, sixteenth note triplets
            (Durations.ThirtySecondNoteTriplet, "32" ), // 8 ticks, thirtysecond note triplets
            (Durations.QuarterNoteTriplet, "64" ), // 4 ticks, sixtyfourth note triplets
        };

        public bool SetTimeSignature(int beatsPerBar, int beatValue)
        {
            if (this.beatsPerBar == beatsPerBar && this.beatValue == beatValue) { return false; }
            this.beatsPerBar = beatsPerBar;
            this.beatValue = beatValue;
            TicksPerMeasure = Durations.WholeNote / beatValue * beatsPerBar;
            TicksSinceLastTimeSignatureChange = 0;
            return true;
        }

        public void AddTime(int duration)
        {
            //Tick += duration;
            TotalTicks += duration;
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

            Console.Out.WriteLine("seems we're currently {0}/{1} ticks into measure {2} since the last time signature change (global tick {3}).  there are {4} ticks left in the measure.", tickInMeasure, TicksPerMeasure, measure, TotalTicks, ticksRemainingInMeasure);

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

        internal List<(int subdivisionDuration, bool[] subdivisions)> GetSubdivisionCells(int subdivision, int tickInMeasure, int duration)
        {
            // whole note subdivision = 0, subdivisions = 1;
            // half note subdivision = 1, subdivisions = 2;
            // quarter note subdivision = 2, subdivisions = 4;
            int subdivisionDuration = Durations.WholeNote / (int)Math.Pow(2, subdivision);
            int tripletSubdivisionDuration = subdivisionDuration / 3;

            return new []{
                GetSubdivisionCellsForCellDuration(subdivisionDuration, tickInMeasure, duration),
                GetSubdivisionCellsForCellDuration(tripletSubdivisionDuration, tickInMeasure, duration),
            }.ToList();
        }

        internal (int subdivisionDuration, bool[] subdivisions) GetSubdivisionCellsForCellDuration(int subdivisionDuration, int tickInMeasure, int duration)
        {
            // x/1 => whole note beat
            // x/2 => half note beat
            // x/4 => quarter note beat
            int measureDuration = this.beatsPerBar * Durations.WholeNote / this.beatValue;

            int numberOfSubdivisionsInMeasure = (int)Math.Ceiling((decimal)measureDuration / subdivisionDuration);

            bool[] subdivisions = DurationSubdivider.GetSubdivisionCells(numberOfSubdivisionsInMeasure, subdivisionDuration, tickInMeasure, duration);

            return (subdivisionDuration, subdivisions);
        }
        
        public int[] GetNoteTies(int duration)
        {
            Debug.Assert(0 < duration, "trying to get note ties for zero duration");
            List<int> ties = new List<int>();

            var subdivisionCells = new List<(int subdivisionDuration, bool[] subdivisionCells)>();

            for (int subdivision = 0; subdivision < 8; subdivision++)
            {
                subdivisionCells.AddRange(GetSubdivisionCells(subdivision, TickInMeasure, duration));
            }

            subdivisionCells.Sort((l, r) => r.subdivisionDuration.CompareTo(l.subdivisionDuration));

            int sum = 0;
            int loopCount = 0;
            int maxLoop = 1024;
            int minCellDuration = int.MaxValue;
            int offset = 0;
            bool foundAnyCell = false;

            while (offset < TicksPerMeasure && sum < duration && loopCount++ < maxLoop)
            {
                bool foundCell = false;
                foreach ((int subdivisionDuration, bool[] cells) in subdivisionCells)
                {
                    minCellDuration = Math.Min(minCellDuration, subdivisionDuration);
                    int index = (offset + sum) / subdivisionDuration;
                    if (cells[index])
                    {
                        ties.Add(subdivisionDuration);
                        sum += subdivisionDuration;
                        foundCell = true;
                        foundAnyCell = true;
                        break;
                    }
                }
                if (!foundCell)
                {
                    if (foundAnyCell)
                    {
                        // no need to keep looking
                        break;
                    }
                    offset += minCellDuration;
                }
            }

            Debug.Assert(loopCount < maxLoop, "infinite loop");

            Console.Out.WriteLine("Split duration {0} inside a measure starting at {1} into [{2}]", duration, TickInMeasure, string.Join(", ", ties));

            Debug.Assert(0 < ties.Count, "GetNoteTies is not returning any durations");
            return ties.ToArray();
        }

        public string ConvertToLilyPondDuration(int delta)
        {
            foreach (var (ticks, name) in LilyPondDurations)
            {
                if (delta == ticks)
                {
                    return name;
                }
            }
            Debug.Fail(string.Format("don't know how to convert duration {0} to LilyPond duration", delta));
            Console.Out.WriteLine("don't know how to convert duration {0} to LilyPond duration", delta);
            return "4";
        }
    }
}