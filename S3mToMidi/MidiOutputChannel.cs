using System.Collections.Immutable;
using System.Diagnostics;

namespace S3mToMidi
{
    internal class MidiOutputChannel : OutputChannel
    {
        private List<Event> events = new List<Event>();

        public MidiOutputChannel(int channelNumber) : base(channelNumber)
        {
            Console.WriteLine("Creating output midi channel {0}", ChannelNumber + 1);
            Debug.Assert(ChannelNumber <= 16, "output channel number out of midi range.");
        }

        public override ImmutableList<Event> GetEvents()
        {
            return [.. events];
        }

        protected override void AddEventInternal(Event @event)
        {
            events.Add(@event);
        }
    }

}