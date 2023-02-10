using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Configuration;
using System.Windows.Threading;
using System.Collections.Specialized;

namespace FolderMediaPlayer
{
    internal class MediaHelper : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            //Debug.WriteLine("NotifyPropertyChanged [{0}] : {1}", propertyName, this.GetType().GetProperty(propertyName).GetValue(this));

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private MediaClock mediaClock { get; set; }

        private ClockController _mediaController;
        public ClockController mediaController
        {
            get
            {
                return this._mediaController;
            }
            set
            {
                this._mediaController = value;
                NotifyPropertyChanged();
            }
        }

        public void TogglePauseAndResume()
        {
            if (this.mediaClock == null)
                return;

            if (ClockState.Active == this.mediaClock.CurrentState)
            {
                if (true == this.mediaClock.IsPaused)
                {
                    this.mediaController.Resume();
                }
                else
                {
                    this.mediaController.Pause();
                }
            }
            else
            {
                this.mediaController.Begin();
                this.playing = true;
            }
        }

        public void ToggleStopAndPlay()
        {
            if (this.mediaClock == null)
                return;

            if (ClockState.Active == this.mediaClock.CurrentState)
            {
                this.mediaController.Stop();
                this.playing = false;
            }
            else
            {
                this.mediaController.Begin();
                this.playing = true;
            }
        }

        private TimeSpan smallStep = new TimeSpan(0, 0, 5);
        private TimeSpan mediumStep = new TimeSpan(0, 0, 20);
        private TimeSpan largeStep = new TimeSpan(0, 1, 0);

        public void JumpForwardSmallStep()
        {
            Seek(smallStep);
        }

        public void JumpBackwardSmallStep()
        {
            Seek(-1 * smallStep);  
        }

        public void JumpForwardMediumStep()
        {
            Seek(mediumStep);
        }

        public void JumpBackwardMediumStep()
        {
            Seek(-1 * mediumStep);
        }

        public void JumpForwardLargeStep()
        {
            Seek(largeStep);
        }

        public void JumpBackwardLargeStep()
        {
            Seek(-1 * largeStep);
        }

        public void JumpToBegin()
        {
            Seek(TimeSpan.Zero, true);
        }

        public void JumpToEnd()
        {
            if (this.mediaClock == null)
                return;

            var targetTime = this.mediaClock.NaturalDuration.TimeSpan - TimeSpan.FromSeconds(1.0);
            Seek(targetTime, true);
        }

        public void NextMedia()
        {
            this.playing = false;
            this._mediaFailed = false;
            this._mediaEnded = false;
            SetMediaSourceFromNextFile();
            this.playing = true;
        }

        public void PreviousMedia()
        {
            this.playing = false;
            this._mediaFailed = false;
            this._mediaEnded = false;
            SetMediaSourceFromNextFile(true);
            this.playing = true;
        }

        private DateTime lastSeekTime;
        private TimeSpan seekInterval = new TimeSpan(0, 0, 0, 0, 300);

        public void Seek(TimeSpan offset, bool absolute = false)
        {
            if (this.mediaClock == null)
                return;

            if (DateTime.UtcNow - this.lastSeekTime < seekInterval)
            {
                Debug.WriteLine("Seek interval");
                return;
            }

            TimeSpan targetTime = TimeSpan.Zero;
            if (absolute)
            {
                targetTime = offset;
            }
            else
            {
                var currentTime = this.mediaClock.CurrentTime ?? TimeSpan.Zero;
                targetTime = currentTime + offset;
            }

            if (targetTime < TimeSpan.Zero)
                targetTime = TimeSpan.Zero;

            bool isStopped = !this.playing;
            
            this.mediaController.SeekAlignedToLastTick(targetTime, TimeSeekOrigin.BeginTime);
            this.lastSeekTime = DateTime.UtcNow;

            NotifyPropertyChanged("mediaPlaybackTime");

            if (isStopped)
            {
                this.mediaController.Pause();
            }
        }

        private void OnMediaSourceChanged()
        {
            this.mediaClock = new MediaTimeline(new Uri(this.mediaSource)).CreateClock();
            this.mediaClock.Completed += (sender, e) =>
            {
                this.mediaEnded = true;
            };
            this.mediaController = this.mediaClock.Controller;
            switch((PlaybackVolume)Settings.ReadEnumSetting<PlaybackVolume>("PlaybackVolume"))
            {
                case PlaybackVolume.Specified:
                    this.mediaVolume = Settings.ReadDoubleSetting("SpecifiedVolume");
                    break;
            }
        }

        private string _mediaTitle;
        public string mediaTitle
        {
            get { return this._mediaTitle; }
            set { this._mediaTitle = value; NotifyPropertyChanged(); }
        }

        public TimeSpan mediaDuration
        {
            get { return this.mediaClock.NaturalDuration.HasTimeSpan ?
                    this.mediaClock.NaturalDuration.TimeSpan : TimeSpan.Zero; }
        }

        public TimeSpan mediaPlaybackTime
        {
            get { return (this.mediaClock.CurrentTime ?? TimeSpan.Zero); }
        }

        private void PlaybackTimerMethod(object sender, EventArgs e)
        {
            NotifyPropertyChanged("mediaPlaybackTime");
            NotifyPropertyChanged("mediaDuration");
        }

        private DispatcherTimer _timer;

        private void SetupTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Tick += new EventHandler(PlaybackTimerMethod);
            _timer.Start();
        }

        private void StopTimer(object sender, CancelEventArgs e)
        {
            _timer.Stop();
        }

        private string _mediaSource = String.Empty;
        public string mediaSource
        {
            get
            {
                return this._mediaSource;
            }
            set
            {
                if (this._mediaSource != value)
                {
                    this.playedMediaCollection.Add(value);
                    this._mediaSource = value;
                    this.mediaTitle = Path.GetFileName(value);
                    OnMediaSourceChanged();
                    NotifyPropertyChanged();
                }
            }
        }

        private bool _mediaEnded = false;
        public bool mediaEnded
        {
            get { return this._mediaEnded; }
            set
            {
                this._mediaEnded = value;
                NotifyPropertyChanged();
                PlayStateChange();
            }
        }

        private bool _mediaFailed = false;
        public bool mediaFailed
        {
            get { return this._mediaFailed; }
            set
            {
                this._mediaFailed = value;
                NotifyPropertyChanged();
                PlayStateChange();
            }
        }

        private bool _playing = false;
        public bool playing
        {
            get { return this._playing; }
            set
            {
                this._playing = value;
                NotifyPropertyChanged();
                if (value)
                    MediaPlayed();
                else
                    MediaStopped();
            }
        }

        private void MediaStopped()
        {
            this._timer.Stop();
        }

        private void MediaPlayed()
        {
            try
            {
                switch ((ResizeMode)Settings.ReadEnumSetting<ResizeMode>(
                    "ResizeModeWhenPlayingVideo"))
                {
                    case ResizeMode.Adjust :
                        double vScaling = Settings.ReadDoubleSetting("AdjustVideoSizeScaling") / 100.0;
                        this.mediaHeight = this.mediaSize.Height * vScaling;
                        this.mediaWidth = this.mediaSize.Width * vScaling;
                        break;
                    case ResizeMode.Screen :
                        Size screenArea = NativeMethods.GetScreenArea();
                        double sScaling = Settings.ReadDoubleSetting("FitToScreenSizeScaling") /100.0;
                        this.mediaHeight = screenArea.Height * sScaling;
                        this.mediaWidth = screenArea.Width * sScaling;
                        break;
                }

                SetupTimer();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public Size mediaSize { get; set; } 

        public double aspectRatio = 16.0 / 9.0;

        public double mediaAspectRatio
        {
            get { return this.aspectRatio; }
            set
            {
                if (0 == value)
                    return;

                this.aspectRatio = value;
                NotifyPropertyChanged();

                if (Settings.ReadBoolSetting("FixAspectRatio", true))
                {
                    this._mediaWidth = this._mediaHeight * value;
                    NotifyPropertyChanged("mediaWidth");
                    this.MediaSizeChanged.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private double _mediaWidth = 853.33333;
        public double mediaWidth
        {
            get { return this._mediaWidth; }
            set
            {
                Debug.WriteLine("Media Width : {0}", value);
                if (value < this.mediaMinWidth)
                    return;

                this._mediaWidth = value;
                NotifyPropertyChanged();
                if (Settings.ReadBoolSetting("FixAspectRatio", true))
                {
                    this._mediaHeight = value / this.aspectRatio;
                    NotifyPropertyChanged("mediaHeight");
                }
                //this.MediaSizeChanged.Invoke(this, EventArgs.Empty);
            }
        }

        private double _mediaHeight = 480.0;
        public double mediaHeight
        {
            get { return this._mediaHeight; }
            set
            {
                Debug.WriteLine("Media Height : {0}", value);
                if (value < this.mediaMinHeight)
                    return;

                this._mediaHeight = value;
                NotifyPropertyChanged();
                if (Settings.ReadBoolSetting("FixAspectRatio", true))
                {
                    this._mediaWidth = value * this.aspectRatio;
                    NotifyPropertyChanged("mediaWidth");
                }
                //this.MediaSizeChanged.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler MediaSizeChanged = (sender, e) => { };
        public event EventHandler MediaVolumeChanged = (sender, e) => { };

        public double changeVolumeStep = 0.05;
        public double changeSpeedStep = 0.1;

        public double mediaMinWidth { get; set; }
        public double mediaMinHeight { get; set; }

        private double _mediaVolume = Settings.ReadDoubleSetting("LastVolume");
        public double mediaVolume
        {
            get { return this._mediaVolume; }
            set
            {
                this._mediaVolume = value;

                if (0 > this._mediaVolume)
                    this._mediaVolume = 0;
                else if (1 < this._mediaVolume)
                    this._mediaVolume = 1;

                NotifyPropertyChanged();
                MediaVolumeChanged.Invoke(this, EventArgs.Empty);
            }
        }

        public void VolumeUp()
        {
            this.mediaVolume += this.changeVolumeStep;
        }

        public void VolumeDown()
        {
            this.mediaVolume -= this.changeVolumeStep;
        }

        public void SpeedUp()
        {
            this.mediaSpeed += this.changeSpeedStep;
        }

        public void SpeedDown()
        {
            this.mediaSpeed -= this.changeSpeedStep;
        }

        private double _mediaSpeed = 1.0;
        public double mediaSpeed
        {
            get
            {
                return this._mediaSpeed;
            }
            set
            {
                this._mediaSpeed = Math.Round(value, 1, MidpointRounding.AwayFromZero);
                if (0.1 > this._mediaSpeed)
                    this._mediaSpeed = 0.1;

                try
                {
                    this.mediaController.SpeedRatio = this._mediaSpeed;
                    Debug.WriteLine("Media playback speed ratio : {0:0.0}", this._mediaSpeed);
                    NotifyPropertyChanged();
                }
                catch
                { }
            }
        }

        public MediaHelper()
        {
            double height;
            if (Double.TryParse(ConfigurationManager.AppSettings["DefaultWindowWidth"], out height))
                this._mediaHeight = height;

            double width;
            if (Double.TryParse(ConfigurationManager.AppSettings["DefaultWindowHeight"], out width))
                this._mediaWidth = width;

            var argFilePath = ((App)Application.Current).argFilePath;
            if (argFilePath == String.Empty)
            {
                SetMediaSourceFromNextFile();
                this.playing = true;
            }
            else
            {
                this.mediaSource = argFilePath;
            }
        }

        public void SetMedia(string path)
        {
            this.playing = false;
            this._mediaFailed = false;
            this._mediaEnded = false;
            this.mediaSource = path;
            this.playing = true;
        }

        private void PlayStateChange()
        {
            if (this.mediaFailed)
            {
                Debug.WriteLine("Media Failed");

                this.playing = false;
                this._mediaFailed = false;
                this._mediaEnded = false;
                SetMediaSourceFromNextFile();
                this.playing = true;
            }
            else if (this.mediaEnded)
            {
                Debug.WriteLine("Media Ended");

                this.playing = false;
                this._mediaFailed = false;
                this._mediaEnded = false;

                switch ((PlaybackBehavior)Settings.ReadEnumSetting<PlaybackBehavior>(
                    "PlaybackEndBehavior"))
                {
                    case PlaybackBehavior.Stop:
                        this.mediaController.Stop();
                        break;
                    case PlaybackBehavior.SingleRepeat:

                        this.mediaController.Stop();
                        this.playing = true;
                        this.mediaController.Begin();
                        break;
                    case PlaybackBehavior.NextMedia:

                        SetMediaSourceFromNextFile();
                        this.playing = true;
                        break;
                    case PlaybackBehavior.NLoop:

                        string sourcePath = GetNextFile();

                        int counter = 0;
                        foreach (string media in this.playedMediaCollection)
                        {
                            if (media == sourcePath)
                            {
                                counter++;
                            }
                        }

                        Debug.WriteLine("NLoop : {0} / {1}", counter,
                            Settings.ReadIntSetting("PlaybackBehaviorNLoopN", 1));

                        if (counter >= Settings.ReadIntSetting("PlaybackBehaviorNLoopN", 1))
                        {
                            Debug.WriteLine("NLoop Stop");
                            this.playedMediaCollection.Clear();
                            this.mediaController.Stop();
                        }
                        else
                        {
                            this.mediaSource = sourcePath;
                            this.playing = true;
                        }
                        break;
                }
            }
        }

        private StringCollection playedMediaCollection = new StringCollection();

        public void SetMediaSourceFromNextFile(bool reverse = false)
        {
            string sourcePath = GetNextFile(reverse);
            if (String.Empty != sourcePath)
            {
                this.mediaSource = sourcePath;
            }
        }

        private string GetNextFile(bool reverse = false)
        {
            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());

            IEnumerable<FileInfo> iEnumFiles;

            if (false == reverse)
            {
                iEnumFiles = di.EnumerateFiles().OrderBy<FileInfo, String>(FileInfo => FileInfo.Name);
            }
            else
            {
                iEnumFiles = di.EnumerateFiles().OrderByDescending<FileInfo, String>(FileInfo => FileInfo.Name);
            }

            string currentSource = String.Empty;
            int count = 0;
            try
            {
                currentSource = this.mediaSource;
                count = iEnumFiles.Count();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (1 > count)
                return String.Empty;

            Debug.WriteLine("Current directory items count ; {0}", count);

            string[] excludeExtension = { ".exe", ".dll", ".ini", ".json", ".config", ".pdb" };

            int index = 0;
            for (int i = count - 1; i >= 0; i--)
            {
                if (currentSource == iEnumFiles.ElementAt(i).FullName)
                {
                    if (0 != index)
                    {
                        break;
                    }
                }
                else if (!excludeExtension.Contains(iEnumFiles.ElementAt(i).Extension.ToLower()))
                {
                    index = i;
                }
            }

            string sourcePath = String.Empty;
            try
            {
                sourcePath = iEnumFiles.ElementAt(index).FullName;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return sourcePath;
        }
    }
}
