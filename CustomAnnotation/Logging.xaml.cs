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
                string fname = NextAvailableFilename("C:\\WOZAnnotationLogs\\" + Filename.Text + ".txt");
                System.IO.File.WriteAllLines(@fname, lines);
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
