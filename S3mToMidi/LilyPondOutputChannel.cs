using System.Collections.Immutable;

namespace S3mToMidi
{
    internal class LilyPondOutputChannel : OutputChannel
    {
        private List<Event> events = new List<Event>();

        public LilyPondOutputChannel(int channelNumber) : base(channelNumber)
        {

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