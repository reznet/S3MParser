using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Smf
{
    public class TrackEvent
    {
        public int Command;
        public int Channel;
        
        internal void Write(System.IO.MemoryStream memoryStream)
        {
            throw new NotImplementedException();
        }
    }
}
