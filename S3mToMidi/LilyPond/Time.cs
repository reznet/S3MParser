using System.Diagnostics;

namespace S3mToMidi.LilyPond
{
    internal class Time
    {
        private int TickInMeasure;
        public int TicksPerMeasure;
        private int TicksSinceLastTimeSignatureChange;
        private int TotalTicks;

        private int beatsPerBar;
        private int beatValue;

        private RhythmGrid grid;

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

        public bool SetTimeSignature(int beatsPerBar, int beatValue)
        {
            if (this.beatsPerBar == beatsPerBar && this.beatValue == beatValue) { return false; }
            this.beatsPerBar = beatsPerBar;
            this.beatValue = beatValue;
            TicksPerMeasure = Durations.WholeNote / beatValue * beatsPerBar;
            TicksSinceLastTimeSignatureChange = 0;
            grid = new RhythmGrid(this.beatsPerBar, Durations.WholeNote / this.beatValue);
            return true;
        }

        public void AddTime(int duration)
        {
            TotalTicks += duration;
            TicksSinceLastTimeSignatureChange += duration;
            var measure = TicksSinceLastTimeSignatureChange / TicksPerMeasure;
            TickInMeasure = TicksSinceLastTimeSignatureChange % TicksPerMeasure;
            Console.Out.WriteLine("Added {0} ticks to time.  Measure is {1}. TickInMeasure is now {2}", duration, measure, TickInMeasure);
        }

        public int[] GetBarlineTies(int duration)
        {
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
            List<int> ties = new List<int>();
            int tick = TickInMeasure;
            int remainingDuration = duration;
            while (0 < remainingDuration)
            {
                var l = grid.GetDurationOptionsByStartTime(tick);
                Debug.Assert(l != null, "don't know where to start");
                // find a duration that can be solved
                int subDuration = l.FirstOrDefault(d => d <= remainingDuration);
                Debug.Assert(subDuration != default, "can't find a subduration");
                tick += subDuration;
                remainingDuration -= subDuration;
                ties.Add(subDuration);
            }

            Console.Out.WriteLine("Split duration {0} inside a measure starting at {1} into [{2}]", duration, TickInMeasure, string.Join(", ", ties));

            int sumOfTies = ties.Sum();
            Debug.Assert(sumOfTies == duration, $"Ties sum up to {sumOfTies} which does not match input duration {duration}");
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