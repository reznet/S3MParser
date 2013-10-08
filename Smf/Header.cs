using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Smf
{
    class Header : IChunk
    {
        public enum TrackFormat
        {
            Single,
            Multiple,
            MultipleSong
        }

        public TrackFormat Format = TrackFormat.Single;

        public Int16 TrackCount;
        public Int16 Timing;

        #region IChunk Members

        public string ID
        {
            get { return "MThd"; }
        }

        public void Write(BinaryWriter writer)
        {
            // header_chunk = "MThd" + <header_length> + <format> + <n> + <division>
            byte[] bytes = Encoding.ASCII.GetBytes(this.ID);
            writer.Write(bytes);
            writer.Write(6);
            writer.Write((int)this.Format);
            writer.Write(this.TrackCount);
            writer.Write(this.Timing);
        }

        #endregion
    }
}
