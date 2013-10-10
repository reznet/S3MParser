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
                this.Command,
                this.Data,
                this.Row.Pattern.PatternNumber,
                this.Row.RowNumber);
        }

        

        internal static readonly byte CHANNEL_MASK = 31;     // 0x00011111;
        internal static readonly byte NOTE_INSTRUMENT_FOLLOWS_MASK = 1 << 5; // 0x00100000;
        internal static readonly byte VOLUME_FOLLOWS_MASK = 1 << 6; // 0x01000000;
        internal static readonly byte COMMAND_FOLLOWS_MASK = 1 << 7; // 0x10000000;

        internal static ChannelEvent Parse(System.IO.Stream stream, System.IO.BinaryReader reader)
        {
            ChannelEvent channelEvent = new ChannelEvent();
            byte first = reader.ReadByte();
            if (first == 0)
            {
                return null;
            }
            //Console.Out.WriteLine("First byte is {0:X}", first);
            channelEvent.ChannelNumber = 1 + first & CHANNEL_MASK;
            Debug.Assert(channelEvent.ChannelNumber <= 6, "channel number should be less than 6 but is " + channelEvent.ChannelNumber);
           // Console.Out.WriteLine("Channel is {0}", channelEvent.ChannelNumber);
            if ((first & NOTE_INSTRUMENT_FOLLOWS_MASK) == NOTE_INSTRUMENT_FOLLOWS_MASK)
            {
                // read note and instrument
                channelEvent.Note = reader.ReadByte();
                channelEvent.Instrument = reader.ReadByte();
                //Debug.Assert(channelEvent.Instrument < 16, "instrument should be less than 16");
                //Console.Out.WriteLine("Note: {0:X2}", channelEvent.Note);
                //Console.Out.WriteLine("Instrument: {0:X2}", channelEvent.Instrument);
            }
            if ((first & VOLUME_FOLLOWS_MASK) == VOLUME_FOLLOWS_MASK)
            {
                channelEvent.Volume = reader.ReadByte();
                //Console.Out.WriteLine("Volume: {0:D2}", channelEvent.Volume);
            }
            if ((first & COMMAND_FOLLOWS_MASK) == COMMAND_FOLLOWS_MASK)
            {
                channelEvent.Command = reader.ReadByte().ToCommandType();
                channelEvent.Data = reader.ReadByte();
            }

            return channelEvent;
        }
    }
}
