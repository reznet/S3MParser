using System.Collections.ObjectModel;
using System.Diagnostics;

namespace S3mToMidi.LilyPond
{
    public class RhythmGrid : Subdivider
    {
        private int minimumSubdivision = 192;

        private readonly SortedDictionary<int, List<int>> durationOptionsByTime;

        public RhythmGrid(int beatsPerBar, int beatValue) : base(beatsPerBar, beatValue)
        {
            durationOptionsByTime = GetRhythmGrid(MeasureDuration);
        }

        public ReadOnlyCollection<int> GetDurationOptionsByStartTime(int startTime)
        {
            var options = durationOptionsByTime.Where(pair => pair.Key == startTime).Select(pair => pair.Value).FirstOrDefault();
            Debug.Assert(options != null, "there are not any available duration options at time " + startTime);
            if (options == null)
            {
                throw new Exception("Cannot add any notes at measure time " + startTime);
            }
            return options.AsReadOnly();
        }

        private SortedDictionary<int, List<int>> GetRhythmGrid(int measureDuration)
        {
            SortedDictionary<int, List<int>> grid = new SortedDictionary<int, List<int>>();

            int n = Durations.WholeNote;

            for (int i = 2; i <= (int)Math.Sqrt(64); i++)
            {
                int time = 0;
                decimal subdivision = n / (int)Math.Pow(2, i);
                int intSubdivision = (int)subdivision;
                while (time + intSubdivision <= measureDuration)
                {
                    if (!grid.TryGetValue(time, out List<int>? durations))
                    {
                        durations = new List<int>();
                        grid.Add(time, durations);
                    }

                    if (!durations.Contains(intSubdivision)) { durations.Add(intSubdivision); } // todo perf
                    time += intSubdivision;
                }
            }

            // add tuplets
            for (int i = 24; i <= 64; i *= 2)
            {
                int time = 0;
                decimal subdivision = (decimal)n * 2 / i;
                if (!decimal.IsInteger(subdivision)) { continue; }
                int intSubdivision = (int)subdivision;
                while (time + intSubdivision <= measureDuration)
                {
                    if (!grid.TryGetValue(time, out List<int>? durations))
                    {
                        durations = new List<int>();
                        grid.Add(time, durations);
                    }

                    if (!durations.Contains(intSubdivision)) { durations.Add(intSubdivision); } // todo perf
                    time += intSubdivision;
                }
            }

            // add dotted rhythms
            for (int j = 1; j < 8; j++)
            {
                int d = (int)Math.Pow(2, j);

                // subdivision is the duration of the time unit
                // e.g. dotted eighth note = whole note / eighth note * 1.5
                int subdivision = n / d * 3 / 2;
                int time = 0;
                while (time + subdivision <= measureDuration)
                {
                    // do not let the duration cross a beat
                    // e.g. in 4/4, only add a dotted eighth note if
                    // it complete fits within a quarter of the measure
                    int nextTime = time + subdivision;
                    int left = time / BeatValue;
                    int right = nextTime / BeatValue;
                    if (nextTime % BeatValue == 0){ right = Math.Max(0, right - 1); }
                    if (left == right)
                    {
                        if (!grid.TryGetValue(time, out List<int>? durations))
                        {
                            durations = new List<int>();
                            grid.Add(time, durations);
                        }

                        if (!durations.Contains(subdivision)) { durations.Add(subdivision); }
                    }

                    time += subdivision;
                }
            }

            // ensure each lookup is sorted descending so we can later find longest duration that fits
            foreach (var pair in grid)
            {
                grid[pair.Key].Sort((a, b) => b.CompareTo(a));
            }

            return grid;

        }

        public override int GetNextSubdivision(int startTime, int endTime)
        {
            var options = GetDurationOptionsByStartTime(startTime);
            return options.FirstOrDefault(d => d <= endTime - startTime);
        }

        public int GetNextSubdivisionPreferBeats(int startTime, int endTime)
        {
            var beats = Enumerable.Range(1, MeasureDuration / Durations.SixteenthNote).Select(e => Durations.SixteenthNote * e).ToArray();
            var options = GetDurationOptionsByStartTime(startTime).Where(duration => duration <= (endTime - startTime));
            // prefer durations that lead us to an even measure division
            // i.e. prefer dotted eighth note to quarter note triplet + sixtyfourth triplet
            var x = options.Select(duration => 
            {
                var minimumDistanceToBeats = beats.Select(beat => beat - (startTime + duration)).Where(s => 0 <= s).OrderBy(s => s);
                var minimumDistanceToBeat = minimumDistanceToBeats.First();
                return new KeyValuePair<int, int>(minimumDistanceToBeat, duration);
            }).OrderBy(pair => pair.Key);
            return x.Select(y => y.Value).FirstOrDefault();
        }
    }
}