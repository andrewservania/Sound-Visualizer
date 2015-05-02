using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer
{
    public class GraphAudioBuffer
    {

        public static BindingList<short> audioData;
        public static  Dictionary<int, short> audioGraphWindow;
        private  int listCapacity = SoundPlayer.bufferSize/2;
        private  int sampleCounter = 0;
        public GraphAudioBuffer()
        {
            audioData = new BindingList<short>();
            audioGraphWindow = new Dictionary<int, short>();

        }

        public void addSample(short sample ){

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

        public BindingList<short> getAudioData()
        {
            return audioData;
        }
        public Dictionary<int, short> getAudioData2()
        {
            return audioGraphWindow;
        }
    }
}
