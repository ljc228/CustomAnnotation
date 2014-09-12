using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomAnnotation
{
    public class Log
    {
        public string mVideoTime{get; set;}
        public string mVideoTimeShort { get; set; }
        string mTime;
        double mLogValue;
        public int mLogIndex;
        public int mDebugIndex;

        public string Time
        {
            get
            {
                return mTime;
            }
            set
            {
                mTime = value;
            }

        }

        public double LogValue
        {
            get
            {
                return mLogValue;
            }
            set
            {
                mLogValue = value;
            }

        }
    }
}
