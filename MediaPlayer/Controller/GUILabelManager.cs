using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SoundVisualizer
{
    public class GUILabelManager
    {
        Label songName;
        Label songLoading;
        Label songTime;
        Label volumePercentage;

        public GUILabelManager(Label _songName,Label _songLoading, Label _songTime, Label _volumePercentage)
        {   
            songName = _songTime;
            songLoading = _songLoading;
            songTime = _songTime;
            volumePercentage = _volumePercentage;
        }
    }
}
