using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Smf
{
    public class Track : IChunk
    {
        public IList<TrackEvent> Events = new List<TrackEvent>();
        #region IChunk Members

        public string ID
        {
            get { return "MTrk"; }
        }

        public void Write(System.IO.BinaryWriter writer)
        {
            writer.Write((Encoding.ASCII.GetBytes(this.ID)));

            MemoryStream memoryStream = new MemoryStream();
            foreach (TrackEvent evt in this.Events)
            {
                evt.Write(memoryStream);
            }
            writer.Write((int)memoryStream.Length);
            writer.Write(memoryStream.ToArray());
        }

        #endregion

        public void AddEvent(TrackEvent trackEvent)
        {
            this.Events.Add(trackEvent);
        }
    }
}
