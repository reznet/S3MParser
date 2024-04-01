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
        public Row? Row;
        public Pattern Pattern => Row.Pattern;

        public NoteAction NoteAction => Note switch
        {
            -1 or 0xFF => NoteAction.None,
            0xFE => NoteAction.Stop,
            _ => NoteAction.Start,
        };

        public bool HasVolume { get; private set; }

        public bool HasInstrument => Instrument > 0;
        public override string ToString()
        {
            return String.Format("Pattern:{6:D2} Row:{7:D2} Channel:{0:D2} Note:{1} Instrument:{2} Volume:{3} Command:{4} Data:{5}",
                ChannelNumber,
                Note,
                Instrument,
                Volume,
                Command.ToString(),
                Data,
                Row.Pattern.PatternNumber,
                Row.RowNumber);
        }

        private const byte CHANNEL_MASK = 31;                       // 0x00011111;
        private const byte NOTE_INSTRUMENT_FOLLOWS_MASK = 1 << 5;   // 0x00100000;
        private const byte VOLUME_FOLLOWS_MASK = 1 << 6;            // 0x01000000;
        private const byte COMMAND_FOLLOWS_MASK = 1 << 7;           // 0x10000000;

        internal static ChannelEvent? Parse(BinaryReader reader)
        {
            ChannelEvent channelEvent = new();
            byte first = reader.ReadByte();
            if (first == 0)
            {
                return null;
            }
            channelEvent.ChannelNumber = (1 + first) & CHANNEL_MASK;
            if ((first & NOTE_INSTRUMENT_FOLLOWS_MASK) == NOTE_INSTRUMENT_FOLLOWS_MASK)
            {
                // read note and instrument
                channelEvent.Note = reader.ReadByte();
                channelEvent.Instrument = reader.ReadByte();
            }
            if ((first & VOLUME_FOLLOWS_MASK) == VOLUME_FOLLOWS_MASK)
            {
                channelEvent.HasVolume = true;
                channelEvent.Volume = reader.ReadByte();
            }
            if ((first & COMMAND_FOLLOWS_MASK) == COMMAND_FOLLOWS_MASK)
            {
                // given the effect ABC
                // command byte is the decimal value of the first hex character in the command columns
                // e.g. A => 10
                byte commandByte = reader.ReadByte();

                // databyte is the decimal value of the last two hex characters in the command columns
                // e.g. BC => 118
                byte dataByte = reader.ReadByte();

                // hi and low are the decimal values of the last two hex characters
                // e.g. B => 11, C = 12
                // low comes before first
                // e.g. B = low, C = high
                // TODO are these backwards?
                _ = (dataByte & 0xF0) >> 4;
                _ = dataByte & 0xF;

                // commandChar is the first hex character of the command columns
                // e.g. A in the command ABC
                // 0 is @, 1 is A, 2 is B, ...
                _ = (char)('A' - 1 + commandByte);

                // single argument "simple" commands (xx)
                // use dataByte as the argument to the command
                // A, B, C, G, T, U, V
                //
                // double argument simple commands (xy)
                // H, I, J, K, L, O, Q, R, U
                //
                // two part command names:
                // look at hi and low to determine which command it is
                // D0y, Dx0, DFy, DxF
                // EFx, EEx,
                // Fxx, FFx, FEx
                // S0, S1, S2, S3, S4, S8
                // SA, SB, SC, SD, SE, SF

                // default to channel event data equaling the command data
                // commands with two part names will update this
                channelEvent.Data = dataByte;

                var commandAndInfo = CommandAndInfo.Create(commandByte, dataByte);
                channelEvent.Command = commandAndInfo.Commmand;
                channelEvent.Data = commandAndInfo.X;
            }

            return channelEvent;
        }
    }

}
