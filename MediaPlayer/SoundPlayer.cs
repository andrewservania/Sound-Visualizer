using NAudio.Wave;
using SoundVisualizer;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace MediaPlayer
{
    public class SoundPlayer
    {
        public IWavePlayer waveOutDevice;
        public IWavePlayer waveOutPlottingDevice;
        public ISampleProvider sampleProvider;

        public static float[] audioChunk;
        public static float[] completeAudioFile;
        public static GraphAudioBuffer graphAudioBuffer;

        //  public static byte[] completeMusicFile;
        public static int bufferSize = 800;

        // private const int offset = 2000;
        public static event EventHandler songLoadedEvent;

        public static NAudio.Wave.WaveChannel32 audioChannel;
        public AudioFileReader audioFileReader;

        public SoundPlayer(string path)
        {
            string tempFilePath = @"" + path;
            waveOutDevice = new WaveOut();
            waveOutPlottingDevice = new WaveOut();
            // mp3FileReader = new Mp3FileReader(tempFilePath);

            audioFileReader = new AudioFileReader(tempFilePath);

            audioChunk = new float[bufferSize];

            graphAudioBuffer = new GraphAudioBuffer();

            // completeMusicFile = new byte[mp3FileReader.Length];

            //mp3FileReader.Read(completeMusicFile, 0, Convert.ToInt32(mp3FileReader.Length));

            // mp3FileReader = new Mp3FileReader(tempFilePath);

            //   audioChannel = new WaveChannel32(mp3FileReader);

            audioFileReader = new AudioFileReader(tempFilePath);

            sampleProvider = new MySampleProvider(audioFileReader);

            completeAudioFile = new float[audioFileReader.Length];
            audioFileReader.Read(completeAudioFile, 0, (int)audioFileReader.Length);
            audioFileReader = new AudioFileReader(tempFilePath);
            waveOutDevice.Init(audioFileReader);

            songLoadedEvent(null, null);
        }

        public void Play()
        {
            waveOutDevice.Play();
        }

        public void fillGraph(long position)
        {
            //songPosition += offset;
            //if( songPosition >= completeMusicFile.Length)
            //{
            //    //Don't do nothing.
            //}
            //else
            //{
            //    Buffer.BlockCopy(completeMusicFile, songPosition, audioDataShort, 0, bufferSize);
            //    for (int x = 0; x < audioDataShort.Length; x++)
            //    {
            //        graphAudioBuffer.addSample(audioDataShort[x]);
            //    }
            //}

            for (int i = (int)position; i < (int)(position + bufferSize); i++)
            {
                graphAudioBuffer.addSample(completeAudioFile[i]);
            }

            // sampleProvider.Read(audioChunk, 0, bufferSize);

            // GraphAudioBuffer.audioData = new BindingList<float>(audioChunk.ToList<float>());
            //  MainScreen.chartReference.DataSource = GraphAudioBuffer.audioData;

            //  chart1.DataSource = GraphAudioBuffer.audioData;
            //for (int x = 0; x < audioChunk.Length; x++)
            //{
            //    graphAudioBuffer.addSample(audioChunk[x]);
            //}

            //  graphAudioBuffer.addSampleAccumulatively(0);
        }

        public void Pause()
        {
            waveOutDevice.Pause();
        }

        public void Stop()
        {
            waveOutPlottingDevice.Stop();
            waveOutDevice.Stop();
            // mp3FileReader.Seek(0, 0);
            audioFileReader.Seek(0, 0);
        }

        public static async Task PlayIntroSound()
        {
            Stream introSoundStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SoundVisualizer.Songs.IntroSound.mp3");
            Mp3FileReader introSoundReader = new Mp3FileReader(introSoundStream);
            IWavePlayer mp3Player = new WaveOut();
            GraphAudioBuffer graph = new GraphAudioBuffer();

            mp3Player.Init(introSoundReader);
            mp3Player.Play();

            mp3Player.Volume = 0.06f;
        }
    }
}