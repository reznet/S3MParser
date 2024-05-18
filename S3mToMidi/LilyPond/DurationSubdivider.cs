namespace S3mToMidi;

public class DurationSubdivider
{
    internal static bool[] GetSubdivisionCells(int numberOfSubdivisionsInMeasure, int subdivisionDuration, int tickInMeasure, int duration)
    {
        var tempDuration = duration;
        var subdivisionCells = new bool[numberOfSubdivisionsInMeasure];
        var cellDuration = subdivisionDuration;

        // skip leading rests
        var cellIndex = (int)Math.Ceiling((decimal)tickInMeasure / cellDuration);

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
