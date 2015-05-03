using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer
{
    public class SoundPlayer
    {
        public IWavePlayer waveOutDevice1;

        public Mp3FileReader mp3FileReader1;
        public Mp3FileReader mp3FileReader2;
        public static byte[] audioData;
        public static short[] audioDataShort;
        public static GraphAudioBuffer graphAudioBuffer;
        public static byte[] completeMusicFile;
        public static int bufferSize = 800; 
        public static event EventHandler songLoadedEvent;
        

        public SoundPlayer(string path)
        {
            string tempFilePath = @""+path;
            waveOutDevice1 = new WaveOut();


            mp3FileReader1 = new Mp3FileReader(tempFilePath);
            waveOutDevice1.Init(mp3FileReader1);
            mp3FileReader2 = new Mp3FileReader(tempFilePath);
            


            

            audioData = new byte[bufferSize];
            audioDataShort = new short[bufferSize/2];
            graphAudioBuffer = new GraphAudioBuffer();
            songLoadedEvent(null, null);
        }      

        public void Play(){
            waveOutDevice1.Play();
        }

        public async Task updateGraph(){


            await mp3FileReader2.ReadAsync(audioData, 0, audioData.Length);
             Buffer.BlockCopy(audioData, 0, audioDataShort, 0, audioData.Length);
             for (int x = 0; x < audioDataShort.Length; x++)
             {
                 graphAudioBuffer.addSample(audioDataShort[x]);
             }
            
        }

        public void Pause(){
           waveOutDevice1.Pause();

        }

        public void Stop()
        {
            waveOutDevice1.Stop();

            mp3FileReader1.Seek(0, 0);

        }
    }
}
