using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

/*

// on startup:
var zap = new CachedSound("zap.wav");
var boom = new CachedSound("boom.wav");

// later in the app...
AudioPlaybackEngine.Instance.PlaySound(zap);
AudioPlaybackEngine.Instance.PlaySound(boom);
AudioPlaybackEngine.Instance.PlaySound("crash.wav");

// on shutdown
AudioPlaybackEngine.Instance.Dispose();

*/


namespace NAudio
{
    class AudioPlaybackEngine : IDisposable
    {
        private readonly IWavePlayer outputDevice;
        private readonly MixingSampleProvider mixer;

        public AudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2)
        {
            outputDevice = new WaveOutEvent();
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
            mixer.ReadFully = true;
            outputDevice.Init(mixer);
            outputDevice.Play();
        }

        public void PlaySound(string fileName)
        {
            var input = new AudioFileReader(fileName);
            AddMixerInput(new AutoDisposeFileReader(input));
        }

        private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
        {
            if (input.WaveFormat.Channels == mixer.WaveFormat.Channels)
            {
                return input;
            }
            if (input.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(input);
            }
            throw new NotImplementedException("Not yet implemented this channel count conversion");
        }

        public void PlaySound(CachedSound sound)
        {
            switch (sound.Type) {
                case TYPE_SOURCE.FILE_LOCAL:
                    AddMixerInput(new CachedSoundSampleProvider(sound));
                    break;
                case TYPE_SOURCE.FILE_MP3_ONLINE:
                    using (MemoryStream mp3file = new MemoryStream(sound.AudioData_FILE_MP3_ONLINE))
                    {
                        using (WaveStream blockAlignedStream = new BlockAlignReductionStream(WaveFormatConversionStream.CreatePcmStream(
                            new Mp3FileReader(mp3file))))
                        {
                            using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                            {
                                waveOut.Init(blockAlignedStream);
                                waveOut.Play();
                                while (waveOut.PlaybackState == PlaybackState.Playing)
                                {
                                    System.Threading.Thread.Sleep(100);
                                }
                            }
                        } 
                    }
                    break;
            }            
        }

        private void AddMixerInput(ISampleProvider input)
        {
            mixer.AddMixerInput(ConvertToRightChannelCount(input));
        }

        public void Dispose()
        {
            outputDevice.Dispose();
        }

        public static readonly AudioPlaybackEngine Instance = new AudioPlaybackEngine(44100, 2);
    }

    enum TYPE_SOURCE
    {
        FILE_LOCAL,
        FILE_MP3_ONLINE,
        STREAM_CLOUD,
    }

    class CachedSound
    {
        public float[] AudioData { get; private set; }
        public byte[] AudioData_FILE_MP3_ONLINE { get; private set; }
        public WaveFormat WaveFormat { get; private set; }
        public TYPE_SOURCE Type = TYPE_SOURCE.FILE_LOCAL;
        public CachedSound(string audioFileName, TYPE_SOURCE type = TYPE_SOURCE.FILE_LOCAL)
        {
            this.Type = type;
            switch (type)
            {
                case TYPE_SOURCE.FILE_LOCAL:
                    using (var audioFileReader = new AudioFileReader(audioFileName))
                    {
                        // TODO: could add resampling in here if required
                        WaveFormat = audioFileReader.WaveFormat;
                        var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
                        float[] readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
                        int samplesRead;
                        while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
                        {
                            wholeFile.AddRange(readBuffer.Take(samplesRead));
                        }
                        AudioData = wholeFile.ToArray();
                    }
                    break;
                case TYPE_SOURCE.FILE_MP3_ONLINE:
                    using (Stream ms = new MemoryStream())
                    {
                        using (Stream stream = WebRequest.Create(audioFileName)
                            .GetResponse().GetResponseStream())
                        {
                            //byte[] buffer = new byte[32768];
                            //int read;
                            //while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                            //{
                            //    ms.Write(buffer, 0, read);
                            //} 
                            // TODO: could add resampling in here if required
                            WaveFormat = new WaveFormat();
                            var wholeFile = new List<byte>();
                            byte[] readBuffer = new byte[32768];
                            int samplesRead = 0;
                            while ((samplesRead = stream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                            {
                                wholeFile.AddRange(readBuffer.Take(samplesRead));
                            }
                            AudioData_FILE_MP3_ONLINE = wholeFile.ToArray();
                        }
                    }
                    break;
                case TYPE_SOURCE.STREAM_CLOUD:
                    break;
            }

        }
    }

    class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound cachedSound;
        private long position;

        public CachedSoundSampleProvider(CachedSound cachedSound)
        {
            this.cachedSound = cachedSound;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesToCopy = 0;
            long availableSamples = 0;

            switch (this.cachedSound.Type)
            {
                case TYPE_SOURCE.FILE_LOCAL:
                    availableSamples = cachedSound.AudioData.Length - position;
                    samplesToCopy = (int)Math.Min(availableSamples, count);
                    Array.Copy(cachedSound.AudioData, position, buffer, offset, samplesToCopy);
                    position += samplesToCopy;
                    break;
                case TYPE_SOURCE.FILE_MP3_ONLINE:
                    availableSamples = cachedSound.AudioData_FILE_MP3_ONLINE.Length - position;
                    samplesToCopy = (int)Math.Min(availableSamples, count);
                    Array.Copy(cachedSound.AudioData_FILE_MP3_ONLINE, position, buffer, offset, samplesToCopy);
                    position += samplesToCopy;
                    break;
            }
            return (int)samplesToCopy;
        }

        public WaveFormat WaveFormat { get { return cachedSound.WaveFormat; } }
    }

    class AutoDisposeFileReader : ISampleProvider
    {
        private readonly AudioFileReader reader;
        private bool isDisposed;
        public AutoDisposeFileReader(AudioFileReader reader)
        {
            this.reader = reader;
            this.WaveFormat = reader.WaveFormat;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (isDisposed)
                return 0;
            int read = reader.Read(buffer, offset, count);
            if (read == 0)
            {
                reader.Dispose();
                isDisposed = true;
            }
            return read;
        }

        public WaveFormat WaveFormat { get; private set; }
    }
}
