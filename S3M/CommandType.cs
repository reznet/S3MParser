using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace S3M
{
    public enum CommandType
    {
        None,
        SetSpeed,						// Axx	   Set speed to xx (the default is 06)
        JumpToOrder,					// Bxx	   Jump to order xx (hexadecimal)
        BreakPatternToRow,				// Cxx	   Break pattern to row xx (decimal)
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
        // Note: the ST3 manual lists Uxy as a command, but it clashes with Uxx and Uxy appears to actually be an IT (ImpulseTracker) command
        //FineVibratoWithSpeedAndDepth,	// Uxy     Fine Vibrato with speed x and depth y
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

    public class CommandAndInfo
    {
        public CommandType Commmand { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public static CommandAndInfo Create(byte command, byte info)
        {
            char first = command.ToString("X")[0];
            int hi = (info & 0xF0) >> 4;
            int low = info & 0xF;
            char second = hi.ToString("X")[0];
            char third = low.ToString("X")[0];

            CommandType type = CommandType.None;
            int x = 0;
            int y = 0;

            switch(first)
            {
                case 'A':
                    type = CommandType.SetSpeed;
                    x = info;
                    break;
                case 'B':
                    type = CommandType.JumpToOrder;
                    x = info;
                    break;
                case 'C':
                    type = CommandType.BreakPatternToRow;
                    x = info;
                    break;
                case 'D':
                    switch (hi)
                    {
                        case 0: // F0x
                            type = CommandType.VolumeSlideDown;
                            y = low;
                            break;
                        case 16: // DFy
                            type = CommandType.FineVolumeSlideDown;
                            y = low;
                            break;
                        default: // Dx0
                            type = CommandType.FineVolumeSlideUp;
                            x = hi;
                            break;
                    }
                    break;
                case 'E':
                    Debug.Fail("TODO - handle E commands");
                    break;
                case 'F':
                    Debug.Fail("TODO - handle F commands");
                    break;
                case 'G':
                    type = CommandType.TonePortamento;
                    x = info;
                    break;
                case 'H':
                    type = CommandType.Vibrato;
                    x = hi;
                    y = low;
                    break;
                case 'I':
                    type = CommandType.Tremelo;
                    x = hi;
                    y = low;
                    break;
                case 'J':
                    type = CommandType.Arpeggio;
                    x = hi;
                    y = low;
                    break;
                case 'K':
                    type = CommandType.VibratoAndVolumeSlide;
                    x = hi;
                    y = low;
                    break;
                case 'L':
                    type = CommandType.TonePortamentoAndVolumeSlide;
                    x = hi;
                    y = low;
                    break;
                case 'O':
                    type = CommandType.SetSampleOffset;
                    x = hi;
                    y = low;
                    break;
                case 'Q':
                    type = CommandType.RetriggerWithVolumeSlide;
                    x = hi;
                    y = low;
                    break;
                case 'R':
                    type = CommandType.TremoloWithSpeed;
                    x = hi;
                    y = low;
                    break;
                case 'S':
                    Debug.Fail("TODO - handle S commands");
                    break;
                case 'T':
                    type = CommandType.SetTempo;
                    x = info;
                    break;
                case 'U':
                    type = CommandType.FineVibrato;
                    x = info;
                    break;
                case 'V':
                    type = CommandType.SetGlobalVolume;
                    x = info;
                    break;
                default:
                    Debug.Fail(string.Format("Unrecognized command {0}", first));
                    break;
            }

            return new CommandAndInfo() { Commmand = type, X = x, Y = y };
        }
    }
}
