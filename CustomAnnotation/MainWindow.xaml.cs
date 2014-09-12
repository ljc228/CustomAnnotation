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

using System.Timers;

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

    public enum LOCALSTATE
    {
        NONE,
        READYTOPLAY,
        PAUSED,
        PLAYING
    }

    public partial class MainWindow : Window
    {
        
        public APPSTATE mState;
        
        DispatcherTimer mStartupCheckTimer;
        VideoPlayer mVideoPlayer;
        ObjectiveCriteria mObjectiveCriteria;
        JoystickAnnotate mJoystickAnnotate;
        Logging mLogging;
        LevelChange mLevelChange;

        DispatcherTimer mTimer;
        System.Timers.Timer mPausableLoggingTimer;
        DispatcherTimer mJoystickTickTimer;

        int debugIndex = 0;

        DateTime startTime;
        DateTime mPauseTime;
        System.TimeSpan mAccumulatedVoidTime;

        LOCALSTATE mLocalState = LOCALSTATE.NONE;

        public MainWindow(string objectiveCriteria, string coderName)
        {
            InitializeComponent();

            mVideoPlayer = new VideoPlayer(VideoPlayerHolder);
            mObjectiveCriteria = new ObjectiveCriteria(ObjectiveCriteriaHolder, objectiveCriteria);
            mJoystickAnnotate = new JoystickAnnotate(GamePadAnnotationHolder);
            mLogging = new Logging(LoggingDetailsHolder, objectiveCriteria, coderName);
            mLevelChange = new LevelChange(LevelChangeHolder);
            mState = APPSTATE.NONE;


            mTimer = new DispatcherTimer();
            mTimer.Interval = TimeSpan.FromMilliseconds(200);
            mTimer.Tick += new EventHandler(TimerTick);
            mTimer.Start();

            mPausableLoggingTimer = new System.Timers.Timer();
            mPausableLoggingTimer.Elapsed += new ElapsedEventHandler(LogInterval);
            mPausableLoggingTimer.Interval = 200;
            mPausableLoggingTimer.Enabled = false;

            mStartupCheckTimer = new DispatcherTimer();
            mStartupCheckTimer.Interval = TimeSpan.FromMilliseconds(250);
            mStartupCheckTimer.Tick += new EventHandler(mStartupCheckTimerTick);
            mStartupCheckTimer.Start();

            mJoystickTickTimer = new DispatcherTimer();
            mJoystickTickTimer.Interval = TimeSpan.FromMilliseconds(10);
            mJoystickTickTimer.Tick += new EventHandler(JoystickTick);
            mJoystickTickTimer.Start();
        }

        void LogInterval(object sender, EventArgs e)
        {
            System.TimeSpan diff1 = DateTime.Now.Subtract(startTime);
            System.TimeSpan diff2 = diff1.Subtract(mAccumulatedVoidTime);

            App.Current.Dispatcher.Invoke((System.Action)delegate()
            {
                Log log = new Log();
                log.Time = diff2.ToString(@"hh\:mm\:ss\:fff", System.Globalization.CultureInfo.InvariantCulture);
                log.mVideoTime = DateTime.Today.Add(mVideoPlayer.GetVideoPosition()).ToString("HH:mm:ss:fff");
                log.mVideoTimeShort = DateTime.Today.Add(mVideoPlayer.GetVideoPosition()).ToString("HH:mm:ss");
                log.LogValue = Math.Round(mJoystickAnnotate.GetValue() / 1000, 2);
                log.mLogIndex = debugIndex % 5;
                log.mDebugIndex = debugIndex;
                mLogging.Add(log);

                debugIndex++;
            
            });
        }

        void JoystickTick(object sender, EventArgs e)
        {
            ButtonState mButtonState = mJoystickAnnotate.GetButtonState();

            if (mButtonState == ButtonState.BUTTONLTPRESSED)
            {
                mVideoPlayer.Pause();
                return;
            }
            else if (mButtonState == ButtonState.BUTTONRTPRESSED)
            {
                mVideoPlayer.Play();
                return;
            } 

        }

        void TimerTick(object sender, EventArgs e)
        {
            // Has the gamepad been removed?
            if (mJoystickAnnotate.IsConnected == ConnectedState.NONE)
            {
                mVideoPlayer.Pause();
               
                if (mPausableLoggingTimer.Enabled == true)
                    mPausableLoggingTimer.Enabled = false;

                mLogging.SetLoggingState(LOGGINGSTATE.PAUSED);
                mStartupCheckTimer.Start();
                return;
            }

            if (mVideoPlayer.State == STATE.ENDED)
            {
                if (mLogging.GetLoggingState() != LOGGINGSTATE.POPULATED && mLogging.GetLoggingState() != LOGGINGSTATE.EXPORTED)
                {
                    if (mPausableLoggingTimer.Enabled == true)
                        mPausableLoggingTimer.Enabled = false; 
                
                    mLogging.SetLoggingState(LOGGINGSTATE.POPULATED);
                    mLogging.ExportLog(null, null);
                    UserMessage.Content = "Media Playback has ended -  Log has been automatically exported!!";
                }

                return;
            }
            else if (mVideoPlayer.State == STATE.NONE)
                Media_Status.Source = new BitmapImage(new Uri("./images/UIX_02.png", UriKind.RelativeOrAbsolute));


            if (mVideoPlayer.State == STATE.PLAYING && mJoystickAnnotate.IsConnected == ConnectedState.CONNECTED)
            {
                
                if(mLocalState == LOCALSTATE.PAUSED)
                {
                    System.TimeSpan diff1 = DateTime.Now.Subtract(mPauseTime);
                    mAccumulatedVoidTime += diff1;
                    mLocalState = LOCALSTATE.PLAYING;
                    UserMessage.Content = "Playing Media...";
                }

                if (mLocalState == LOCALSTATE.READYTOPLAY)
                {
                    mLocalState = LOCALSTATE.PLAYING;
                    UserMessage.Content = "Playing Media...";
                    ResetPrimaryItems();
                    startTime = DateTime.Now;
                    mPausableLoggingTimer.Start();     
                }

                if (mPausableLoggingTimer.Enabled == false)
                    mPausableLoggingTimer.Enabled = true;

                mLogging.SetLoggingState(LOGGINGSTATE.LOGGING);

                //if (mJoystickAnnotate.GetButtonState() == ButtonState.BUTTON2PRESSED)
                //{
                //    mVideoPlayer.Pause();
                //    return;
                //}

                mLevelChange.State = CHANGESTATE.READY;
                mLevelChange.UpdateLevels(mLogging.mLogList);
            }
             
            else if (mVideoPlayer.State == STATE.PAUSED)
            {
                if (mLocalState == LOCALSTATE.PAUSED)
                    return;

                mPauseTime = DateTime.Now;
                mLocalState = LOCALSTATE.PAUSED;
                
                if (mLogging.GetLoggingState() == LOGGINGSTATE.LOGGING)
                {
                    if (mPausableLoggingTimer.Enabled == true)
                        mPausableLoggingTimer.Enabled = false;

                    mLogging.SetLoggingState(LOGGINGSTATE.PAUSED);
                    UserMessage.Content = "Media is Paused...";
                }

                //if (mJoystickAnnotate.GetButtonState() == ButtonState.BUTTON2PRESSED)
                //{
                //    mVideoPlayer.Play();
                //    return;
                //} 
                
                if (mLevelChange.State != CHANGESTATE.UPDATED)
                    mLevelChange.UpdateLevels(mLogging.mLogList);
                
                if (LevelChangeHolder.Visibility == Visibility.Hidden)
                    LevelChangeHolder.Visibility = Visibility.Visible;
                return;
            }

            else if (mVideoPlayer.State == STATE.MEDIACHANGE)
            {
                UserMessage.Content = "Waiting for Media to load...";

                if (mVideoPlayer.ChangeMedia() == true)
                {
                    Media_Status.Source = new BitmapImage(new Uri("./images/UIX_02.png", UriKind.RelativeOrAbsolute));
                    mTimer.Stop();

                    if (mPausableLoggingTimer.Enabled == true)
                        mPausableLoggingTimer.Enabled = false;

                    mLogging.SetLoggingState(LOGGINGSTATE.NONE);
                    mLogging.Clear();
                    mLevelChange.Clear();

                    mStartupCheckTimer.Start();   
                }
            }
            else if (mVideoPlayer.State == STATE.RESTARTED)
            {
                /* A CHANGE OF MEDIA IS REQUIRED */

                if (mPausableLoggingTimer.Enabled == true)
                    mPausableLoggingTimer.Stop();

                mLogging.SetLoggingState(LOGGINGSTATE.NONE);
                mLogging.Clear();
                mLevelChange.Clear();

                UserMessage.Content = "Media has been Restarted...";
                mVideoPlayer.State = STATE.READY;
                mLocalState = LOCALSTATE.READYTOPLAY;
            }

            else
                mLogging.SetLoggingState(LOGGINGSTATE.NONE);
        }

        


        void mStartupCheckTimerTick(object sender, EventArgs e)
        {        
            if(mJoystickAnnotate.IsConnected == ConnectedState.NONE)
            {
                if (mJoystickAnnotate.AttemptConnect())
                {
                    mJoystickAnnotate.IsConnected = ConnectedState.CONNECTED;
                    GP_Status.Source = new BitmapImage(new Uri("./images/UIX_03.png", UriKind.RelativeOrAbsolute));
                    UserMessage.Content = "GAMEPAD CONNECTED";

                }
                else
                {
                    GP_Status.Source = new BitmapImage(new Uri("./images/UIX_01.png", UriKind.RelativeOrAbsolute));
                    UserMessage.Content = "!! NO GAMEPAD DETECTED !!";
                    return;
                }              
            }
          

            if (mVideoPlayer.State == STATE.MEDIACHANGE)
            {
                mVideoPlayer.ChangeMedia();
            }

            else if (mVideoPlayer.State == STATE.MEDIALOADED)
            {
                /* A CHANGE OF MEDIA IS REQUIRED */

                if (mPausableLoggingTimer.Enabled == true)
                    mPausableLoggingTimer.Enabled = false;

                UserMessage.Content = "Media is loaded...";
                mLogging.SetFileName(mVideoPlayer.mFilename);

                Media_Status.Source = new BitmapImage(new Uri("./images/UIX_04.png", UriKind.RelativeOrAbsolute));
                mVideoPlayer.State = STATE.READY;

                mLocalState = LOCALSTATE.READYTOPLAY;
                mStartupCheckTimer.Stop(); 
                mTimer.Start();
                
            }           
        }

        private void ResetPrimaryItems()
        {
            mAccumulatedVoidTime = TimeSpan.Zero;
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
                mVideoPlayer.Restart();
            }
            else if (e.Key == System.Windows.Input.Key.F8)
            {
                mVideoPlayer.Open();
            }
        }



    }
}
