using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SoundVisualizer
{
    public class DragAndDropHandler
    {
        private ProgressBar songLoadingProgressBar;
        private Label songLoadingLabel,songName;
        private string lastSongName = string.Empty;
        private string songFilename = string.Empty;
        private string safeFileName = string.Empty;

        public DragAndDropHandler(ProgressBar _progressBar, Label _songLoadingLabel, Label _songName)
        {
            songLoadingProgressBar = _progressBar;
            songLoadingLabel = _songLoadingLabel;
            songName = _songName;

        }

        public void DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        public void DragDrop(object sender, DragEventArgs e)
        {

                    lastSongName = songFilename;
                    songLoadingProgressBar.Visible = true;
                    songLoadingProgressBar.Style = ProgressBarStyle.Marquee;
                    songLoadingLabel.Visible = true;

                    // songFilename.Trim();
                    for (int i = songFilename.Length - 1; i > 0; i--)
                    {
                        if (songFilename[i] == '\\')
                        {
                            safeFileName = songFilename.Substring(i + 1);

                            break;
                        }
                    }

        }


        public string GetSongFileName()
        {
            return songFilename;
        }

        public string GetSafeFilename()
        {
            return safeFileName;
        }
    }
}
