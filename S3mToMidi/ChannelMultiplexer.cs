using System.Diagnostics;
using System.Diagnostics.Metrics;
using S3M;

namespace S3mToMidi
{
    internal class ChannelMultiplexer
    {
        private readonly Dictionary<int, OutputChannel> _outputChannels = new Dictionary<int, OutputChannel>();

        private readonly Func<int> GetNextAvailableMidiChannel;

        public int InputChannelNumber { get; }

        public int DefaultVolume { get; }

        public bool IsMuted { get; }

        public ChannelMultiplexer(int inputChannelNumber, int defaultVolume, bool isMuted, Func<int> getNextAvailableMidiChannel)
        {
            InputChannelNumber = inputChannelNumber;
            DefaultVolume = defaultVolume;
            IsMuted = isMuted;
            GetNextAvailableMidiChannel = getNextAvailableMidiChannel;
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
                    value = CreateOutputChannel();
                    _outputChannels.Add(channelKey, value);
                }
                return value;
            }
        }

        private OutputChannel CreateOutputChannel()
        {
            if (IsMuted)
            {
                return new MuteOutputChannel();
            }
            else
            {
                int channelNumber = GetNextAvailableMidiChannel();
                if (channelNumber == 09) // 10 in zero-based counter is 09
                {
                    // avoid General Midi channel 10 because players interpret it as a drum channel
                    Console.WriteLine("Skipping output drum channel 10");
                    channelNumber = GetNextAvailableMidiChannel();
                }

                return new MidiOutputChannel(channelNumber);
            }
        }
    }
}