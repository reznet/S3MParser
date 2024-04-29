using System.Collections.Immutable;
using CommandLine;

namespace S3mToMidi
{
    internal class LilyPondOutputChannel : OutputChannel
    {
        private List<Event> events= new List<Event>();

        public LilyPondOutputChannel() : base(1)
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