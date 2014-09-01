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

using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;

using System.Windows.Controls.Primitives;

namespace CustomAnnotation{

    public enum APPSTATE
    {
        NONE,
        GAMEPADCONNECTED,
        MEDIALOADED,
        READY,
    }

    public partial class MainWindow : Window
    {
        
        public APPSTATE mState;
        DispatcherTimer mLoggingTimer;
        DispatcherTimer mStartupCheckTimer;
        VideoPlayer mVideoPlayer;
        JoystickAnnotate mJoystickAnnotate;
        Logging mLogging;
        LevelChange mLevelChange;

        public MainWindow()
        {
            InitializeComponent();

            mVideoPlayer = new VideoPlayer(VideoPlayerHolder);
            mJoystickAnnotate = new JoystickAnnotate(GamePadAnnotationHolder);
            mLogging = new Logging(LoggingDetailsHolder);
            mLevelChange = new LevelChange(LevelChangeHolder);
            mState = APPSTATE.NONE;
            
            mLoggingTimer = new DispatcherTimer();
            mLoggingTimer.Interval = TimeSpan.FromMilliseconds(250);
            mLoggingTimer.Tick += new EventHandler(LoggingTimerTick);
            mLoggingTimer.Start();

            mStartupCheckTimer = new DispatcherTimer();
            mStartupCheckTimer.Interval = TimeSpan.FromMilliseconds(250);
            mStartupCheckTimer.Tick += new EventHandler(mStartupCheckTimerTick);
            mStartupCheckTimer.Start();
        }

        void LoggingTimerTick(object sender, EventArgs e)
        {
            // Has the gamepad been removed?
            if (mJoystickAnnotate.IsConnected == ConnectedState.NONE)
            {
                mVideoPlayer.Pause();
                mLogging.LoggingStatus = false;
                mStartupCheckTimer.Start();
                return;
            }

            if (mVideoPlayer.State == STATE.ENDED)
            {
                mLogging.LoggingStatus = false;
                UserMessage.Content = "Media Playback has ended -  Export Log!!";
                return;
            }
            else if (mVideoPlayer.State == STATE.READY)
                UserMessage.Content = "";
            else if (mVideoPlayer.State == STATE.NONE)
                Media_Status.Source = new BitmapImage(new Uri("./images/UIX_02.png", UriKind.RelativeOrAbsolute));


            if (mVideoPlayer.State == STATE.PLAYING && mJoystickAnnotate.IsConnected == ConnectedState.CONNECTED)
            {
                if (mLogging.LoggingStatus == false)
                {
                    mLogging.LoggingStatus = true;
                    UserMessage.Content = "";
                }

                if (mJoystickAnnotate.GetButtonState() == ButtonState.BUTTON2PRESSED)
                {
                    mVideoPlayer.Pause();
                    return;
                }

                Log log = new Log();
                log.Time = DateTime.Today.Add(mVideoPlayer.GetVideoPosition()).ToString("HH:mm:ss");
                log.LogValue = Math.Round(mJoystickAnnotate.GetValue() / 10000, 2);
                mLogging.Add(log);
                
                mLevelChange.State = CHANGESTATE.READY;
                mLevelChange.UpdateLevels(mLogging.mLogList);
            }
            else if (mVideoPlayer.State == STATE.PAUSED)
            {
                mLogging.LoggingStatus = false; 
                
                if (mJoystickAnnotate.GetButtonState() == ButtonState.BUTTON2PRESSED)
                {
                    mVideoPlayer.Play();
                    return;
                } 
                
                if (mLevelChange.State != CHANGESTATE.UPDATED)
                    mLevelChange.UpdateLevels(mLogging.mLogList);
                
                if (LevelChangeHolder.Visibility == Visibility.Hidden)
                    LevelChangeHolder.Visibility = Visibility.Visible;
                return;
            }
            else if (mVideoPlayer.State == STATE.STOPPED)
            {
                /* SHOULD THIS BE CLEARING HERE? */
                mLogging.LoggingStatus = false;
                mLogging.Clear();
                mLevelChange.Clear();
                mVideoPlayer.State = STATE.READY;
            }
            else
                mLogging.LoggingStatus = false;

        }

        void mStartupCheckTimerTick(object sender, EventArgs e)
        {        
            if(mJoystickAnnotate.IsConnected == ConnectedState.NONE)
            {
                if (mJoystickAnnotate.AttemptConnect())
                {
                    mJoystickAnnotate.IsConnected = ConnectedState.CONNECTED;
                    GP_Status.Source = new BitmapImage(new Uri("./images/UIX_03.png", UriKind.RelativeOrAbsolute));
                    UserMessage.Content = "GAMEPAD IS CONNECTED";

                }
                else
                {
                    GP_Status.Source = new BitmapImage(new Uri("./images/UIX_01.png", UriKind.RelativeOrAbsolute));
                    UserMessage.Content = "!! NO GAMEPAD DETECTED !!";
                    return;
                }              
            }
          
            
            if (mVideoPlayer.State == STATE.PLAYING)
            {
                UserMessage.Content = "";
                mStartupCheckTimer.Stop();
            } 
            else if (mVideoPlayer.State == STATE.MEDIALOADED)
            {               
                mLogging.mFilename = mVideoPlayer.mFilename;
                mLogging.Filename.Text = mVideoPlayer.mFilename;
                mLevelChange.Clear();
                Media_Status.Source = new BitmapImage(new Uri("./images/UIX_04.png", UriKind.RelativeOrAbsolute));
                mVideoPlayer.State = STATE.READY;
            }           
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetWindow(this).Close();
        }

       
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F5)
            {
                mVideoPlayer.Play();  
            }
            else if (e.Key == System.Windows.Input.Key.F6)
            {
                mVideoPlayer.Pause();
            }
            else if (e.Key == System.Windows.Input.Key.F7)
            {
                mVideoPlayer.Stop();
            }
            else if (e.Key == System.Windows.Input.Key.F8)
            {
                mVideoPlayer.Open();
            }
        }



    }
}
