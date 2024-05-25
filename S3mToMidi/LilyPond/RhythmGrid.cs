using System.Collections.ObjectModel;

namespace S3mToMidi.LilyPond
{
    public class RhythmGrid
    {
        private int minimumSubdivision = 192;

        private SortedDictionary<int, List<int>> durationOptionsByTime;

        public RhythmGrid(int duration)
        {
            durationOptionsByTime = GetRhythmGrid(duration);
        }

        public RhythmGrid(int duration, int minimumSubdivision)
        {
            this.minimumSubdivision = minimumSubdivision;
            durationOptionsByTime = GetRhythmGrid(duration);
        }

        public ReadOnlyCollection<int> GetDurationOptionsByStartTime(int startTime)
        {
            return durationOptionsByTime.Where(pair => pair.Key == startTime).Select(pair => pair.Value).First().AsReadOnly();
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
                    
                    if (!grid.TryGetValue(time, out List<int>? durations))
                    {
                        durations = new List<int>();
                        grid.Add(time, durations);
                    }

                    if(!durations.Contains(intSubdivision)){ durations.Add(intSubdivision); } // todo perf
                    time += intSubdivision;
                }
            }

            // add dotted rhythms
            for(int j = 1; j< 8; j++)
            {
                int d = (int)Math.Pow(2, j);
                int subdivision = n / d * 3 / 2;
                int time = 0;
                while (time + subdivision <= measureDuration)
                {
                    if (!grid.TryGetValue(time, out List<int>? durations))
                    {
                        durations = new List<int>();
                        grid.Add(time, durations);
                    }

                    if(!durations.Contains(subdivision)){ durations.Add(subdivision); } // todo perf
                    time += subdivision * 2;
                }
            }

            // ensure each lookup is sorted descending so we can later find longest duration that fits
            foreach(var pair in grid)
            {
                grid[pair.Key].Sort((a, b) => b.CompareTo(a));
            }

            return grid;

        }

    }
}