using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace MediaPlayer
{
    public partial class Form1 : Form
    {
        public SoundPlayer soundPlayer;

        public ThreadStart songPositionthreadStart;
        public Thread readSongPositionThread;

        public ThreadStart currentTimeThreadStart;
        public Thread readCurrentTimeThread;

        public const string chartingAreaName = "Draw";
        public static string songFileName = string.Empty;

       
        public Form1()
        {
            InitializeComponent();
            InitializeChart();
            SoundPlayer.songLoadedEvent += new EventHandler(songLoadedEvent);

            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);

        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach(string songFilename in files) {

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
            //PlayButton_Click(null, null);
            
        }

        public void InitializeChart()
        {
            chart1.Series.First().XValueMember = "X";
            chart1.Series.First().YValueMembers = "Y";

            chart1.DataSource = GraphAudioBuffer.audioData;
            chart1.DataBind();

            chart1.ChartAreas.First().AxisY.Minimum = -32000.0;
            chart1.ChartAreas.First().AxisY.Maximum = 32000.0;


            // Info: https://www.daniweb.com/software-development/csharp/code/451281/simple-line-graph-charting
    

            Controls.Add(chart1);// is this necessary?
            

        }

       
        private void PlayButton_Click(object sender, EventArgs e)
        {
            if (soundPlayer == null)
            {
              // songName.Text = "Please Select a song first. :) ";
            }
            else
            {
                if(soundPlayer.waveOutDevice.PlaybackState == PlaybackState.Playing ){
                // Don't do anything
                }
                else
                {
                    songPositionthreadStart = new ThreadStart(ReadSongPositionAsync);
                    readSongPositionThread = new Thread(songPositionthreadStart);

                    currentTimeThreadStart = new ThreadStart(ReadCurrentSongTime);
                    readCurrentTimeThread = new Thread(currentTimeThreadStart);
                    
                    readSongPositionThread.Start();
                    readCurrentTimeThread.Start();
                }
                soundPlayer.Play();
            }
        }

        public void ReadSongPositionAsync()
        {
            while (soundPlayer.mp3FileReader.Position < soundPlayer.mp3FileReader.Length)
            {
               // System.Diagnostics.Debug.WriteLine("Position: " + soundPlayer.mp3FileReader.Position + "            \r");

                MethodInvoker inv1 = delegate
                {

                    soundPlayer.updateGraph(Convert.ToInt32(soundPlayer.mp3FileReader.Position));
                    chart1.DataBind();
                    chart1.Update();

                };
                this.Invoke(inv1);


                Thread.Sleep(1);
            }
        }

        public void ReadCurrentSongTime(){

            while (soundPlayer.mp3FileReader.CurrentTime < soundPlayer.mp3FileReader.TotalTime)
            {
                MethodInvoker inv = delegate
                {
                    this.songTimeLabel.Text = soundPlayer.mp3FileReader.CurrentTime.ToString().Remove(
                        soundPlayer.mp3FileReader.CurrentTime.ToString().Length-5,5);


                };
                this.Invoke(inv);
                Thread.Sleep(100);
            }
        }

        public void ReadAudioRealtime()
        {

        }

        private void PauzeButton_Click(object sender, EventArgs e)
        {
            if(readCurrentTimeThread !=null)
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

            songPositionthreadStart = new ThreadStart(ReadSongPositionAsync);
            readSongPositionThread = new Thread(songPositionthreadStart);

            currentTimeThreadStart = new ThreadStart(ReadCurrentSongTime);
            readCurrentTimeThread = new Thread(currentTimeThreadStart);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void songTime(object sender, EventArgs e)
        {

        }

        string lastSongName = string.Empty;
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "MP3 Audio Files (.mp3)|*.mp3|All Files(*.*)|*.*";
            openFileDialog.FilterIndex = 1;

            openFileDialog.Multiselect = false;

            DialogResult result = openFileDialog.ShowDialog();
            if(result == DialogResult.OK)
            {
                reset();
                songFileName = openFileDialog.FileName;
                lastSongName = songFileName;

                songLoadingProgressBar.Visible = true;
                songLoadingProgressBar.Style = ProgressBarStyle.Marquee;
                songLoadingLabel.Visible = true;
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

        public void loadSong(object fileName){

            soundPlayer = new SoundPlayer((fileName).ToString());
            PlayButton_Click(null, null);
        }
      
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Andrew's little Media Player :)\nCopyright (c) 2015 Andrew Servania");
        }

        public void clear()
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            reset(); 
        }

        private void StopButton_Click(object sender, EventArgs e)
        {

            songTimeLabel.Text = "00:00:00.00";

            if (soundPlayer != null)
            {
                soundPlayer.Stop();
            }
            
            if(readSongPositionThread!= null)
            {
                readSongPositionThread.Abort();

            }

            if(readCurrentTimeThread != null)
            {
                readCurrentTimeThread.Abort();

            }

            songPositionthreadStart = new ThreadStart(ReadSongPositionAsync);
            readSongPositionThread = new Thread(songPositionthreadStart);

            currentTimeThreadStart = new ThreadStart(ReadCurrentSongTime);
            readCurrentTimeThread = new Thread(currentTimeThreadStart);



            songTimeLabel.Text = "00:00:00.00";


        }

        private void volumeBar_Scroll(object sender, ScrollEventArgs e)
        {
            if (soundPlayer != null)
            {
                double percent = 1.0-(e.NewValue/91.0);
                volumePercentageLabel.Text = Math.Round(percent*100.0) + "%";
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




    }
}
