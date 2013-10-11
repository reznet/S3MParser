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
                byte command = reader.ReadByte();
                channelEvent.Command = command.ToCommandType();
                channelEvent.Data = reader.ReadByte();

                Console.WriteLine("Got command byte {0} which is command type {1} and data is {2}", command, channelEvent.Command, channelEvent.Data);
            }

            return channelEvent;
        }
    }
}
