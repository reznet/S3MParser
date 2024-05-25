using System.Collections.ObjectModel;
using System.Diagnostics;

namespace S3mToMidi.LilyPond
{
    public class RhythmGrid
    {
        private int minimumSubdivision = 192;

        private readonly int beatsPerBar;
        private readonly int beatValue;

        private readonly SortedDictionary<int, List<int>> durationOptionsByTime;

        public RhythmGrid(int beatsPerBar, int beatValue)
        {
            this.beatsPerBar = beatsPerBar;
            this.beatValue = beatValue;
            durationOptionsByTime = GetRhythmGrid(beatsPerBar * beatValue);
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

            for (int i = 2; i <= minimumSubdivision; i++)
            {
                int time = 0;
                decimal subdivision = (decimal)n * 2 / i;
                if (!decimal.IsInteger(subdivision)) { continue; }
                int intSubdivision = (int)subdivision;
                while (time + intSubdivision <= measureDuration)
                {
                    // do not let the duration cross a beat
                    // e.g. in 4/4, only add a dotted eighth note if
                    // it complete fits within a quarter of the measure
                    int nextTime = time + intSubdivision;

                    int timeLevel = 0;
                    int nextTimeLevel = 0;

                    for (int level = 2; level < 8; level++)
                    {
                        var unit = (int)Math.Pow(2, level);
                        if (timeLevel == 0 && time % unit == 0) { timeLevel = level; }
                        if (nextTimeLevel == 0 && nextTime % unit == 0) { nextTimeLevel = level; }
                    }

                    if (Math.Abs(timeLevel - nextTimeLevel) <= 2)
                    {
                        int left = time / beatValue;
                        int right = nextTime / beatValue;
                        if (nextTime % beatValue == 0) { right = Math.Max(0, right - 1); }
                        if (left == right)
                        {
                            if (!grid.TryGetValue(time, out List<int>? durations))
                            {
                                durations = new List<int>();
                                grid.Add(time, durations);
                            }

                            if (!durations.Contains(intSubdivision)) { durations.Add(intSubdivision); } // todo perf
                        }
                    }

                    time += intSubdivision / 2;
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
                    int left = time / beatValue;
                    int right = nextTime / beatValue;
                    if (nextTime % beatValue == 0) { right = Math.Max(0, right - 1); }
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

    }
}