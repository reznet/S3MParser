using System.Diagnostics;

namespace S3M
{
    [DebuggerDisplay("PatternNumber={PatternNumber}")]
    public class Pattern
    {
        public Pattern()
        {
            Rows = Enumerable.Range(0, S3MFile.ROW_COUNT).Select(i => new Row(){ RowNumber = i, Pattern = this }).ToArray();
        }
        public int PatternNumber
        {
            get;
            set;
        }

        public Row[] Rows = new Row[S3MFile.ROW_COUNT];

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

        internal static Pattern Read(Stream stream, BinaryReader reader)
        {
            Pattern pattern = new Pattern();

            // ignore the pattern length bytes because some trackers write the wrong value, so it cannot be trusted
            // instead, read until either EOF or the 64th row is read
            reader.ReadBytesAsInt(2);

            for (int i = 0; i < S3MFile.ROW_COUNT; i++)
            {
                Row row = Row.Parse(reader);
                row.RowNumber = i;
                row.Pattern = pattern;
                pattern.Rows[i] = row;
            }

            return pattern;
        }
    }
}
