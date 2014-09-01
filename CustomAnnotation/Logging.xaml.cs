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

namespace CustomAnnotation
{

    public partial class Logging : UserControl
    {
        private bool mLoggingStatus;
        public string mFilename { get; set; }
        
        public List<Log> mLogList = new List<Log>();

        public Logging(Grid placeholder)
        {
            placeholder.Children.Add(this); 
            InitializeComponent(); 
            
        }

        public bool LoggingStatus
        {
            get
            {
                return mLoggingStatus;
            }
            set
            {
                mLoggingStatus = value;
                if (value == false)
                    lstatus.Content = "Idle...";
                else
                    lstatus.Content = "Logging...";
            }
        }

        public void SetFileName(string fn)
        {
            mFilename = fn;
            Filename.Text = fn;
        }

        public void Add(Log log)
        {
            mLogList.Add(log);
        }

        public void Clear()
        {
            mLogList.Clear();
        }

        private void ExportLog(object sender, RoutedEventArgs e)
        {
            if (mLoggingStatus == true)
                return;
            
            string[] lines = new string[mLogList.Count];

            for (int i = 0; i < mLogList.Count; i++)
            {
                lines[i] = string.Format("{0},{1}", mLogList[i].Time, mLogList[i].LogValue);
            }

            try
            {
                System.IO.Directory.CreateDirectory("C:\\WOZAnnotationLogs");
                System.IO.File.WriteAllLines(@"C:\WOZAnnotationLogs\" + Filename.Text + ".txt", lines);
            }
            catch (Exception exception)
            {

            }
        }

    }
}
