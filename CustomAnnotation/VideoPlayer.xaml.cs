using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;

using WPFMediaKit;



namespace CustomAnnotation
{

    public enum STATE
    {
        NONE,
        MEDIALOADED,
        READY,
        PLAYING,
        PAUSED,
        STOPPED, 
        ENDED
    }

    public partial class VideoPlayer : UserControl
    {
        DispatcherTimer mVideoSeektimer;
        DispatcherTimer mInitStartTimer;
        public string mFilename {get; set;}
        public STATE State { get; set; }
        private bool IsDragging { get; set; }
        private double mCurrentposition { get; set; }


        public VideoPlayer(Grid placeholder)
        {

            placeholder.Children.Add(this);
            
            InitializeComponent();
            mVideoSeektimer = new DispatcherTimer();
            mVideoSeektimer.Interval = TimeSpan.FromMilliseconds(250);
            mVideoSeektimer.Tick += new EventHandler(VideoSeekTimerTick);


            mInitStartTimer = new DispatcherTimer();
            mInitStartTimer.Interval = TimeSpan.FromMilliseconds(350);
            mInitStartTimer.Tick += new EventHandler(InitStartTimerTick);

            State = STATE.NONE;

        }

        void VideoSeekTimerTick(object sender, EventArgs e)
        {

            if (GetVideoPosition() == TimeSpan.FromTicks(bgvideo.MediaDuration))
            {
                State = STATE.ENDED;
                Replay.Visibility = Visibility.Visible;
            }

            
            if (!IsDragging)
            {
                SeekBar.Value = TimeSpan.FromTicks(bgvideo.MediaPosition).TotalSeconds;
                mCurrentposition = SeekBar.Value;
                
                TimeSpan time = TimeSpan.FromSeconds(mCurrentposition);
                VideoPos.Content = DateTime.Today.Add(time).ToString("HH:mm:ss");
            }
            else
            {
                TimeSpan time = TimeSpan.FromSeconds(SeekBar.Value);
                VideoPos.Content = DateTime.Today.Add(time).ToString("HH:mm:ss");
            }


            VideoTime.Content = DateTime.Today.Add(TimeSpan.FromTicks(bgvideo.MediaPosition)).ToString("HH:mm:ss");
            VideoTime1.Content = DateTime.Today.Add(TimeSpan.FromTicks(bgvideo.MediaPosition)).ToString("HH:mm:ss");
            VideoTime2.Content = DateTime.Today.Add(TimeSpan.FromTicks(bgvideo.MediaPosition)).ToString("HH:mm:ss");

        }

        public TimeSpan GetVideoPosition()
        {
            return TimeSpan.FromTicks(bgvideo.MediaPosition);// Position;
        }

        // Play the media. 
        private void OnMouseDownPlayMedia(object sender, MouseButtonEventArgs args)
        {
            if (State == STATE.READY || State == STATE.PAUSED || State == STATE.STOPPED)
            {
                bgvideo.Volume = 3; 
                bgvideo1.Volume = 0;// IsMuted = true;
                bgvideo2.Volume = 0;// .IsMuted = true;

                bgvideo.Play();
                bgvideo1.Play();
                bgvideo2.Play();

                State = STATE.PLAYING;
            }
        }

        // Pause the media. 
        private void OnMouseDownPauseMedia(object sender, MouseButtonEventArgs args)
        {

            if (State == STATE.PLAYING)
            {
                bgvideo.Pause();
                bgvideo1.Pause();
                bgvideo2.Pause();

                State = STATE.PAUSED;
            }
        }

        // Stop the media. 
        private void OnMouseDownStopMedia(object sender, MouseButtonEventArgs args)
        {
            if (State == STATE.PLAYING || State == STATE.PAUSED || State == STATE.ENDED)
            {
                bgvideo.Stop();
                bgvideo1.Stop();
                bgvideo2.Stop();

                bgvideo.MediaPosition = 0;
                bgvideo1.MediaPosition = 0;
                bgvideo2.MediaPosition = 0;

                State = STATE.STOPPED;
                
                bgvideo.Volume = 0;
                bgvideo1.Volume = 0;
                bgvideo2.Volume = 0;

                bgvideo.Play();
                bgvideo1.Play();
                bgvideo2.Play();

                mInitStartTimer.Start();
            }
        }

        public void Play()
        {
            OnMouseDownPlayMedia(null, null);
        }

        public void Pause()
        {
            OnMouseDownPauseMedia(null, null);
        }

        public void Stop()
        {
            OnMouseDownStopMedia(null, null);
        }

        public void Open()
        {
            OpenVideo(null, null);
        }



        // Change the volume of the media. 
        private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            bgvideo.Volume = (double)volumeSlider.Value;
        }

        private void InitializePropertyValues()
        {
            bgvideo.Volume = (int)volumeSlider.Value;
        }

        private void OpenVideo(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".wmv";
            dlg.Filter = "All Files (*.*) | *.*";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;

                //Filename
                string fn = System.IO.Path.GetFileName(filename);
                mFilename = fn.Substring(8, fn.Length - 12);
                string video1 = '1' + fn.Substring(1);
                string video2 = '2' + fn.Substring(1);
                string video3 = '3' + fn.Substring(1);


                // File Location
                string fl = System.IO.Path.GetDirectoryName(filename);

                bgvideo.Source = new Uri(fl + "\\" + video1);
                bgvideo1.Source = new Uri(fl + "\\" + video2);
                bgvideo2.Source = new Uri(fl + "\\" + video3);

                bgvideo.Volume = 0;
                bgvideo1.Volume = 0;// IsMuted = true;
                bgvideo2.Volume = 0;// .IsMuted = true;

                bgvideo.Play();
                bgvideo1.Play();
                bgvideo2.Play();

                State = STATE.NONE;
                mInitStartTimer.Start();
            }
        }

        void InitStartTimerTick(object sender, EventArgs e)
        {
            bgvideo.Stop();
            bgvideo1.Stop();
            bgvideo2.Stop();

            bgvideo.MediaPosition = 0;
            bgvideo1.MediaPosition = 0;
            bgvideo2.MediaPosition = 0;

            mInitStartTimer.Stop();
        }

        private void MediaOpened(object sender, RoutedEventArgs e)
        {
            TimeSpan ts = TimeSpan.FromTicks(bgvideo.MediaDuration);
            SeekBar.Maximum = ts.TotalSeconds;
            SeekBar.SmallChange = 1;
            SeekBar.LargeChange = Math.Min(10, ts.Seconds / 10);

            VideoLength.Content = DateTime.Today.Add(ts).ToString("HH:mm:ss");
            State = STATE.MEDIALOADED;
            mVideoSeektimer.Start();
        }


        private void seekBar_DragStarted(object sender, DragStartedEventArgs e)
        {
            IsDragging = true;
        }

        private void seekBar_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            IsDragging = false;

            bgvideo.MediaPosition = TimeSpan.FromSeconds(SeekBar.Value).Ticks;
            bgvideo1.MediaPosition = TimeSpan.FromSeconds(SeekBar.Value).Ticks;
            bgvideo2.MediaPosition = TimeSpan.FromSeconds(SeekBar.Value).Ticks;

        }

        private void VideoControls_MouseEnter(object sender, MouseEventArgs e)
        {
            PlayButton.Visibility = Visibility.Visible;
            PauseButton.Visibility = Visibility.Visible;
            StopButton.Visibility = Visibility.Visible;
            OpenButton.Visibility = Visibility.Visible;
            volumeSlider.Visibility = Visibility.Visible;

        }

        private void VideoControls_MouseLeave(object sender, MouseEventArgs e)
        {
            PlayButton.Visibility = Visibility.Hidden;
            PauseButton.Visibility = Visibility.Hidden;
            StopButton.Visibility = Visibility.Hidden;
            OpenButton.Visibility = Visibility.Hidden;
            volumeSlider.Visibility = Visibility.Hidden;
        }

        private void Replay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Replay.Visibility = Visibility.Hidden;
            Stop();
        }

    }
}
