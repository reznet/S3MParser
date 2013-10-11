using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3MParser
{
    class TimeSignatureEvent : Event
    {
        private readonly int _beatsPerBar;
        private readonly int _beatValue;

        public TimeSignatureEvent(int tick, int beatsPerBar, int beatValue)
            : base(tick)
        {
            _beatsPerBar = beatsPerBar;
            _beatValue = beatValue;
        }

        public int BeatsPerBar { get { return _beatsPerBar; } }

        public int BeatValue { get { return _beatValue; } }
    }
}
