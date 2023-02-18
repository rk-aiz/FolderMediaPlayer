using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Media.Animation;

namespace FolderMediaPlayer
{

    public class CustomLabel : Label
    {
        public bool StaysOpen
        {
            get { return (bool)GetValue(StaysOpenProperty); }
            set { SetValue(StaysOpenProperty, value); }
        }

        public static readonly DependencyProperty StaysOpenProperty =
            DependencyProperty.Register("StaysOpen", typeof(bool), typeof(Label),
                                        new PropertyMetadata(false, new PropertyChangedCallback(StaysOpen_PropertyChanged)));

        public static readonly RoutedEvent ContentChangedEvent =
            EventManager.RegisterRoutedEvent(
                name: "ContentChanged",
                routingStrategy: RoutingStrategy.Bubble,
                handlerType: typeof(RoutedEventHandler),
                ownerType: typeof(CustomLabel));

        public string VisibilitySetting { get; set; }
        public bool VisibilityDefault { get; set; }

        protected static void StaysOpen_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomLabel source = (CustomLabel)d;
            //var sb = (Storyboard)source.FindResource("LabelOpacityAnimation");
            if ((bool)e.NewValue)
            {
                Debug.WriteLine("StaysOpen True");
                //sb.Pause(source);
                source.Opacity = 1.0;
            }
            else
            {
                Debug.WriteLine("StaysOpen False");
                //sb.Resume(source);
                source.Opacity = 0.0;
            }
        }

        public event RoutedEventHandler ContentChanged
        {
            add { AddHandler(ContentChangedEvent, value); }
            remove { RemoveHandler(ContentChangedEvent, value); }
        }
    }

    public class OnScreenVolumeLabel : CustomLabel
    {

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (Settings.ReadBoolSetting(
                this.VisibilitySetting, this.VisibilityDefault))
            {
                RaiseEvent(new RoutedEventArgs(ContentChangedEvent, this));
            }
            base.OnContentChanged(oldContent, newContent);
        }
    }

    public class OnScreenSpeedLabel : CustomLabel
    {
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (Settings.ReadBoolSetting(this.VisibilitySetting, this.VisibilityDefault))
            {
                RaiseEvent(new RoutedEventArgs(ContentChangedEvent, this));
            }
            base.OnContentChanged(oldContent, newContent);
        }
    }

    public class OnScreenTitleLabel : CustomLabel
    {
        new public static readonly RoutedEvent ContentChangedEvent =
            EventManager.RegisterRoutedEvent(
                name: "ContentChanged",
                routingStrategy: RoutingStrategy.Bubble,
                handlerType: typeof(RoutedEventHandler),
                ownerType: typeof(OnScreenTitleLabel));

        new public bool StaysOpen
        {
            get { return (bool)GetValue(StaysOpenProperty); }
            set { SetValue(StaysOpenProperty, value); }
        }

        new public static readonly DependencyProperty StaysOpenProperty =
            DependencyProperty.Register("StaysOpen", typeof(bool), typeof(OnScreenTitleLabel),
                new PropertyMetadata(false, new PropertyChangedCallback(StaysOpen_PropertyChanged)));

        new protected static void StaysOpen_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomLabel source = (CustomLabel)d;
            var sb = (Storyboard)source.FindResource("TitleOpacityAnimation");
            if ((bool)e.NewValue)
            {
                Debug.WriteLine("TitleOpacityAnimation Pause");
                sb.Stop(source);
                source.Opacity = 1.0;
            }
            else
            {
                Debug.WriteLine("TitleOpacityAnimation Resume");
                sb.Begin(source);
                source.Opacity = 0.0;
            }
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (false == this.StaysOpen &&
                Settings.ReadBoolSetting(
                    this.VisibilitySetting, this.VisibilityDefault))
            {
                RaiseEvent(new RoutedEventArgs(ContentChangedEvent, this));
            }
            base.OnContentChanged(oldContent, newContent);
        }
    }

    public class OnScreenPlaybackTimeLabel : CustomLabel
    {
        public TimeSpan Time
        {
            get { return (TimeSpan)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public static readonly DependencyProperty TimeProperty =
        DependencyProperty.Register("Time", typeof(TimeSpan), typeof(Label),
                                new PropertyMetadata(TimeSpan.Zero, new PropertyChangedCallback(OnTimeChanged)));

        public TimeSpan tThreathold = new TimeSpan(0, 0, 0, 2, 950);

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (Settings.ReadBoolSetting(this.VisibilitySetting, this.VisibilityDefault))
            {
                this.Visibility = Visibility.Visible;
            }
            else
            {
                this.Visibility = Visibility.Collapsed;
            }
            base.OnContentChanged(oldContent, newContent);
        }

        protected static void OnTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OnScreenPlaybackTimeLabel source = (OnScreenPlaybackTimeLabel)d;

            if (source.StaysOpen || Settings.ReadBoolSetting(source.VisibilitySetting, source.VisibilityDefault))
            {
                if (((TimeSpan)e.NewValue - (TimeSpan)e.OldValue).Duration() > source.tThreathold)
                {
                    source.RaiseEvent(new RoutedEventArgs(ContentChangedEvent, source));
                }
            }
        }
    }
}
