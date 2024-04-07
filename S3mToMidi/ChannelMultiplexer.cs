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
            this[1].AddEvent(@event);
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

        public IList<OutputChannel> OutputChannels
        {
            get 
            {
                return [.. _outputChannels.Values];
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
                // S3M supports max 100 instruments
                // channel and instrument numbers start at 1
                var channelKey = (InputChannelNumber - 1) << 8 | instrument;
                if (!_outputChannels.TryGetValue(channelKey, out OutputChannel? value))
                {
                    value = new OutputChannel();
                    _outputChannels.Add(channelKey, value);
                }
                return value;
            }
        }
    }
}