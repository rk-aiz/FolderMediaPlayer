using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FolderMediaPlayer
{
    internal class RequestSeek
    {
        TimeSpan? _nextTargetTime = null;
        TimeSpan? nextTargetTime
        {
            get { return _nextTargetTime; }
            set { _nextTargetTime = value; }
        }

        private int wait = 500;

        TimeSpan? _procTargetTime = null;
        TimeSpan? procTargetTime
        {
            get { return _procTargetTime; }
            set
            {
                if (value != null)
                {
                    this._procTargetTime = value;
                }
                else if (nextTargetTime != null)
                {
                    this._procTargetTime = nextTargetTime;
                    this.nextTargetTime = null;

                }
                else
                {
                    this._procTargetTime = null;
                    return;
                }

                OnSeekProc();
            }
        }

        private async void OnSeekProc()
        {
            SeekProc?.Invoke(this, new SeekProcEventArgs(procTargetTime.Value));
            await Task.Delay(wait);
            procTargetTime = null;
        }

        public void Request(TimeSpan time)
        {
            if (procTargetTime == null)
            {
                procTargetTime = time;
            }
            else
            {
                nextTargetTime = time;
            }
        }

        public delegate void SeekProcEventHandler(object sender, SeekProcEventArgs e);
        public event SeekProcEventHandler SeekProc;

        public RequestSeek()
        {

        }
    }

    public class SeekProcEventArgs : EventArgs
    {
        public TimeSpan TargetTime;
        public SeekProcEventArgs(TimeSpan ts)
        {
            TargetTime = ts;
        }
    }
}
