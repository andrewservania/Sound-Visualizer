using System.Collections.Generic;
using System.ComponentModel;

namespace MediaPlayer
{
    public class GraphAudioBuffer
    {
        public static BindingList<float> audioData;
        public static Dictionary<int, float> audioGraphWindow;
        private int listCapacity = SoundPlayer.bufferSize / 2;

        private int sampleCounter = 0;

        public GraphAudioBuffer()
        {
            audioData = new BindingList<float>();
            audioGraphWindow = new Dictionary<int, float>();
        }

        public void addSample(float sample)
        {
            if (sampleCounter > listCapacity - 1)
            {
                //1. add the sample to the list but remove number 0;

                audioGraphWindow.Add(sampleCounter, sample);
                audioData.Add(sample);
                audioData.RemoveAt(0);
            }
            else
            {
                audioGraphWindow.Add(sampleCounter, sample);
                audioData.Add(sample);
            }

            sampleCounter++;
        }

        public void addSampleAccumulatively(float sample)
        {
            audioData.Add(sample);
        }

        public BindingList<float> getAudioData()
        {
            return audioData;
        }

        public Dictionary<int, float> getAudioData2()
        {
            return audioGraphWindow;
        }
    }
}