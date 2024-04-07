using System.Diagnostics;
using System.Diagnostics.Metrics;
using S3M;

namespace S3mToMidi
{
    internal class ChannelMultiplexer
    {
        private readonly Dictionary<int, OutputChannel> _outputChannels = new Dictionary<int, OutputChannel>();
        public int InputChannelNumber { get; }

        public int DefaultVolume { get; }

        public ChannelMultiplexer(int inputChannelNumber, int defaultVolume)
        {
            InputChannelNumber = inputChannelNumber;
            DefaultVolume = defaultVolume;
        }

        public void AddEvent(Event @event)
        {
            this[0].AddEvent(@event);
        }

        public void AddEvent(NoteEvent noteEvent)
        {
            this[noteEvent.Instrument].AddEvent(noteEvent);
        }

        public void NoteOn(int tick, int instrument, int note, int? volume)
        {
            var outputChannel = this[instrument];

            outputChannel.AddEvent(new NoteEvent(tick, NoteEvent.EventType.NoteOn, outputChannel.ChannelNumber, instrument, note, volume ?? DefaultVolume));
        }

        public void NoteOff(int tick)
        {
            var channelPlayingNote = _outputChannels.Values.FirstOrDefault(c => c.IsPlayingNote);
            Debug.Assert(channelPlayingNote != null, "Could not find channel playing note to generate note off event.");

            var noteOff = channelPlayingNote.CurrentNote.Clone(tick);
            noteOff.Type = NoteEvent.EventType.NoteOff;

            channelPlayingNote.AddEvent(noteOff);
        }

        public List<Event> Events
        {
            get 
            {
                return _outputChannels.Values.SelectMany(c => c.NoteEvents).ToList();
            }
        }

        public bool IsPlayingNote
        {
            get
            {
                return _outputChannels.Values.Any(c => c.IsPlayingNote);
            }
        }

        private OutputChannel this[int instrument]
        {
            get
            {
                if (!_outputChannels.TryGetValue(instrument, out OutputChannel? value))
                {
                    value = new OutputChannel(_outputChannels.Count);
                    _outputChannels.Add(value.ChannelNumber, value);
                }
                return value;
            }
        }
    }
}