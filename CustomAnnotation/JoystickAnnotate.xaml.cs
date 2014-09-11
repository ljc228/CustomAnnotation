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

using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;

namespace CustomAnnotation
{

    public enum ButtonState
    {
        NONE,
        BUTTON1PRESSED,
        BUTTON2PRESSED,
        BUTTON3PRESSED,
        BUTTON4PRESSED
    }

    public enum ConnectedState
    {
        NONE,
        CONNECTED,
        CONFIRMED
    }


    
    public partial class JoystickAnnotate : UserControl
    {

        private DispatcherTimer mPollGamepadTimer = new DispatcherTimer();
        private Device mGamepad;
        private JoystickState mGamePadState;
        public ConnectedState IsConnected { get; set; }
        public ButtonState mButtonState { get; set; }

        private int mThumbStick { get; set; }


        public JoystickAnnotate(Grid placeholder)
        {
            placeholder.Children.Add(this);

            InitializeComponent();

            IsConnected = ConnectedState.NONE;
        }

        public bool AttemptConnect()
        {
            if (InitDevices() == true)
            {
                mThumbStick = 0;

                BrushConverter bc = new BrushConverter();
                Right.Background = (Brush)bc.ConvertFrom("#FF343434");
                Right.Foreground = Brushes.Red;

                Left.Background = Brushes.Green;
                Left.Foreground = Brushes.Black;
                return true;
            }
            else
                return false;
        }
        
        private void Connect(object sender, RoutedEventArgs e)
        {
            
                if (((System.Windows.Controls.Button)sender).Tag.ToString() == "L")
                {
                    mThumbStick = 0;

                    BrushConverter bc = new BrushConverter();
                    Right.Background = (Brush)bc.ConvertFrom("#FF343434");
                    Right.Foreground = Brushes.Red;

                    Left.Background = Brushes.Green;
                    Left.Foreground = Brushes.Black;
                }
                else if (((System.Windows.Controls.Button)sender).Tag.ToString() == "R")
                {
                    mThumbStick = 1;

                    BrushConverter bc = new BrushConverter();  
                    Left.Background =  (Brush)bc.ConvertFrom("#FF343434");
                    Left.Foreground = Brushes.Red;

                    Right.Background = Brushes.Green;
                    Right.Foreground = Brushes.Black;
                }
        }

        public double GetValue()
        {
            // Return a normalised double

            if(mThumbStick == 0)
                return mGamePadState.Y *-1;
            else
                return mGamePadState.Rz * -1;
        }

        public ButtonState GetButtonState()
        {
            ButtonState rS = mButtonState;
            mButtonState = ButtonState.NONE;
            return rS;
        }

       
        public bool InitDevices()
        {
            //create joystick device.
            foreach (DeviceInstance di in Manager.GetDevices(
                DeviceClass.GameControl,
                EnumDevicesFlags.AttachedOnly))
            {
                mGamepad = new Device(di.InstanceGuid);
                break;
            }

            if (mGamepad == null)
            {
                //Throw exception if joystick not found.
                return false;
            }

            //Set joystick axis ranges.
            else
            {
                foreach (DeviceObjectInstance doi in mGamepad.Objects)
                {
                    if ((doi.ObjectId & (int)DeviceObjectTypeFlags.Axis) != 0)
                    {
                        mGamepad.Properties.SetRange(
                            ParameterHow.ById,
                            doi.ObjectId,
                            new InputRange(-1000, 1000));
                    }

                }

                mGamepad.Properties.AxisModeAbsolute = true;
                mGamepad.SetCooperativeLevel(new WindowInteropHelper(System.Windows.Application.Current.MainWindow).Handle, CooperativeLevelFlags.NonExclusive | CooperativeLevelFlags.Background);

                //Acquire devices for capturing.
                mGamepad.Acquire();
                mGamePadState = mGamepad.CurrentJoystickState;

            }

            mPollGamepadTimer.Tick += new EventHandler(GamePadUpdateTick);
            mPollGamepadTimer.Interval = TimeSpan.FromMilliseconds(100);
            mPollGamepadTimer.Start();

            return true;
        }

        public void PollGamePad()
        {
            if (mGamepad != null)
            {
                mGamepad.Poll();
                try
                {
                    mGamePadState = mGamepad.CurrentJoystickState;

                    this.XInput.Content = mGamePadState.X;
                    this.YInput.Content = mGamePadState.Y;

                    if (mThumbStick == 0)
                        this.EngagementSlider.Value = mGamePadState.Y * -1;
                    else
                        this.EngagementSlider.Value = mGamePadState.Rz * -1;


                    if (mButtonState == ButtonState.NONE)
                    {
                        //Capture Buttons.
                        byte[] buttons = mGamePadState.GetButtons();
                        for (int i = 0; i < buttons.Length; i++)
                        {
                            if (buttons[1] != 0)
                            {
                                mButtonState = ButtonState.BUTTON1PRESSED;
                            }
                            else if (buttons[2] != 0)
                            {
                                mButtonState = ButtonState.BUTTON2PRESSED;
                            }
                            else if (buttons[3] != 0)
                            {
                                mButtonState = ButtonState.BUTTON3PRESSED;
                            }
                            else if (buttons[4] != 0)
                            {
                                mButtonState = ButtonState.BUTTON4PRESSED;
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    IsConnected = ConnectedState.NONE;
                    mGamepad = null;
                }
            }
            
        }

        
        private void GamePadUpdateTick(object sender, EventArgs e)
        {
            // Updating the Label which displays the current second
            PollGamePad();

            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }

    }
}
