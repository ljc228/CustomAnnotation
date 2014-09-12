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


using ColourSliderLibrary;
using System.Collections;

namespace CustomAnnotation
{

    public enum CHANGESTATE
    {
        NONE,
        READY,
        UPDATED,
        CHANGED
    }

    public partial class LevelChange : UserControl
    {
        ColourSlider[] sliders;
        Label[] mLabels;
        Grid mPlaceholder;
        public CHANGESTATE State { get; set; }
        public List<Log> mLogList = new List<Log>();
        
        public LevelChange(Grid placeholder)
        {
            mPlaceholder = placeholder;
            CreateSliders();
            mPlaceholder.Children.Add(this);
            InitializeComponent();

            State = CHANGESTATE.NONE;
            Clear();
        }


        public void CreateSliders()
        {
            sliders = new ColourSlider[20];
            mLabels = new Label[20];
                        
            for (int i = 0; i < 20; i++)
            {
                sliders[i] = new ColourSlider();
                sliders[i].Maximum = 1;
                sliders[i].Minimum = -1;
                sliders[i].Width = 80;                
                sliders[i].RenderTransformOrigin = new Point(0.5, 0.5);
                sliders[i].Value = 0;
                            
                Thickness m = sliders[i].Margin;
                m.Left = -850 + (90 * i);
                sliders[i].Margin = m;
                RotateTransform rotateTransform1 = new RotateTransform(-90, 0, 0);
                sliders[i].RenderTransform = rotateTransform1;              
                sliders[i].Visibility = Visibility.Visible;

                mPlaceholder.Children.Add(sliders[i]);

                mLabels[i] = new Label();
                mLabels[i].FontSize = 8;
                mLabels[i].Foreground = Brushes.White;
                mLabels[i].Content = "00:00:00";
                Thickness ma = mLabels[i].Margin;
                ma.Left = 12 + (45 * i);
                ma.Top = 85;
                mLabels[i].Margin = ma;

                mPlaceholder.Children.Add(mLabels[i]);
            }

            State = CHANGESTATE.READY;
            Clear();

        }


        public void Clear()
        {
            for (int i = 0; i < 20; i++)
            {
                mLabels[i].Content = "00:00:00";
                sliders[i].Value = 0;
            }

        }

        public void UpdateLevels(List<Log> l)
        {
            mLogList = l;
            SetLevels();                       
        }

        private void SetLevels()
        {
            int position = mLogList.Count - 1;

            for (int i = 19; i >= 0; i--)
            {
                if (position >= 0)
                {
                    sliders[i].Value = mLogList[position].LogValue;
                    mLabels[i].Content = mLogList[position].mVideoTimeShort;
                    position--;
                }
                else
                    break;
            }

            State = CHANGESTATE.UPDATED;
        }

        private void UpdateChanges(object sender, RoutedEventArgs e)
        {

            int position = mLogList.Count - 1;
            
            for (int i = 19; i >= 0; i--)
            {
                if (position >= 0)
                {
                    mLogList[position-1].LogValue = sliders[i].Value;
                    position--;
                }
                else
                    break;
            }

            State = CHANGESTATE.UPDATED;
        }

        private void ResetChanges(object sender, RoutedEventArgs e)
        {
            SetLevels();
        }

    }
}
