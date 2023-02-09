using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
using System.Windows.Media.Animation;

namespace FolderMediaPlayer
{

    public partial class CustomMediaElement : MediaElement
    {
        public Size MediaSize
        {
            get { return (Size)GetValue(MediaSizeProperty); }
            set { SetValue(MediaSizeProperty, value); }
        }

        public static readonly DependencyProperty MediaSizeProperty =
            DependencyProperty.Register("MediaSize", typeof(Size), typeof(MediaElement),
                                        new PropertyMetadata(Size.Empty));

        public bool IsMediaEnded
        {
            get { return (bool)GetValue(IsMediaEndedProperty); }
            set { SetValue(IsMediaEndedProperty, value); }
        }

        public static readonly DependencyProperty IsMediaEndedProperty =
            DependencyProperty.Register("IsMediaEnded", typeof(bool), typeof(MediaElement),
                                        new PropertyMetadata(false));

        public bool IsMediaFailed
        {
            get { return (bool)GetValue(IsMediaFailedProperty); }
            set { SetValue(IsMediaFailedProperty, value); }
        }

        public static readonly DependencyProperty IsMediaFailedProperty =
            DependencyProperty.Register("IsMediaFailed", typeof(bool), typeof(MediaElement),
                                        new PropertyMetadata(false));

        public bool IsPlaying
        {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }

        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register("IsPlaying", typeof(bool), typeof(MediaElement),
                                        new PropertyMetadata(false, new PropertyChangedCallback(IsPlaying_PropertyChanged)));

        public double AspectRatio
        {
            get { return (double)GetValue(AspectRatioProperty); }
            set { SetValue(AspectRatioProperty, value); }
        }

        public static readonly DependencyProperty AspectRatioProperty =
            DependencyProperty.Register("AspectRatio", typeof(double), typeof(MediaElement),
                                        new PropertyMetadata(16.0 / 9.0, new PropertyChangedCallback(AspectRatio_PropertyChanged)));

        public ClockController MediaController
        {
            get { return (ClockController)GetValue(MediaControllerProperty); }
            set { SetValue(MediaControllerProperty, value); }
        }

        public static readonly DependencyProperty MediaControllerProperty =
            DependencyProperty.Register("MediaController", typeof(ClockController), typeof(MediaElement),
                                        new PropertyMetadata(null, new PropertyChangedCallback(ClockController_PropertyChanged)));

        private static void ClockController_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaElement source = (MediaElement)d;
            source.Clock = (MediaClock)((ClockController)e.NewValue).Clock;
        }

        private static void IsPlaying_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaElement source = (MediaElement)d;
            Debug.WriteLine("Natural Media Height {0}", source.NaturalVideoHeight);
            Debug.WriteLine("Natural Media Width {0}", source.NaturalVideoWidth);
            /*CustomMediaElement source = (CustomMediaElement)d;
            Debug.WriteLine("IsPlaying_PropertyChanged");
            if (true == (bool)e.NewValue && (bool)e.OldValue != (bool)e.NewValue)
            {
                Debug.WriteLine("IsPlaying_PropertyChanged => Play");
                source.Play();
            }
            else if (false == (bool)e.NewValue && (bool)e.OldValue != (bool)e.NewValue)
            {
                Debug.WriteLine("IsPlaying_PropertyChanged => Stop");
                source.Stop();
            }*/
        }
        private static void AspectRatio_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine("AspectRatio_PropertyChanged : {0}", (double)e.NewValue);
        }

        /*protected override void OnRender(DrawingContext drawingContext)
        {
            Play();
            base.OnRender(drawingContext);
        }*/

        public void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("MediaElement_MediaOpened Event");
            //SetValue(IsPlayingProperty, true);
            this.IsMediaEnded = false;
            this.IsMediaFailed = false;
            if (0 != this.NaturalVideoHeight)
            {
                Debug.WriteLine("Natural video size : {0}, {1}", this.NaturalVideoWidth, this.NaturalVideoHeight);
                this.MediaSize = new Size((double)this.NaturalVideoWidth, (double)this.NaturalVideoHeight);
                this.AspectRatio = (double)this.NaturalVideoWidth / (double)this.NaturalVideoHeight;
            }
        }

        public async void MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Debug.WriteLine("MediaElement_MediaFailed Event");

            TimeSpan p = this.Position;

            await Task.Delay(1000);

            Debug.WriteLine("MediaElement_MediaFailed Event, Position : {0}", this.Position.ToString());

            if (false == this.IsPlaying || (p == this.Position && true == this.IsPlaying))
            {
                SetValue(IsPlayingProperty, false);
                this.IsMediaFailed = true;
            }
        }

        public void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("MediaElement_MediaEnded Event");
            //Close();
            SetValue(IsPlayingProperty, false);
            this.IsMediaEnded = true;
        }

        public CustomMediaElement()
        {
            this.MediaOpened += new RoutedEventHandler(MediaElement_MediaOpened);
            this.MediaEnded += new RoutedEventHandler(MediaElement_MediaEnded);
            this.MediaFailed += new EventHandler<ExceptionRoutedEventArgs>(MediaElement_MediaFailed);
        }
    }

}
