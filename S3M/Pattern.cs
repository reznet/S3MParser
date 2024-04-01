using System.Diagnostics;

namespace S3M
{
    [DebuggerDisplay("PatternNumber={PatternNumber}")]
    public class Pattern
    {
        public int PatternNumber
        {
            get;
            internal set;
        }

        public List<Row> Rows = new List<Row>();

        public IEnumerable<ChannelEvent> EventsByChannel
        {
            get
            {
                var f = from row in this.Rows
                        from evt in row.ChannelEvents
                        select evt;
                return f;
            }
        }

        public IEnumerable<Channel> Channels
        {
            get
            {
                for (var channel = 1; channel <= 32; channel++)
                {
                    yield return new Channel(channel, from a in EventsByChannel where a.ChannelNumber == channel select a);
                }
            }
        }

        public IEnumerable<Cell> GetCells(int channelNumber)
        {
            int row = 0;
            foreach (var ce in from ebc in this.EventsByChannel where ebc.ChannelNumber == channelNumber select ebc)
            {
                while (row < ce.Row.RowNumber)
                {
                    yield return new Cell(row, channelNumber, null);
                    row++;
                }
                if (row == ce.Row.RowNumber)
                {
                    yield return new Cell(row, channelNumber, ce);
                    row++;
                }
            }
            while (row <= this.Rows.Count)
            {
                yield return new Cell(row, channelNumber, null);
                row++;
            }
        }


        public static IEnumerable<Cell> ConvertToCells(IEnumerable<ChannelEvent> channelEvents, int rowCount, int channelNumber)
        {
            int row = 1;
            foreach (var ce in channelEvents)
            {
                while (row < ce.Row.RowNumber)
                {
                    yield return new Cell(row, channelNumber, null);
                    row++;
                }
                if (row == ce.Row.RowNumber)
                {
                    yield return new Cell(row, channelNumber, ce);
                    row++;
                }
            }
            while (row <= rowCount)
            {
                yield return new Cell(row, channelNumber, null);
                row++;
            }
        }

        internal static Pattern Read(Stream stream, BinaryReader reader)
        {
            Pattern pattern = new Pattern();

            // ignore the pattern length bytes because some trackers write the wrong value, so it cannot be trusted
            // instead, read until either EOF or the 64th row is read
            reader.ReadBytesAsInt(2);

            for (int i = 0; i < 64; i++)
            {
                Row row = Row.Parse(reader);
                row.RowNumber = i;
                row.Pattern = pattern;
                pattern.Rows.Add(row);
            }

            return pattern;
        }
    }
}
