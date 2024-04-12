using System.Collections.Immutable;

namespace S3mToMidi
{
    internal class MuteOutputChannel : OutputChannel
    {
        const int MuteMidiChannel = 0;
        public MuteOutputChannel() : base(MuteMidiChannel){}

        public override ImmutableList<Event> GetEvents()
        {
            return ImmutableList<Event>.Empty;
        }

        protected override void AddEventInternal(Event @event)
        {
            // no-op
        }
    }
}