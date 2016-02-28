using NAudio.Wave;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using SoundVisualizer;

namespace MediaPlayer
{
    public partial class MainScreen : Form
    {
        public SoundPlayer soundPlayer;
        public ThreadStart songPositionthreadStart;
        public Thread readSongPositionThread;
        public ThreadStart currentTimeThreadStart;
        public Thread readCurrentTimeThread;
        public const string chartingAreaName = "Draw";
        public static string songFileName = string.Empty;
        private string lastSongName = string.Empty;
        public static long start = 0;
        public static long endTime;
        public static long seekStepValue;
        public static int seekPrecision;
        private static int zoomCount;
        private NAudio.Wave.WaveChannel32 waveChannel;

        ChartManager chartManager;
        DragAndDropHandler dragAndDropHandler;
        GUILabelManager guiLabelManager;

        public MainScreen()
        {
            InitializeComponent();
         
            chartManager = new ChartManager(chart1,Controls);



            dragAndDropHandler = new DragAndDropHandler(songLoadingProgressBar, songLoadingLabel,songName);
            guiLabelManager = new GUILabelManager(songName, songLoadingLabel, songTimeLabel, volumePercentageLabel);
            SetVolumeLabelSlider();

            SoundPlayer.songLoadedEvent += new EventHandler(songLoadedEvent);

            // permissions
            this.AllowDrop = true;
            // Events callback methods
            this.DragEnter += new DragEventHandler(dragAndDropHandler.DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);

            SoundPlayer.PlayIntroSound();

            //  waveViewer1.MouseWheel += new MouseEventHandler(mouseScroll_OnWaveViewer);
        }

        private void SetVolumeLabelSlider()
        {
            seekPrecision = seekBar.Maximum;
            double percent = 1.0 - (volumeBar.Value / 91.0);
            volumePercentageLabel.Text = Math.Round(percent * 100.0) + "%";
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string songFilename in files)
            {
                if (songFilename.Contains(".mp3") == false)
                {
                    MessageBox.Show("'" + songFilename.Substring(songFilename.Length - 4, 4) + "' is not supported at the moment.");
                }
                else
                {
                    reset();
                    dragAndDropHandler.DragDrop(sender, e);
                    Thread songLoadingThread = new Thread(new ParameterizedThreadStart(loadSong));
                    songLoadingThread.Start(dragAndDropHandler.GetSongFileName());
                    songName.Text = dragAndDropHandler.GetSafeFilename(); ;
                }
            }
        }


        private void songLoadedEvent(object sender, EventArgs e)
        {
            MethodInvoker invoker = delegate
            {
                songLoadingProgressBar.Visible = false;
                songLoadingLabel.Visible = false;
                chartManager.GetChart().DataSource = GraphAudioBuffer.audioData;
                chartManager.GetChart().DataBind();
            };
            this.Invoke(invoker);
            //PlayButton_Click(null, null)
        }


        private void PlayButton_Click(object sender, EventArgs e)
        {
            if (soundPlayer == null)
            {
                // songName.Text = "Please Select a song first. :) ";
            }
            else
            {
                if (soundPlayer.waveOutDevice.PlaybackState == PlaybackState.Playing)
                {
                    // Don't do anything
                }
                else
                {
                    //songPositionthreadStart = new ThreadStart(ReadSongPositionAsync);
                    //readSongPositionThread = new Thread(songPositionthreadStart);

                    currentTimeThreadStart = new ThreadStart(UpdateCurrentSongTime);
                    readCurrentTimeThread = new Thread(currentTimeThreadStart);

                    //readSongPositionThread.Start();
                    readCurrentTimeThread.Start();
                }
                soundPlayer.Play();
                ReadSongPositionAsync();
            }
        }

        public async Task ReadSongPositionAsync()
        {
            
            Task.Run(() =>
            {
                while (soundPlayer.waveOutDevice.PlaybackState == PlaybackState.Playing)
                {
                    MethodInvoker inv1 = delegate
                    {
                        soundPlayer.fillGraph(soundPlayer.audioFileReader.Position);
                        chartManager.GetChart().DataBind();
                        chartManager.GetChart().Update();
                        //
                    };

                    this.Invoke(inv1);
                }
                Thread.Sleep(100);
            });
        }

        public void UpdateCurrentSongTime()
        {
            while (soundPlayer.audioFileReader.CurrentTime < soundPlayer.audioFileReader.TotalTime)
            {
                MethodInvoker inv = delegate
                {
                    this.songTimeLabel.Text = soundPlayer.audioFileReader.CurrentTime.ToString().Remove(
                        soundPlayer.audioFileReader.CurrentTime.ToString().Length - 5, 5);
                    seekBar.Value = (int)(soundPlayer.audioFileReader.Position / seekStepValue);
                };
                this.Invoke(inv);
                Thread.Sleep(100);
            }
        }

        private void PauzeButton_Click(object sender, EventArgs e)
        {
            if (readCurrentTimeThread != null)
                if (readCurrentTimeThread.IsAlive)
                    readCurrentTimeThread.Abort();
                
       
            if (readSongPositionThread != null)
                if (readSongPositionThread.IsAlive)
                    readSongPositionThread.Abort();
                
            
            if (soundPlayer != null)
                soundPlayer.Pause();
            

            ReadSongPositionAsync();
            // songPositionthreadStart = new ThreadStart(ReadSongPositionAsync);
            // readSongPositionThread = new Thread(songPositionthreadStart);

            currentTimeThreadStart = new ThreadStart(UpdateCurrentSongTime);
            readCurrentTimeThread = new Thread(currentTimeThreadStart);
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void songTime(object sender, EventArgs e)
        {
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "MP3 Audio Files (.mp3)|*.mp3|All Files(*.*)|*.*";
            openFileDialog.FilterIndex = 1;

            openFileDialog.Multiselect = false;

            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                reset();

                songLoadingProgressBar.Visible = true;
                songLoadingProgressBar.Style = ProgressBarStyle.Marquee;
                songLoadingLabel.Visible = true;
                songFileName = openFileDialog.FileName;
                Thread songLoadingThread = new Thread(new ParameterizedThreadStart(loadSong));
                songLoadingThread.Start(songFileName);

                songName.Text = openFileDialog.SafeFileName;
            }
        }

        private void reset()
        {
            if (soundPlayer != null)
            {
                soundPlayer.Stop();
                readSongPositionThread.Abort();
                readCurrentTimeThread.Abort();
            }
        }

        public void loadSong(object fileName)
        {
            soundPlayer = new SoundPlayer((fileName).ToString());
            double percent = 1.0 - (volumeBar.Value / 91.0);

            soundPlayer.waveOutDevice.Volume = (float)percent;
            endTime = soundPlayer.audioFileReader.Length;
            seekStepValue = endTime / seekPrecision;
            PlayButton_Click(null, null);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Sound Visualizer\nVersion 0.1.2 Beta :)\nCopyright (c) 2015 Andrew Servania");
        }

  

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            reset();
            Application.Exit();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            songTimeLabel.Text = "00:00:00.00";

            if (soundPlayer != null) soundPlayer.Stop();
            if (readSongPositionThread != null)   readSongPositionThread.Abort();
            if (readCurrentTimeThread != null)  readCurrentTimeThread.Abort();

            ReadSongPositionAsync();
            // songPositionthreadStart = new ThreadStart(ReadSongPositionAsync);
            // readSongPositionThread = new Thread(songPositionthreadStart);

            currentTimeThreadStart = new ThreadStart(UpdateCurrentSongTime);
            readCurrentTimeThread = new Thread(currentTimeThreadStart);

            songTimeLabel.Text = "00:00:00.00";
        }

        private void volumeBar_Scroll(object sender, ScrollEventArgs e)
        {
            double percent = 1.0 - (e.NewValue / 91.0);
            volumePercentageLabel.Text = Math.Round(percent * 100.0) + "%";
            if (soundPlayer != null)
                soundPlayer.waveOutDevice.Volume = (float)(percent);
        }

        private void TwoD_radiobutton_CheckedChanged(object sender, EventArgs e)
        {
            chartManager.GetChart().ChartAreas.First().Area3DStyle.Enable3D = false;
        }

        private void ThreeD_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            chartManager.GetChart().ChartAreas.First().Area3DStyle.Enable3D = true;
        }

        private void seekBar_Scroll(object sender, EventArgs e)
        {
            if (soundPlayer != null)
            {
                soundPlayer.audioFileReader.Seek(((TrackBar)sender).Value * seekStepValue, SeekOrigin.Begin);
            }
        }
    }
}