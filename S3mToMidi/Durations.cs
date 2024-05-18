using Melanchall.DryWetMidi.Multimedia;

namespace S3mToMidi;

public class Durations
{
    private const int TICKS_PER_QUARTERNOTE = 96;

    public const int WholeNote = TICKS_PER_QUARTERNOTE * 4;
    public const int HalfNote = TICKS_PER_QUARTERNOTE * 2;
    public const int QuarterNote = TICKS_PER_QUARTERNOTE;
    public const int EighthNote = TICKS_PER_QUARTERNOTE / 2;
    public const int SixteenthNote = TICKS_PER_QUARTERNOTE / 4;
    public const int ThirtySecondNote = TICKS_PER_QUARTERNOTE / 8;
    public const int SixtyFourthNote = TICKS_PER_QUARTERNOTE / 16;

    // dotted durations

    public const int DottedWholeNote = WholeNote * 3 / 2;
    public const int DottedHalfNote = HalfNote * 3 / 2;
    public const int DottedQuarterNote = QuarterNote * 3 / 2;
    public const int DottedEighthNote = EighthNote * 3 / 2;
    public const int DottedSixteenthNote = SixteenthNote * 3 / 2;
    public const int DottedThirtySecondNote = ThirtySecondNote * 3 / 2;
    public const int DottedSixtyFourthNote = SixtyFourthNote * 3 / 2;

}
