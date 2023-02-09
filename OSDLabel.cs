using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Media.Animation;

namespace FolderMediaPlayer
{
    public class OnScreenVolumeLabel : Label
    {
        public static readonly RoutedEvent ContentChangedEvent =
            EventManager.RegisterRoutedEvent(
                name: "ContentChanged",
                routingStrategy: RoutingStrategy.Bubble,
                handlerType: typeof(RoutedEventHandler),
                ownerType: typeof(OnScreenVolumeLabel));

        public string VisibilitySetting { get; set; }
        public bool VisibilityDefault { get; set; }

        public event RoutedEventHandler ContentChanged
        {
            add { AddHandler(ContentChangedEvent, value); }
            remove { RemoveHandler(ContentChangedEvent, value); }
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (Settings.ReadBoolSetting(this.VisibilitySetting, this.VisibilityDefault))
            {   
                RaiseEvent(new RoutedEventArgs(ContentChangedEvent, this));
            }
            base.OnContentChanged(oldContent, newContent);
        }
    }

    public class OnScreenSpeedLabel : Label
    {
        public static readonly RoutedEvent ContentChangedEvent =
            EventManager.RegisterRoutedEvent(
                name: "ContentChanged",
                routingStrategy: RoutingStrategy.Bubble,
                handlerType: typeof(RoutedEventHandler),
                ownerType: typeof(OnScreenSpeedLabel));

        public string VisibilitySetting { get; set; }
        public bool VisibilityDefault { get; set; }

        public event RoutedEventHandler ContentChanged
        {
            add { AddHandler(ContentChangedEvent, value); }
            remove { RemoveHandler(ContentChangedEvent, value); }
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (Settings.ReadBoolSetting(this.VisibilitySetting, this.VisibilityDefault))
            {
                RaiseEvent(new RoutedEventArgs(ContentChangedEvent, this));
            }
            base.OnContentChanged(oldContent, newContent);
        }
    }

    public class OnScreenTitleLabel : Label
    {
        public static readonly RoutedEvent ContentChangedEvent =
            EventManager.RegisterRoutedEvent(
                name: "ContentChanged",
                routingStrategy: RoutingStrategy.Bubble,
                handlerType: typeof(RoutedEventHandler),
                ownerType: typeof(OnScreenTitleLabel));

        public string VisibilitySetting { get; set; }
        public bool VisibilityDefault { get; set; }

        public event RoutedEventHandler ContentChanged
        {
            add { AddHandler(ContentChangedEvent, value); }
            remove { RemoveHandler(ContentChangedEvent, value); }
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (Settings.ReadBoolSetting(this.VisibilitySetting, this.VisibilityDefault))
            {
                RaiseEvent(new RoutedEventArgs(ContentChangedEvent, this));
            }
            base.OnContentChanged(oldContent, newContent);
        }
    }

    public class OnScreenPlaybackTimeLabel : Label
    {
        public TimeSpan Time
        {
            get { return (TimeSpan)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public static readonly DependencyProperty TimeProperty =
        DependencyProperty.Register("Time", typeof(TimeSpan), typeof(Label),
                                new PropertyMetadata(TimeSpan.Zero, new PropertyChangedCallback(OnTimeChanged)));

        public static readonly RoutedEvent ContentChangedEvent = EventManager.RegisterRoutedEvent(
            name: "ContentChanged",
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(OnScreenPlaybackTimeLabel));

        public string VisibilitySetting { get; set; }
        public bool VisibilityDefault { get; set; }

        public TimeSpan tThreathold = new TimeSpan(0, 0, 0, 2, 950);

        public event RoutedEventHandler ContentChanged
        {
            add { AddHandler(ContentChangedEvent, value); }
            remove { RemoveHandler(ContentChangedEvent, value); }
        }

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

            if (Settings.ReadBoolSetting(source.VisibilitySetting, source.VisibilityDefault))
            {
                //Debug.WriteLine("Time changed : {0}", ((TimeSpan)e.NewValue - (TimeSpan)e.OldValue).TotalSeconds);
                //Debug.WriteLine("Time changed : {0}", ((TimeSpan)e.NewValue - (TimeSpan)e.OldValue).Duration() > source.tThreathold);
                if (((TimeSpan)e.NewValue - (TimeSpan)e.OldValue).Duration() > source.tThreathold)
                {
                    source.RaiseEvent(new RoutedEventArgs(ContentChangedEvent, source));
                }
            }
        }
    }
}
