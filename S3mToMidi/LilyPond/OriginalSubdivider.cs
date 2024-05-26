using System.Diagnostics;

namespace S3mToMidi.LilyPond
{
    public class OriginalSubdivider : Subdivider
    {
        public OriginalSubdivider(int beatsPerBar, int beatValue) : base(beatsPerBar, beatValue) { }

        public override int[] GetSubdivisions(int startTime, int endTime)
        {
            int duration = endTime - startTime;
            int TickInMeasure = startTime;
            int TicksPerMeasure = this.MeasureDuration;

            bool durationIsMultipleOfDuplet = duration % Durations.SixtyFourthNote == 0;

            Debug.Assert(0 < duration, "trying to get note ties for zero duration");
            List<int> ties = new List<int>();

            var subdivisionCells = new List<(int subdivisionDuration, bool[] subdivisionCells)>();

            for (int subdivision = 0; subdivision < 8; subdivision++)
            {
                subdivisionCells.AddRange(GetSubdivisionCells(durationIsMultipleOfDuplet ? 2 : 3, subdivision, TickInMeasure, duration));
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
                        Debug.Assert(ties.Sum() + subdivisionDuration <= duration, "adding " + subdivisionDuration + " would put sum of ties over target duration");
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

            int sumOfTies = ties.Sum();
            Debug.Assert(sumOfTies == duration, $"Ties sum up to {sumOfTies} which does not match input duration {duration}");
            Debug.Assert(0 < ties.Count, "GetNoteTies is not returning any durations");
            return ties.ToArray();
        }

        internal (int subdivisionDuration, bool[] subdivisions) GetSubdivisionCellsForCellDuration(int subdivisionDuration, int tickInMeasure, int duration)
        {
            // x/1 => whole note beat
            // x/2 => half note beat
            // x/4 => quarter note beat
            int measureDuration = this.BeatsPerBar * Durations.WholeNote / this.BeatValue;

            int numberOfSubdivisionsInMeasure = (int)Math.Ceiling((decimal)measureDuration / subdivisionDuration);

            bool[] subdivisions = DurationSubdivider.GetSubdivisionCells(numberOfSubdivisionsInMeasure, subdivisionDuration, tickInMeasure, duration);

            return (subdivisionDuration, subdivisions);
        }

        internal List<(int subdivisionDuration, bool[] subdivisions)> GetSubdivisionCells(int divisor, int subdivision, int tickInMeasure, int duration)
        {
            // whole note subdivision = 0, subdivisions = 1;
            // half note subdivision = 1, subdivisions = 2;
            // quarter note subdivision = 2, subdivisions = 4;
            int subdivisionDuration = Durations.WholeNote * 2 / (int)Math.Pow(2, subdivision) / divisor;

            return new[]{
                GetSubdivisionCellsForCellDuration(subdivisionDuration, tickInMeasure, duration),
            }.ToList();
        }
    }
}