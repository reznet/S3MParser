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
    public const int OneTwentyEighthNote = TICKS_PER_QUARTERNOTE / 32;

    // dotted durations

    public const int DottedWholeNote = WholeNote * 3 / 2;
    public const int DottedHalfNote = HalfNote * 3 / 2;
    public const int DottedQuarterNote = QuarterNote * 3 / 2;
    public const int DottedEighthNote = EighthNote * 3 / 2;
    public const int DottedSixteenthNote = SixteenthNote * 3 / 2;
    public const int DottedThirtySecondNote = ThirtySecondNote * 3 / 2;
    public const int DottedSixtyFourthNote = SixtyFourthNote * 3 / 2;
    public const int DottedOneTwentyEighthNote = OneTwentyEighthNote * 3 / 2;

    public const int WholeNoteTriplet = WholeNote * 2 / 3;
    public const int HalfNoteTriplet = HalfNote * 2 / 3;
    public const int QuarterNoteTriplet = QuarterNote * 2 / 3;
    public const int EighthNoteTriplet = EighthNote * 2 / 3;
    public const int SixteenthNoteTriplet = SixteenthNote * 2 / 3;
    public const int ThirtySecondNoteTriplet = ThirtySecondNote * 2 / 3;
    public const int SixtyFourthNoteTriplet = SixtyFourthNote * 2 / 3;
    public const int OneTwentyEighthNoteTriplet = OneTwentyEighthNote * 2 / 3;

}
