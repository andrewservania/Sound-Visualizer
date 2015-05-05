using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer
{
    public class SoundPlayer
    {
        public IWavePlayer waveOutDevice;        
        public Mp3FileReader playTime;
        public Mp3FileReader mp3FileReader;
        public static byte[] audioData;
        public static short[] audioDataShort;
        public static GraphAudioBuffer graphAudioBuffer;
        public static byte[] completeMusicFile;
        public static int bufferSize = 300; 
        private const int offset = 2000;
        public static event EventHandler songLoadedEvent;

        public SoundPlayer(string path)
        {
            string tempFilePath = @""+path;
            waveOutDevice = new WaveOut();
            mp3FileReader = new Mp3FileReader(tempFilePath);
            completeMusicFile = new byte[mp3FileReader.Length];

            mp3FileReader.Read(completeMusicFile, 0, Convert.ToInt32(mp3FileReader.Length));
            
            mp3FileReader = new Mp3FileReader(tempFilePath);
            waveOutDevice.Init(mp3FileReader);

            

            audioData = new byte[bufferSize];
            audioDataShort = new short[bufferSize/2];
            graphAudioBuffer = new GraphAudioBuffer();
            songLoadedEvent(null, null);
        }      

        public void Play(){
            waveOutDevice.Play();
           
        }

        public void updateGraph(int songPosition){
            songPosition += offset;
            if( songPosition >= completeMusicFile.Length)
            {
                //Don't do nothing.
            }
            else
            {
                Buffer.BlockCopy(completeMusicFile, songPosition, audioDataShort, 0, bufferSize);
                for (int x = 0; x < audioDataShort.Length; x++)
                {
                    graphAudioBuffer.addSample(audioDataShort[x]);
                }
            }
        }

        public void Pause(){
           waveOutDevice.Pause();
        }

        public void Stop()
        {
            waveOutDevice.Stop();
            mp3FileReader.Seek(0, 0);
        }


        public static async Task PlayIntroSound()
        {
            Stream introSoundStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MediaPlayer.Songs.IntroSound.mp3");
            Mp3FileReader introSoundReader = new Mp3FileReader(introSoundStream);
            IWavePlayer mp3Player = new WaveOut();
            GraphAudioBuffer graph = new GraphAudioBuffer();
            
            mp3Player.Init(introSoundReader);
            mp3Player.Play();
            mp3Player.Volume = 0.06f;
        }
    
    
    }
}
