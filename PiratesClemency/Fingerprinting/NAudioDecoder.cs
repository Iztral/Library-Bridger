using AcoustID.Audio;
using AcoustID.Chromaprint;
using NAudio.Wave;
using System;
using System.IO;

namespace PiratesClemency.Fingerprinting
{
    class NAudioDecoder : IDecoder
    {
        private const int BUFFER_SIZE = 2 * 192000;

        private readonly string file;

        private readonly int bitsPerSample;

        public int SampleRate { get; private set; }

        public int Channels { get; private set; }

        public NAudioDecoder(string file)
        {
            this.file = file;

            using (var reader = OpenWaveStream(file))
            {
                var format = reader.WaveFormat;

                bitsPerSample = format.BitsPerSample;

                SampleRate = format.SampleRate;
                Channels = format.Channels;
            }
        }

        public bool Decode(IAudioConsumer consumer, int maxLength)
        {
            if (bitsPerSample != 16) return false;

            using (var reader = OpenWaveStream(file))
            {
                int remaining, length, size;

                // TODO: get buffers from memory pool.
                byte[] buffer = new byte[2 * BUFFER_SIZE];
                short[] data = new short[BUFFER_SIZE];

                // Samples to read to get maxLength seconds of audio
                remaining = maxLength * Channels * SampleRate;

                // Bytes to read
                length = 2 * Math.Min(remaining, BUFFER_SIZE);

                while ((size = reader.Read(buffer, 0, length)) > 0)
                {
                    Buffer.BlockCopy(buffer, 0, data, 0, size);

                    consumer.Consume(data, size / 2);

                    remaining -= size / 2;
                    if (remaining <= 0)
                    {
                        break;
                    }

                    length = 2 * Math.Min(remaining, BUFFER_SIZE);
                }

                return true;
            }
        }

        private WaveStream OpenWaveStream(string file)
        {
            var extension = Path.GetExtension(file).ToLowerInvariant();

            if (extension.Equals(".mp3"))
            {
                return new Mp3FileReader(file);
            }

            // Try open as WAV (will throw an exception, if not supported).
            return new WaveFileReader(file);
        }
    }
}
