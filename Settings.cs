using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Data;
using System.Reflection;
using System.Xml.Linq;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace FolderMediaPlayer
{
    public class Settings : INotifyPropertyChanged
    {

        private WindowMode _startupWindowMode = WindowMode.Default;
        public WindowMode StartupWindowMode
        {
            get { return this._startupWindowMode; }
            set { this._startupWindowMode = value; NotifyPropertyChanged(); }
        }
        private ResizeMode _playbackResizeMode = ResizeMode.Nothing;
        public ResizeMode PlaybackResizeMode
        {
            get { return this._playbackResizeMode; }
            set { this._playbackResizeMode = value; NotifyPropertyChanged(); }
        }

        private PlaybackBehavier _playbackEndBehavier = PlaybackBehavier.NextMedia;
        public PlaybackBehavier PlaybackEndBehavier
        {
            get { return this._playbackEndBehavier; }
            set { this._playbackEndBehavier = value; NotifyPropertyChanged(); }
        }

        private PlaybackVolume _playbackVolume = PlaybackVolume.NoChange;
        public PlaybackVolume PlaybackVolume
        {
            get { return this._playbackVolume; }
            set { this._playbackVolume = value; NotifyPropertyChanged(); }
        }

        private double _lastVolume = 0.3;
        public double LastVolume
        {
            get { return this._lastVolume; }
            set { this._lastVolume = value; NotifyPropertyChanged(); }
        }

        private double _specifiedVolume = 0.3;
        public double SpecifiedVolume
        {
            get { return this._specifiedVolume; }
            set { this._specifiedVolume = value; NotifyPropertyChanged(); }
        }

        private int _playbackBehavierNLoopN = 1;
        public int PlaybackBehavierNLoopN
        {
            get { return this._playbackBehavierNLoopN; }
            set { this._playbackBehavierNLoopN = value; NotifyPropertyChanged(); }
        }

        private double _lastWindowWidth = 1280;
        public double lastWindowWidth
        {
            get { return this._lastWindowWidth; }
            set { this._lastWindowWidth = value; NotifyPropertyChanged(); }
        }

        private double _lastWindowHeight = 720;
        public double lastWindowHeight
        {
            get { return this._lastWindowHeight; }
            set { this._lastWindowHeight = value; NotifyPropertyChanged(); }
        }

        private double _specifiedWindowWidth = 1280;
        public double specifiedWindowWidth
        {
            get { return this._specifiedWindowWidth; }
            set { this._specifiedWindowWidth = value; NotifyPropertyChanged(); }
        }

        private double _specifiedWindowHeight = 720;
        public double specifiedWindowHeight {
            get { return this._specifiedWindowHeight; }
            set { this._specifiedWindowHeight = value; NotifyPropertyChanged(); }
        }

        private double _adjustVideoSizeScaling = 100;
        public double AdjustVideoSizeScaling
        {
            get { return this._adjustVideoSizeScaling; }
            set { this._adjustVideoSizeScaling = value; NotifyPropertyChanged(); }
        }

        private double _fitToScreenSizeScaling = 100;
        public double FitToScreenSizeScaling
        {
            get { return _fitToScreenSizeScaling; }
            set { this._fitToScreenSizeScaling = value; NotifyPropertyChanged(); }
        }

        private bool _restoreWindowPosition = true;
        public bool RestoreWindowPosition
        {
            get { return this._restoreWindowPosition; }
            set { this._restoreWindowPosition = value; NotifyPropertyChanged(); }
        }

        private double _lastWindowLeft = 480;
        public double lastWindowLeft
        {
            get { return this._lastWindowLeft; }
            set { this._lastWindowLeft = value; NotifyPropertyChanged(); }
        }

        private double _lastWindowTop = 480;
        public double lastWindowTop
        {
            get { return this._lastWindowTop; }
            set { this._lastWindowTop = value; NotifyPropertyChanged(); }
        }

        private bool _fixAspectRatio = true;
        public bool FixAspectRatio
        {
            get { return this._fixAspectRatio; }
            set { this._fixAspectRatio = value; NotifyPropertyChanged(); }
        }

        private bool _onScreenVolumeDisplay = false;
        public bool OnScreenVolumeDisplay
        {
            get { return this._onScreenVolumeDisplay; }
            set { this._onScreenVolumeDisplay = value; NotifyPropertyChanged(); }
        }

        private bool _onScreenPlaybackTimeDisplay = false;
        public bool OnScreenPlaybackTimeDisplay
        {
            get { return this._onScreenPlaybackTimeDisplay; }
            set { this._onScreenPlaybackTimeDisplay = value; NotifyPropertyChanged(); }
        }

        private bool _onScreenTitleDisplay = false;
        public bool OnScreenTitleDisplay
        {
            get { return this._onScreenTitleDisplay; }
            set { this._onScreenTitleDisplay = value; NotifyPropertyChanged(); }
        }

        private bool _onScreenSpeedDisplay = false;
        public bool OnScreenSpeedDisplay
        {
            get { return this._onScreenSpeedDisplay; }
            set { this._onScreenSpeedDisplay = value; NotifyPropertyChanged(); }
        }

        private MouseBehavier _mouseWheelBehavier = MouseBehavier.Volume;
        public MouseBehavier MouseWheelBehavier
        {
            get { return this._mouseWheelBehavier; }
            set { this._mouseWheelBehavier = value; NotifyPropertyChanged(); }
        }

        private MouseCursorMode _mouseCursor = MouseCursorMode.AutoHide;
        public MouseCursorMode MouseCursor
        {
            get { return this._mouseCursor; }
            set { this._mouseCursor = value; NotifyPropertyChanged(); }
        }

        public bool propertySaved = false;
        public int propertyChangeCounter{ get; set; } = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            Debug.WriteLine("NotifyPropertyChanged [{0}] : {1}",
                propertyName, this.GetType().GetProperty(propertyName).GetValue(this));

            this.propertyChangeCounter++;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ApplyCommand _applyCommand;
        public ApplyCommand ApplyCommand
        {
            get { return _applyCommand ?? 
                    (this._applyCommand = new ApplyCommand(this)); }
        }

        public ObservableCollection<ShortcutKeyEntry> ShortcutKeyCollection
        { get; set; }

        public void SetShortcutKeyEntry(ShortcutKeyEntry entry)
        {
            for (int i = 0; i < this.ShortcutKeyCollection.Count; i++)
            {
                if (this.ShortcutKeyCollection[i].index == entry.index)
                {
                    this.ShortcutKeyCollection[i] = entry;
                    NotifyPropertyChanged("propertyChangeCounter");
                    //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShortcutKeyCollection"));
                }
            }
        }

        public void ResetShortcutKeyEntry(int index)
        {
            for (int i = 0; i < this.ShortcutKeyCollection.Count; i++)
            {
                if (this.ShortcutKeyCollection[i].index == index)
                {
                    foreach (ShortcutKeyEntry entry in ShortcutKey.GetDefaultXml())
                    {
                        if (entry.index == index)
                        {
                            this.ShortcutKeyCollection[i] = entry;
                            NotifyPropertyChanged("propertyChangeCounter");
                        }
                    }
                    //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShortcutKeyCollection"));
                }
            }
        }

        public void LoadShortcutKey()
        {
            this.ShortcutKeyCollection = new ObservableCollection<ShortcutKeyEntry>(
                ShortcutKey.EntryList);

            BindingOperations.EnableCollectionSynchronization(
                this.ShortcutKeyCollection, new object());

            var cv = CollectionViewSource.GetDefaultView(this.ShortcutKeyCollection);
            cv.SortDescriptions.Clear();
            cv.SortDescriptions.Add(
                new SortDescription("index", ListSortDirection.Ascending));
        }

        public void Load()
        {
            ShortcutKey.ReadXml();

            this._startupWindowMode = (WindowMode)ReadEnumSetting<WindowMode>(
                "StartupWindowMode");
            this._playbackResizeMode = (ResizeMode)ReadEnumSetting<ResizeMode>(
                "ResizeModeWhenPlayingVideo");
            this._playbackEndBehavier = (PlaybackBehavier)ReadEnumSetting<PlaybackBehavier>(
                "PlaybackEndBehavier");
            this._mouseWheelBehavier = (MouseBehavier)ReadEnumSetting<MouseBehavier>(
                "MouseWheelBehavier");
            this._playbackVolume = (PlaybackVolume)ReadEnumSetting<PlaybackVolume>(
                "PlaybackVolume");
            
            this.MouseCursor = (MouseCursorMode)ReadEnumSetting<MouseCursorMode>(
                "MouseCursor");

            this._lastVolume = ReadDoubleSetting("LastVolume");
            this._specifiedVolume = ReadDoubleSetting("SpecifiedVolume");
            this._lastWindowWidth = ReadDoubleSetting("LastWindowWidth");
            this._lastWindowHeight = ReadDoubleSetting("LastWindowHeight");
            this._lastWindowLeft = ReadDoubleSetting("LastWindowLeft");
            this._lastWindowTop = ReadDoubleSetting("LastWindowTop");
            this._specifiedWindowWidth = ReadDoubleSetting("SpecifiedWindowWidth");
            this._specifiedWindowHeight = ReadDoubleSetting("SpecifiedWindowHeight");
            this._adjustVideoSizeScaling = ReadDoubleSetting("AdjustVideoSizeScaling");
            this._fitToScreenSizeScaling = ReadDoubleSetting("FitToScreenSizeScaling");
            this._restoreWindowPosition = ReadBoolSetting("RestoreWindowPosition", true);
            this._fixAspectRatio = ReadBoolSetting("FixAspectRatio", true);
            this._onScreenVolumeDisplay = ReadBoolSetting("OnScreenVolumeDisplay", false);
            this._onScreenSpeedDisplay = ReadBoolSetting("OnScreenSpeedDisplay", false);
            this._onScreenPlaybackTimeDisplay = ReadBoolSetting("OnScreenPlaybackTimeDisplay", false);
            this._playbackBehavierNLoopN = ReadIntSetting("PlaybackBehavierNLoopN", 1);
            this._onScreenTitleDisplay = ReadBoolSetting("OnScreenTitleDisplay", false);

            this.propertyChangeCounter = 0;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("propertyChangeCounter"));
        }

        public static bool ReadBoolSetting(string propertyName, bool defaultValue = false)
        {
            bool _;
            try
            {
                if (false == Boolean.TryParse(
                    Properties.Settings.Default[propertyName].ToString(),
                    out _))
                {
                    _ = defaultValue;
                }
            }
            catch
            {
                _ = defaultValue;
            }
            return _;
        }

        public static object ReadEnumSetting<TEnum>(string propertyName)
        {
            object _;
            try
            {
                if(false == Enum.TryParse(
                    typeof(TEnum),
                    Properties.Settings.Default[propertyName].ToString(),
                    out _))
                {
                    _ = default(TEnum);
                }
            }
            catch
            {
                _ = default(TEnum);
            }
            return _;
        }
        public static double ReadDoubleSetting(string propertyName, double defaultValue = 0)
        {
            double _;
            try
            {
                if(false == Double.TryParse(
                    Properties.Settings.Default[propertyName].ToString(),
                    out _))
                {
                    _ = defaultValue;
                }
            }
            catch
            {
                _ = defaultValue;
            }
            return _;
        }

        public static int ReadIntSetting(string propertyName, int defaultValue = 0)
        {
            int _;
            try
            {
                if (false == Int32.TryParse(
                    Properties.Settings.Default[propertyName].ToString(),
                    out _))
                {
                    _ = defaultValue;
                }
            }
            catch
            {
                _ = defaultValue;
            }
            return _;
        }

        private void SetSetting(string propertyName, object value)
        {
            try
            {
                Properties.Settings.Default[propertyName] = value.ToString();
            }
            catch { }
        }

        public void Save()
        {
            SetSetting("StartupWindowMode", this.StartupWindowMode);
            SetSetting("ResizeModeWhenPlayingVideo", this.PlaybackResizeMode);
            SetSetting("MouseWheelBehavier", this.MouseWheelBehavier);
            SetSetting("LastWindowWidth", this.lastWindowWidth);
            SetSetting("LastWindowHeight", this.lastWindowHeight);
            SetSetting("LastWindowLeft", this.lastWindowLeft);
            SetSetting("LastWindowTop", this.lastWindowTop);
            SetSetting("SpecifiedWindowWidth", this.specifiedWindowWidth);
            SetSetting("SpecifiedWindowHeight", this.specifiedWindowHeight);
            SetSetting("AdjustVideoSizeScaling", this.AdjustVideoSizeScaling);
            SetSetting("FitToScreenSizeScaling", this.FitToScreenSizeScaling);
            SetSetting("RestoreWindowPosition", this.RestoreWindowPosition);
            SetSetting("FixAspectRatio", this.FixAspectRatio);
            SetSetting("OnScreenPlaybackTimeDisplay", this.OnScreenPlaybackTimeDisplay);
            SetSetting("OnScreenVolumeDisplay", this.OnScreenVolumeDisplay);
            SetSetting("OnScreenSpeedDisplay", this.OnScreenSpeedDisplay);
            SetSetting("PlaybackBehavierNLoopN", this.PlaybackBehavierNLoopN);
            SetSetting("PlaybackEndBehavier", this.PlaybackEndBehavier);
            SetSetting("OnScreenTitleDisplay", this.OnScreenTitleDisplay);
            SetSetting("PlaybackVolume", this.PlaybackVolume);
            SetSetting("LastVolume", this.LastVolume);
            SetSetting("SpecifiedVolume", this.SpecifiedVolume);
            SetSetting("MouseCursor", this.MouseCursor);

            Properties.Settings.Default.Save();

            ShortcutKey.WriteXML();
            
            this.propertyChangeCounter = 0;
            this.propertySaved = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("propertyChangeCounter"));
        }

        public void UpdateShortcutKey()
        {
            ShortcutKey.EntryList = new List<ShortcutKeyEntry>(this.ShortcutKeyCollection);
        }

        public Settings()
        {
        }
    }
}

public enum WindowMode
{
    Default,
    Memorized,
    Specified,
}

public enum ResizeMode
{
    Nothing,
    Adjust,
    Screen,
}

public enum PlaybackBehavier
{
    Stop,
    SingleRepeat,
    NextMedia,
    NLoop,
}

public enum MouseBehavier
{
    Volume,
    Speed,
    JumpSmall,
    JumpMedium,
    JumpLarge,
}

public enum PlaybackVolume
{
    NoChange,
    Specified
}

public enum MouseCursorMode
{
    Visible,
    Hidden,
    AutoHide,
}