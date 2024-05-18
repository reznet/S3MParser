namespace S3mToMidi;

public class DurationSubdivider
{
    internal static bool[] GetSubdivisionCells(int subdivision, int tickInMeasure, int duration)
    {
        var tempDuration = duration;
        var numberOfSubdivisionsInMeasure = (int)Math.Pow(2, subdivision);
        var subdivisionCells = new bool[numberOfSubdivisionsInMeasure];
        var cellDuration = Durations.QuarterNote * 4 / numberOfSubdivisionsInMeasure;

        // skip leading rests
        var cellIndex = (int)Math.Ceiling((double)tickInMeasure / (double)cellDuration);

        // round up to next cell
        //tickInMeasure = index * cellDuration;
        tempDuration -= (cellIndex * cellDuration) - tickInMeasure;

        //Debug.Assert(cellIndex < subdivisionCells[subdivision].Length);
        while (cellIndex < subdivisionCells.Length && cellDuration <= tempDuration)
        {
            subdivisionCells[cellIndex] = true;
            tempDuration -= cellDuration;
            cellIndex++;
        }

        return subdivisionCells;
    }
}
