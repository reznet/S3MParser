using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace S3M
{
    public class ChannelEvent
    {
        public int ChannelNumber;
        public int Note = -1;
        public int Instrument = -1;
        public int Volume = -1;
        public CommandType Command = CommandType.None;
        public char[] CommandAndInfo;
        public int Data = -1;
        public Row Row;
        public Pattern Pattern
        {
            get
            {
                return this.Row.Pattern;
            }
        }

        public NoteAction NoteAction
        {
            get
            {
                switch (Note)
                {
                    case -1:
                    case 0xFF:
                        return S3M.NoteAction.None;
                    case 0xFE:
                        return S3M.NoteAction.Stop;
                    default:
                        return S3M.NoteAction.Start;
                }
            }
        }

        public bool HasVolume
        {
            get { return Volume > 0; }
        }

        public bool HasInstrument
        {
            get { return Instrument > 0; }
        }

        public ChannelEvent()
        {
            // and empty/default command/info part of a cell is .00
            CommandAndInfo = new char[3];
            CommandAndInfo[0] = '.';
            CommandAndInfo[1] = '0';
            CommandAndInfo[2] = '0';
        }

        public override string ToString()
        {
            return String.Format("Pattern:{6:D2} Row:{7:D2} Channel:{0:D2} Note:{1} Instrument:{2} Volume:{3} Command:{4} Data:{5}",
                this.ChannelNumber,
                this.Note,
                this.Instrument,
                this.Volume,
                this.Command.ToString(),
                this.Data,
                this.Row.Pattern.PatternNumber,
                this.Row.RowNumber);
        }

        private const byte CHANNEL_MASK = 31;                       // 0x00011111;
        private const byte NOTE_INSTRUMENT_FOLLOWS_MASK = 1 << 5;   // 0x00100000;
        private const byte VOLUME_FOLLOWS_MASK = 1 << 6;            // 0x01000000;
        private const byte COMMAND_FOLLOWS_MASK = 1 << 7;           // 0x10000000;

        internal static ChannelEvent Parse(System.IO.Stream stream, System.IO.BinaryReader reader)
        {
            ChannelEvent channelEvent = new ChannelEvent();
            byte first = reader.ReadByte();
            if (first == 0)
            {
                return null;
            }
            channelEvent.ChannelNumber = 1 + first & CHANNEL_MASK;
            //Debug.Assert(channelEvent.ChannelNumber <= 6, "channel number should be less than 6 but is " + channelEvent.ChannelNumber);
            if ((first & NOTE_INSTRUMENT_FOLLOWS_MASK) == NOTE_INSTRUMENT_FOLLOWS_MASK)
            {
                // read note and instrument
                channelEvent.Note = reader.ReadByte();
                channelEvent.Instrument = reader.ReadByte();
            }
            if ((first & VOLUME_FOLLOWS_MASK) == VOLUME_FOLLOWS_MASK)
            {
                channelEvent.Volume = reader.ReadByte();
            }
            if ((first & COMMAND_FOLLOWS_MASK) == COMMAND_FOLLOWS_MASK)
            {
                byte commandByte = reader.ReadByte();
                byte dataByte = reader.ReadByte();

                if ((int)commandByte > 0)
                {
                    channelEvent.CommandAndInfo[0] = (char)((int)'A' - 1 + (int)commandByte);
                    if (channelEvent.CommandAndInfo[0] == 'A')
                    {
                        Debugger.Break();
                    }
                }

                if ((int)dataByte > 0)
                {
                    int hi = (dataByte & 0xF0) >> 6;
                    int low = dataByte & 0xF;

                    channelEvent.CommandAndInfo[1] = (char)((int)'A' + hi);
                    channelEvent.CommandAndInfo[2] = (char)low;
                }

                char commandChar = (char)((int)'A' + (int)commandByte);
                bool commandPartInData = false;

                // single argument "simple" commands (xx)
                // A, B, C, G, T, U, V
                //
                // double argument simple commands (xy)
                // H, I, J, K, L, O, Q, R, U
                //
                // two part command names:
                // D0y, Dx0, DFy, DxF
                // EFx, EEx,
                // Fxx, FFx, FEx
                // S0, S1, S2, S3, S4, S8
                // SA, SB, SC, SD, SE, SF

                Dictionary<char, Action<char, char, ChannelEvent>> map = new Dictionary<char, Action<char, char, ChannelEvent>>() 
                {
                    { 'A', ReadSingleArgumentCommand},
                    { 'B', ReadSingleArgumentCommand},
                    { 'C', ReadSingleArgumentCommand},
                    { 'D', ReadDCommand},
                    { 'E', ReadECommand},
                    { 'F', ReadFCommand},
                    { 'G', ReadSingleArgumentCommand},
                    { 'H', ReadDoubleArgumentCommand},
                    { 'I', ReadDoubleArgumentCommand},
                    { 'J', ReadDoubleArgumentCommand},
                    { 'K', ReadDoubleArgumentCommand},
                    { 'L', ReadDoubleArgumentCommand},
                    { 'M', ReadSingleArgumentCommand},
                    { 'N', ReadSingleArgumentCommand},
                    { 'O', ReadDoubleArgumentCommand},
                    { 'P', ReadSingleArgumentCommand},
                    { 'Q', ReadDoubleArgumentCommand},
                    { 'R', ReadDoubleArgumentCommand},
                    { 'S', ReadSCommand},
                    { 'T', ReadSingleArgumentCommand},
                    { 'U', ReadDoubleArgumentCommand},
                    { 'V', ReadSingleArgumentCommand},
                    { 'W', ReadSingleArgumentCommand},
                    { 'X', ReadSingleArgumentCommand},
                    { 'Y', ReadSingleArgumentCommand},
                    { 'Z', ReadSingleArgumentCommand},
                };

                char char1 = (char)((int)'A' - 1 + (int)commandByte);
                char char2 = (char)((int)'A' + ((dataByte & 0xF0) >> 6));
                char char3 = (char)(dataByte & 0xF);
                //channelEvent.Command = commandByte.ToCommandType();
                //channelEvent.Command = map[char1](char2, char3);
                map[char1](char2, char3, channelEvent);

                Console.WriteLine("Got command byte {0} which is command type {1} and data is {2}", commandByte, channelEvent.Command, channelEvent.Data);
            }

            return channelEvent;
        }

        private static void ReadSingleArgumentCommand(char command, char data, ChannelEvent channelEvent)
        { }

        private static void ReadDoubleArgumentCommand(char command, char data, ChannelEvent channelEvent)
        { }

        private static void ReadDCommand(char command, char data, ChannelEvent channelEvent)
        { }

        private static void ReadECommand(char command, char data, ChannelEvent channelEvent)
        { }

        private static void ReadFCommand(char command, char data, ChannelEvent channelEvent)
        { }

        private static void ReadSCommand(char command, char data, ChannelEvent channelEvent)
        {
            Console.WriteLine("ReadSCommand");
        }
    }

}
