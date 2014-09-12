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
using System.IO;

namespace CustomAnnotation
{
    public enum LOGGINGSTATE
    {
        NONE,
        LOGGING,
        PAUSED,
        POPULATED,
        EXPORTED
    }

    public partial class Logging : UserControl
    {

        LOGGINGSTATE mLoggingState { get; set; }
        
        //private bool mLoggingStatus;
        public string mFilename { get; set; }
        public string mObjectiveCriteria { get; set; }
        public string mCoderName { get; set; }
        
        public List<Log> mLogList = new List<Log>();

        public Logging(Grid placeholder, string objectiveCriteria, string coderName)
        {
            placeholder.Children.Add(this); 
            InitializeComponent();
            mObjectiveCriteria = objectiveCriteria;
            mCoderName = coderName;

            mLoggingState = LOGGINGSTATE.NONE;
            
        }

        public LOGGINGSTATE GetLoggingState()
        {
            return mLoggingState;
        }
        public void SetLoggingState(LOGGINGSTATE ls)
        {
            mLoggingState = ls;

            if (ls == LOGGINGSTATE.NONE)
                lstatus.Content = "Idle...";
            else if (ls == LOGGINGSTATE.LOGGING)
                lstatus.Content = "Logging...";
            else if (ls == LOGGINGSTATE.PAUSED)
                lstatus.Content = "Logging has been Paused...";
            else if (ls == LOGGINGSTATE.POPULATED)
                lstatus.Content = "Log needs to be exported...";
            else if (ls == LOGGINGSTATE.EXPORTED)
                lstatus.Content = "Logfile has been exported...";
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

        public void Reset()
        {
            Clear();
        }

        public void Clear()
        {
            mLogList.Clear();
        }

        public void ExportLog(object sender, RoutedEventArgs e)
        {
            if (GetLoggingState() == LOGGINGSTATE.LOGGING)
                return;
            
            string[] lines = new string[mLogList.Count+3];

            lines[0] = string.Format("Coder_Name = {0}", mCoderName); 
            lines[1] = string.Format("Objective_Criteria = {0}", mObjectiveCriteria);
            lines[2] = string.Format("{0}\t{1}\t{2}","App_Timer", "Annotation_Value", "Video_Time");

            for (int i = 0; i < mLogList.Count; i++)
            {
                string t = DateTime.Now.ToString("hh:mm:ss:fff");
                lines[i + 3] = string.Format("{0}\t{1:0.00}\t{2}", mLogList[i].Time, mLogList[i].LogValue, mLogList[i].mVideoTime);
            }

            try
            {
                System.IO.Directory.CreateDirectory("C:\\WOZAnnotationLogs\\"+ mCoderName);
                string fname = NextAvailableFilename("C:\\WOZAnnotationLogs\\" + mCoderName + "\\" + Filename.Text + ".txt");
                System.IO.File.WriteAllLines(@fname, lines);

                SetLoggingState(LOGGINGSTATE.EXPORTED);
            }
            catch (Exception exception)
            {

            }
        }

        private static string numberPattern = " ({0})";

        public static string NextAvailableFilename(string path)
        {
            // Short-cut if already available
            if (!File.Exists(path))
                return path;

            // If path has extension then insert the number pattern just before the extension and return next filename
            if (System.IO.Path.HasExtension(path))
                return GetNextFilename(path.Insert(path.LastIndexOf(System.IO.Path.GetExtension(path)), numberPattern));

            // Otherwise just append the pattern to the path and return next filename
            return GetNextFilename(path + numberPattern);
        }

        private static string GetNextFilename(string pattern)
        {
            string tmp = string.Format(pattern, 1);
            if (tmp == pattern)
                throw new ArgumentException("The pattern must include an index place-holder", "pattern");

            if (!File.Exists(tmp))
                return tmp; // short-circuit if no matches

            int min = 1, max = 2; // min is inclusive, max is exclusive/untested

            while (File.Exists(string.Format(pattern, max)))
            {
                min = max;
                max *= 2;
            }

            while (max != min + 1)
            {
                int pivot = (max + min) / 2;
                if (File.Exists(string.Format(pattern, pivot)))
                    min = pivot;
                else
                    max = pivot;
            }

            return string.Format(pattern, max);
        }

    }
}
