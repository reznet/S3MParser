using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S3M
{
    public enum CommandType
    {
        None,
        SetSpeed,						// Axx		Set speed to xx (the default is 06)
        JumpToOrder,					// Bxx		Jump to order xx (hexadecimal)
        BreakPatternToRow,				// Cxx		Break pattern to row xx (decimal)
        VolumeSlideDown,				// D0y     Volume slide down by y
        VolumeSlideUp,					// Dx0     Volume slide up by x
        FineVolumeSlideDown,			// DFy     Fine volume down by y
        FineVolumeSlideUp,				// DxF     Fine volume up by x
        PitchSlideDown,					// Exx     Slide down by xx
        FinePitchSlideDown,				// EFx     Fine slide down by x
        ExtraFinePitchSlideDown,		// EEx     Extra fine slide down by x
        PitchSlideUp,					// Fxx     Slide up by xx
        FinePitchSlideUp,				// FFx     Fine slide up by x
        ExtraFinePitchSlideUp,			// FEx     Extra fine slide up by x
        TonePortamento,					// Gxx     Tone portamento with speed xx
        Vibrato,						// Hxy     Vibrato with speed x and depth y
        Tremelo,						// Ixy     Tremor with ontime x and offtime y
        Arpeggio,						// Jxy     Arpeggio with halfnote additions of x and y
        VibratoAndVolumeSlide,			// Kxy     Dual command: H00 and Dxy
        TonePortamentoAndVolumeSlide,	// Lxy     Dual command: G00 and Dxy
        SetSampleOffset,				// Oxy     Set sample offset
        RetriggerWithVolumeSlide,		// Qxy     Retrig (+volumeslide) note
        TremoloWithSpeed,				// Rxy     Tremolo with speed x and depth y
        FineVibratoWithSpeedAndDepth,	// Uxy     Fine Vibrato with speed x and depth y
        SetFilter,						// S0x     Set filter
        SetGlissando,					// S1x     Set glissando control
        SetFinetune,					// S2x     Set finetune (=C4Spd)
        SetVibratoWaveform,				// S3x     Set vibrato waveform to x
        SetTremoloWaveform,				// S4x     Set tremolo waveform to x
        SetChannelPan,					// S8x     Set channel pan position
        StereoControl,					// SAx     Stereo control
        PatternLoop,					// SBx     Pattern loop.
        Notecut,						// SCx     Notecut in x frames
        Notedelay,						// SDx     Notedelay for x frames
        PatternDelay,					// SEx     Patterndelay for x notes
        FunkRepeat,						// SFx     Funkrepeat with speed x
        SetTempo,						// Txx     Tempo = xx (hex)
        FineVibrato,					// Uxx     Fine vibrato
        SetGlobalVolume 				// Vxx     Set global volume Accepted values are 0 to 40.
    }

    public static class CommandTypeExtensions
    {
        public static CommandType ToCommandType(this byte value)
        {
            return (CommandType)value;
        }
    }
}
