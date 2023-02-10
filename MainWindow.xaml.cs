using System;
using System.IO;
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
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Windows.Shell;
using System.Collections.Specialized;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Reflection.Metadata;
using System.Configuration;
using System.Windows.Media.Media3D;
using Microsoft.Win32;

namespace FolderMediaPlayer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private WindowChrome windowChrome;

        private MediaHelper mediaHelper = new MediaHelper { mediaMinWidth = 292.0 * 16.0 / 9.0, mediaMinHeight = 292.0 };
        
        private Settings settings = new Settings();

        public Boolean CursorVisibility
        {
            get { return (Boolean)GetValue(CursorVisibilityProperty); }
            set { SetValue(CursorVisibilityProperty, value); }
        }

        public static readonly DependencyProperty CursorVisibilityProperty =
            DependencyProperty.Register("CursorVisibility", typeof(Boolean), typeof(MainWindow),
                                        new PropertyMetadata(true, new PropertyChangedCallback(CursorVisibility_PropertyChanged)));

        public Cursor DefaultCursor
        {
            get { return (Cursor)GetValue(DefaultCursorProperty); }
            set { SetValue(DefaultCursorProperty, value); }
        }

        public static readonly DependencyProperty DefaultCursorProperty =
            DependencyProperty.Register("DafaultCursor", typeof(Cursor), typeof(MainWindow),
                                        new PropertyMetadata(Cursors.Arrow,
                                            new PropertyChangedCallback(DefaultCursor_PropertyChanged)));

        public MouseCursorMode CursorMode
        {
            get { return (MouseCursorMode)GetValue(CursorModeProperty); }
            set { SetValue(CursorModeProperty, value); }
        }

        public static readonly DependencyProperty CursorModeProperty =
            DependencyProperty.Register("CursorMode", typeof(MouseCursorMode), typeof(MainWindow),
                                        new PropertyMetadata(MouseCursorMode.Visible,
                                            new PropertyChangedCallback(CursorMode_PropertyChanged)));

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = mediaHelper;
            this.windowChrome = WindowChrome.GetWindowChrome(this);

            SetBinding(CursorModeProperty, new Binding("MouseCursor")
            {
                Source = this.settings,
                Mode = BindingMode.OneWay,
            });

            if (this.settings.RestoreWindowPosition)
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = this.settings.lastWindowLeft;
                this.Top = this.settings.lastWindowTop;
            }

            this.mediaHelper.MediaSizeChanged += (sender, e) =>
            {
                this.Width = this.mediaHelper.mediaWidth;
                this.Height = this.mediaHelper.mediaHeight;
            };

            this.mediaHelper.MediaVolumeChanged += (sender, e) =>
            {
                this.settings.LastVolume = this.mediaHelper.mediaVolume;
            };
        }

        private void Window_SourceInitialized(object sender, EventArgs ea)
        {
            this.settings.Load();
            this.mediaHelper.mediaVolume = this.settings.LastVolume;
            switch (this.settings.StartupWindowMode)
            {
                case WindowMode.Default:
                    this.Width = this.mediaHelper.mediaWidth;
                    this.Height = this.mediaHelper.mediaHeight;
                    break;
                case WindowMode.Memorized:
                    this.Width = this.mediaHelper.mediaWidth = this.settings.lastWindowWidth;
                    this.Height = this.mediaHelper.mediaHeight = this.settings.lastWindowHeight;

                    break;
                case WindowMode.Specified:
                    this.Width = this.mediaHelper.mediaWidth = this.settings.specifiedWindowWidth;
                    this.Height = this.mediaHelper.mediaHeight = this.settings.specifiedWindowHeight;

                    break;
            }

            HwndSource hwndSource = (HwndSource)HwndSource.FromVisual(this);
            hwndSource.AddHook(WndProc);

            ShortcutKey.RegisterCommandTarget("MainWindow", this);
            ShortcutKey.RegisterCommandTarget("MediaHelper", this.mediaHelper);
            ShortcutKey.StartHook();

            IntPtr hSysMenu = GetSystemMenu(hwndSource.Handle, false);

            MENUITEMINFO mii = new MENUITEMINFO
            {
                cbSize = (uint)Marshal.SizeOf(typeof(MENUITEMINFO)),
                fMask = MIIM.ID | MIIM.STRING,
                fType = (UInt32)MFT.MENUBARBREAK,

                fState = 0,
                wID = (uint)MIIM_ID_OPTION,
                hSubMenu = IntPtr.Zero,
                hbmpChecked = IntPtr.Zero,
                hbmpUnchecked = IntPtr.Zero,
                dwItemData = 0,
                dwTypeData = (string)this.FindResource("stringOption"),
                cch = 0,
                hbmpItem = IntPtr.Zero
            };
            InsertMenuItem(hSysMenu, 0, true, ref mii);

            MENUITEMINFO miOpenDiag = new MENUITEMINFO
            {
                cbSize = (uint)Marshal.SizeOf(typeof(MENUITEMINFO)),
                fMask = MIIM.ID | MIIM.STRING,
                fType = (UInt32)MFT.MENUBARBREAK,
                fState = 0,
                wID = (uint)MIIM_ID_OPENDIALOG,
                hSubMenu = IntPtr.Zero,
                hbmpChecked = IntPtr.Zero,
                hbmpUnchecked = IntPtr.Zero,
                dwItemData = 0,
                dwTypeData = (string)this.FindResource("stringOpenFile"),
                cch = 0,
                hbmpItem = IntPtr.Zero
            };
            InsertMenuItem(hSysMenu, 0, true, ref miOpenDiag);
        }

        private static void CursorMode_PropertyChanged(DependencyObject d,
    DependencyPropertyChangedEventArgs e)
        {
            MainWindow source = (MainWindow)d;
            switch ((MouseCursorMode)e.NewValue)
            {
                case MouseCursorMode.Visible:
                case MouseCursorMode.AutoHide:
                    source.Cursor = source.DefaultCursor;
                    break;
                case MouseCursorMode.Hidden:
                    source.Cursor = Cursors.None;
                    break;
            }
        }

        private static void DefaultCursor_PropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            MainWindow source = (MainWindow)d;
            source.Cursor = (Cursor)e.NewValue;
        }

        private static void CursorVisibility_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainWindow source = (MainWindow)d;

            if (true == (Boolean)e.NewValue && source.CursorMode != MouseCursorMode.Hidden)
            {
                source.Cursor = source.DefaultCursor;
            }
            else if (source.CursorMode != MouseCursorMode.Visible)
            {
                source.Cursor = Cursors.None;
            }
        }

        private bool SysCommandProc(IntPtr id)
        {
            switch ((int)id) // Option Window
            {
                case MIIM_ID_OPTION:
                    OptionWindow optionWindow = new OptionWindow();
                    optionWindow.Owner = this;
                    bool? result = optionWindow.ShowDialog();


                    Debug.WriteLine("ShowDialog result : {0}",
                        result);

                    if (true == result)
                    {
                        Properties.Settings.Default.Reload();
                        this.settings.Load();
                        //this.settings.LoadShortcutKey();
                    }

                    Debug.WriteLine(this.settings.StartupWindowMode);
                    return true;

                case MIIM_ID_OPENDIALOG:
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    if (openFileDialog.ShowDialog() == true)
                        OpenFile(openFileDialog.FileName);
                    return true;
            }
            return false;
        }

        private void SizingProc(IntPtr wParam, IntPtr lParam)
        {
            tagRECT rect = (tagRECT)Marshal.PtrToStructure(lParam, typeof(tagRECT));

            tagRECT scaledRect = rect.Transform(PresentationSource.FromVisual(this).
                CompositionTarget.TransformFromDevice);

            switch ((int)wParam)
            {
                case 1: //WMSZ_LEFT
                    this.mediaHelper.mediaWidth = scaledRect.right - scaledRect.left;
                    scaledRect.top = scaledRect.bottom - Convert.ToInt32(this.mediaHelper.mediaHeight);
                    break;
                case 2: //WMSZ_RIGHT
                    this.mediaHelper.mediaWidth = scaledRect.right - scaledRect.left;
                    scaledRect.bottom = scaledRect.top + Convert.ToInt32(this.mediaHelper.mediaHeight);
                    break;
                case 3: //WMSZ_TOP
                    this.mediaHelper.mediaHeight = scaledRect.bottom - scaledRect.top;
                    scaledRect.right = scaledRect.left + Convert.ToInt32(this.mediaHelper.mediaWidth);
                    break;
                case 4: //WMSZ_TOPLEFT
                    this.mediaHelper.mediaWidth = scaledRect.right - scaledRect.left;
                    scaledRect.top = scaledRect.bottom - Convert.ToInt32(this.mediaHelper.mediaHeight);
                    break;
                case 5: //WMSZ_TOPRIGHT
                case 6: //WMSZ_BOTTOM
                    this.mediaHelper.mediaHeight = scaledRect.bottom - scaledRect.top;
                    scaledRect.right = scaledRect.left + Convert.ToInt32(this.mediaHelper.mediaWidth);
                    break;
                case 7: //WMSZ_BOTTOMLEFT
                    this.mediaHelper.mediaWidth = scaledRect.right - scaledRect.left;
                    scaledRect.bottom = scaledRect.top + Convert.ToInt32(this.mediaHelper.mediaHeight);
                    break;
                case 8: //WMSZ_BOTTOMRIGHT
                    this.mediaHelper.mediaHeight = scaledRect.bottom - scaledRect.top;
                    scaledRect.right = scaledRect.left + Convert.ToInt32(this.mediaHelper.mediaWidth);
                    break;
            }

            if (scaledRect.right - scaledRect.left <= this.mediaHelper.mediaMinWidth)
            {
                scaledRect.left = (int)this.Left;
                scaledRect.right = scaledRect.left + (int)this.mediaHelper.mediaMinWidth;
            }

            if (scaledRect.bottom - scaledRect.top <= this.mediaHelper.mediaMinHeight)
            {
                scaledRect.top = (int)this.Top;
                scaledRect.bottom = scaledRect.top + (int)this.mediaHelper.mediaMinHeight;
            }

            Marshal.StructureToPtr(
                scaledRect.Transform(
                    PresentationSource.FromVisual(this).
                        CompositionTarget.TransformToDevice),
                lParam, true);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            switch (msg)
            {
                case 0x112: // WM_SYSCOMMAND
                    if (SysCommandProc(wParam))
                        handled = true;
                    break;
                case 0x0214: //WM_SIZING
                    SizingProc(wParam, lParam);
                    handled = true;
                    break;
            }
            /*else if (msg == 0x001C)
            {
                this.mediaHelper.TogglePauseAndResume();
            }*/

            return IntPtr.Zero;
        }

        private void ScreenThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            this.Left += e.HorizontalChange;
            this.Top += e.VerticalChange;

            e.Handled = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.settings.lastWindowWidth = this.Width;
            this.settings.lastWindowHeight = this.Height;
            this.settings.lastWindowLeft = this.Left;
            this.settings.lastWindowTop = this.Top;
            this.settings.Save();
            mediaHelper.playing = false;
            base.OnClosing(e);
        }

        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            MouseBehavierProc(this.settings.MouseWheelBehavier, e.Delta > 0);
        }

        private void MouseBehavierProc(MouseBehavier behavier, bool mode)
        {
            switch (behavier)
            {
                case MouseBehavier.Volume:
                    this.mediaHelper.mediaVolume +=
                        this.mediaHelper.changeVolumeStep * (mode ? 1 : -1);
                    break;
                case MouseBehavier.Speed:
                    this.mediaHelper.mediaSpeed +=
                        this.mediaHelper.changeSpeedStep * (mode ? 1 : -1);
                    break;
                case MouseBehavier.JumpLarge:
                    if (mode)
                        this.mediaHelper.JumpBackwardLargeStep();
                    else
                        this.mediaHelper.JumpForwardLargeStep();
                    break;
                case MouseBehavier.JumpMedium:
                    if (mode)
                        this.mediaHelper.JumpBackwardMediumStep();
                    else
                        this.mediaHelper.JumpForwardMediumStep();
                    break;
                case MouseBehavier.JumpSmall:
                    if (mode)
                        this.mediaHelper.JumpForwardSmallStep();
                    else
                        this.mediaHelper.JumpBackwardSmallStep();
                    break;
            }
        }

        private void MainWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ToggleWindowState();
        }

        public void ToggleWindowState()
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        private void OpenFile(string path)
        {
            this.mediaHelper.SetMedia(path);
            Directory.SetCurrentDirectory(Path.GetDirectoryName(path));
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            StringCollection fileList = ((DataObject)e.Data).GetFileDropList();

            if (null != fileList)
            {
                OpenFile(fileList[0]);
            }
        }

        public void ShowSystemMenu()
        {
            Point absP = PointToScreen(Mouse.GetPosition(this));
            IntPtr hWnd = new WindowInteropHelper(this).Handle;
            IntPtr hSysMenu = GetSystemMenu(hWnd, false);

            uint command = TrackPopupMenuEx(hSysMenu, 0x0100, //TPM_LEFTBUTTON = 0x0000; TPM_RETURNCMD = 0x0100;
                (int)absP.X, (int)absP.Y, hWnd, IntPtr.Zero);
        
            if (0 == command)
                return;

            SendMessage(hWnd, 0x0112, // WM_SYSCOMMAND = 0x0112;
                    new IntPtr(command), IntPtr.Zero);
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowSystemMenu();
            e.Handled = true;
        }

        private double RestoreLeft;
        private double RestoreTop;
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState.Maximized == this.WindowState)
            {
                this.RestoreLeft = this.RestoreBounds.Left;
                this.RestoreTop = this.RestoreBounds.Top;
                this.ScreenThumb.IsEnabled = false;
                ClearValue(WindowChrome.WindowChromeProperty);
                this.mediaHelper.mediaHeight = Double.NaN;
                this.Topmost = true;
            }
            else if (WindowState.Normal == this.WindowState)
            {
                if (0 != this.RestoreBounds.Height)
                {
                    Debug.WriteLine("RestoreBounds Left {0} Top {1} Width {2} Height {3}",
                        this.RestoreBounds.Left, this.RestoreBounds.Top,
                        this.RestoreBounds.Width, this.RestoreBounds.Height);
                    this.mediaHelper.mediaHeight = this.RestoreBounds.Height;
                    this.Top = this.RestoreTop;
                    this.Left = this.RestoreLeft;
                }
                this.ScreenThumb.IsEnabled = true;
                SetValue(WindowChrome.WindowChromeProperty, this.windowChrome);
                this.Topmost = false;
            }
        }

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern uint TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool InsertMenuItem(IntPtr hMenu, uint uItem, bool fByPosition, [In] ref MENUITEMINFO lpmii);

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct tagRECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public tagRECT Transform(Matrix matrix)
            {
                return new tagRECT {
                    left = (int)Math.Round((matrix.M11 * left + matrix.M21 * top + matrix.OffsetX),
                        0, MidpointRounding.AwayFromZero),
                    top = (int)Math.Round((matrix.M12 * left + matrix.M22 * top + matrix.OffsetY),
                        0, MidpointRounding.AwayFromZero),
                    right = (int)Math.Round((matrix.M11 * right + matrix.M21 * bottom + matrix.OffsetX),
                        0, MidpointRounding.AwayFromZero),
                    bottom = (int)Math.Round((matrix.M12 * right + matrix.M22 * bottom + matrix.OffsetY),
                        0, MidpointRounding.AwayFromZero),
                };
            }
        }

        [Flags]
        enum MFT : UInt32
        {
            STRING = 0x00000000,
            BITMAP = 0x00000004,
            MENUBARBREAK = 0x00000020,
            MENUBREAK = 0x00000040,
            OWNERDRAW = 0x00000100,
            RADIOCHECK = 0x00000200,
            SEPARATOR = 0x00000800,
            RIGHTORDER = 0x00002000,
            RIGHTJUSTIFY = 0x00004000,
        }

        [Flags]
        enum MIIM : UInt32
        {
            BITMAP = 0x00000080,
            CHECKMARKS = 0x00000008,
            DATA = 0x00000020,
            FTYPE = 0x00000100,
            ID = 0x00000002,
            STATE = 0x00000001,
            STRING = 0x00000040,
            SUBMENU = 0x00000004,
            TYPE = 0x00000010
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MENUITEMINFO
        {
            internal UInt32 cbSize;
            internal MIIM fMask;
            internal UInt32 fType;
            internal UInt32 fState;
            internal UInt32 wID;
            internal IntPtr hSubMenu;
            internal IntPtr hbmpChecked;
            internal IntPtr hbmpUnchecked;
            internal UInt32 dwItemData;
            internal string dwTypeData;
            internal UInt32 cch;
            internal IntPtr hbmpItem;
        };

    private const int MIIM_ID_OPTION = 0x0001;
    private const int MIIM_ID_OPENDIALOG = 0x0002;
    }
}

