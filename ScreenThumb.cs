using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Input;

namespace FolderMediaPlayer
{
    class ScreenThumb : Thumb
    {
        public Boolean CursorVisibility
        {
            get { return (Boolean)GetValue(CursorVisibilityProperty); }
            set { SetValue(CursorVisibilityProperty, value); }
        }

        public static readonly DependencyProperty CursorVisibilityProperty =
            DependencyProperty.Register("CursorVisibility", typeof(Boolean), typeof(ScreenThumb),
                                        new PropertyMetadata(true, new PropertyChangedCallback(CursorVisibility_PropertyChanged)));

        public Cursor DefaultCursor
        {
            get { return (Cursor)GetValue(DefaultCursorProperty); }
            set { SetValue(DefaultCursorProperty, value); }
        }

        public static readonly DependencyProperty DefaultCursorProperty =
            DependencyProperty.Register("DafaultCursor", typeof(Cursor), typeof(ScreenThumb),
                                        new PropertyMetadata(Cursors.Arrow,
                                            new PropertyChangedCallback(DefaultCursor_PropertyChanged)));

        public MouseCursorMode CursorMode
        {
            get { return (MouseCursorMode)GetValue(CursorModeProperty); }
            set { SetValue(CursorModeProperty, value); }
        }

        public static readonly DependencyProperty CursorModeProperty =
            DependencyProperty.Register("CursorMode", typeof(MouseCursorMode), typeof(ScreenThumb),
                                        new PropertyMetadata(MouseCursorMode.Visible));

        private static void DefaultCursor_PropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            ScreenThumb source = (ScreenThumb)d;
            source.Cursor = (Cursor)e.NewValue;
        }

        private static void CursorVisibility_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ScreenThumb source = (ScreenThumb)d;

            if (true == (Boolean)e.NewValue && source.CursorMode != MouseCursorMode.Hidden)
            {
                source.Cursor = source.DefaultCursor;
            }
            else if (source.CursorMode != MouseCursorMode.Visible)
            {
                source.Cursor = Cursors.None;
            }
        }

    }
}
