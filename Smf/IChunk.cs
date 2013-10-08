using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Smf
{
    interface IChunk
    {
        string ID { get; }
        void Write(BinaryWriter writer);
    }
}
