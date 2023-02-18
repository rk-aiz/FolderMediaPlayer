using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Diagnostics;

namespace FolderMediaPlayer
{
    internal class MediaSlider : Slider
    {
        public double MediaDuration
        {
            get { return (double)GetValue(MediaDurationProperty); }
            set { SetValue(MediaDurationProperty, value); }
        }

        public static readonly DependencyProperty MediaDurationProperty =
            DependencyProperty.Register("MediaDuration", typeof(TimeSpan), typeof(MediaSlider),
                                        new PropertyMetadata(TimeSpan.Zero,
                                            new PropertyChangedCallback(MediaDuration_PropertyChanged)));

        public TimeSpan MediaPlaybackTime
        {
            get { return (TimeSpan)GetValue(MediaPlaybackTimeProperty); }
            set { SetValue(MediaPlaybackTimeProperty, value); }
        }

        public static readonly DependencyProperty MediaPlaybackTimeProperty =
            DependencyProperty.Register("MediaPlaybackTime", typeof(TimeSpan), typeof(MediaSlider),
                                        new PropertyMetadata(TimeSpan.Zero,
                                            new PropertyChangedCallback(MediaPlaybackTime_PropertyChanged)));

        private static void MediaDuration_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaSlider source = (MediaSlider)d;
            source.Maximum = ((TimeSpan)e.NewValue).TotalSeconds;
        }

        private static void MediaPlaybackTime_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaSlider source = (MediaSlider)d;
            if (!source.sliding)
                source.Value = ((TimeSpan)e.NewValue).TotalSeconds;
            //source.SetCurrentValue(MediaSlider.ValueProperty, ((TimeSpan)e.NewValue).TotalSeconds);
        }

        private bool sliding = false;

        protected override void OnGotMouseCapture(MouseEventArgs e)
        {
            this.sliding = true;
            Debug.WriteLine("OnGotMouseCapture");
            base.OnGotMouseCapture(e);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            this.sliding = false;
            Debug.WriteLine("OnGotMouseCapture");
            base.OnLostMouseCapture(e);
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            if (newValue != this.MediaPlaybackTime.TotalSeconds)
                OnMediaSliderJumped();
        }

        public void OnMediaSliderJumped()
        {
            MediaSliderJumped?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler MediaSliderJumped = (sender, e) => { };

        public MediaSlider()
        {
            /*SetBinding(MediaSlider.ValueProperty, new Binding("MediaPlaybackTime"){
                Source=this
                Converter=
            });*/
        }

    }
}
