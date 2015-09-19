using NAudio.Wave;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

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

        public static Chart chartReference;

        public MainScreen()
        {
            InitializeComponent();
            seekPrecision = seekBar.Maximum;
            double percent = 1.0 - (volumeBar.Value / 91.0);
            volumePercentageLabel.Text = Math.Round(percent * 100.0) + "%";
            InitializeChart();
            SoundPlayer.songLoadedEvent += new EventHandler(songLoadedEvent);
            chartReference = chart1;
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);
            SoundPlayer.PlayIntroSound();

            //  waveViewer1.MouseWheel += new MouseEventHandler(mouseScroll_OnWaveViewer);
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

                    lastSongName = songFilename;

                    songLoadingProgressBar.Visible = true;
                    songLoadingProgressBar.Style = ProgressBarStyle.Marquee;
                    songLoadingLabel.Visible = true;
                    Thread songLoadingThread = new Thread(new ParameterizedThreadStart(loadSong));
                    songLoadingThread.Start(songFilename);

                    string safeFileName = string.Empty;
                    // songFilename.Trim();
                    for (int i = songFilename.Length - 1; i > 0; i--)
                    {
                        if (songFilename[i] == '\\')
                        {
                            safeFileName = songFilename.Substring(i + 1);

                            break;
                        }
                    }

                    songName.Text = safeFileName;
                }
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void songLoadedEvent(object sender, EventArgs e)
        {
            MethodInvoker invoker = delegate
            {
                songLoadingProgressBar.Visible = false;
                songLoadingLabel.Visible = false;
                chart1.DataSource = GraphAudioBuffer.audioData;
                chart1.DataBind();
            };
            this.Invoke(invoker);
            //PlayButton_Click(null, null
        }

        public void InitializeChart()
        {
            chart1.Series.First().ChartType = SeriesChartType.FastLine;

            chart1.Series.First().XValueMember = "X";
            chart1.Series.First().YValueMembers = "Y";

            chart1.DataSource = GraphAudioBuffer.audioData;
            chart1.DataBind();

            //chart1.ChartAreas.First().AxisY.Minimum = -32000.0;
            //chart1.ChartAreas.First().AxisY.Maximum = 32000.0;

            chart1.ChartAreas.First().AxisY.Minimum = -1.0f;
            chart1.ChartAreas.First().AxisY.Maximum = 1.0f;

            // Info: https://www.daniweb.com/software-development/csharp/code/451281/simple-line-graph-charting

            Controls.Add(chart1);// is this necessary?
        }

        //public void InitializeWaveVeawer(string filepath)
        //{
        //    if(filepath.Contains(".wav")){
        //        Stream introSoundStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(filepath);
        //        NAudio.Wave.WaveFileReader wavReader = new NAudio.Wave.WaveFileReader(introSoundStream);
        //        waveViewer1.WaveStream = wavReader;

        //    }
        //    else if (filepath.Contains(".mp3"))
        //    {
        //        NAudio.Wave.audioFileReader mp3reader = new NAudio.Wave.audioFileReader(filepath);
        //        waveViewer1.WaveStream = mp3reader;
        //        waveChannel = new WaveChannel32(mp3reader);
        //        waveChannel.
        //    }

        //    zoomCount = waveViewer1.SamplesPerPixel;

        //}

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
                        chart1.DataBind();
                        chart1.Update();
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
            {
                if (readCurrentTimeThread.IsAlive)
                {
                    readCurrentTimeThread.Abort();
                }
            }

            if (readSongPositionThread != null)
            {
                if (readSongPositionThread.IsAlive)
                {
                    readSongPositionThread.Abort();
                }
            }
            if (soundPlayer != null)
            {
                soundPlayer.Pause();
            }

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

        public void clear()
        {
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            reset();
            Application.Exit();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            songTimeLabel.Text = "00:00:00.00";

            if (soundPlayer != null)
            {
                soundPlayer.Stop();
            }

            if (readSongPositionThread != null)
            {
                readSongPositionThread.Abort();
            }

            if (readCurrentTimeThread != null)
            {
                readCurrentTimeThread.Abort();
            }

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
            {
                soundPlayer.waveOutDevice.Volume = (float)(percent);
            }
        }

        private void TwoD_radiobutton_CheckedChanged(object sender, EventArgs e)
        {
            chart1.ChartAreas.First().Area3DStyle.Enable3D = false;
        }

        private void ThreeD_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            chart1.ChartAreas.First().Area3DStyle.Enable3D = true;
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