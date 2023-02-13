using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using System.Configuration;
using System.Linq;
using System.Windows.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FolderMediaPlayer
{
    class ShortcutKey
    {
        private static Dictionary<string, object> commandTargetObjects =
            new Dictionary<string, object>();

        public static void RegisterCommandTarget(string name, object target)
        {
            commandTargetObjects.Add(name, target);
        }

        private static HwndSource hWndSource = HwndSource.FromHwnd(new WindowInteropHelper(Application.Current.MainWindow).Handle);

        public static void StartHook()
        {
            hWndSource.AddHook(new HwndSourceHook(WndProc));
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg,
            IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //Debug.WriteLine(msg.ToString("X"));
            switch (msg)
            {
                case 0x0100: //WM_KEYDOWN
                    Interpreter(KeyInterop.KeyFromVirtualKey(wParam.ToInt32()));
                    handled = true;
                    break;
                /*case 0x0312: //WM_HOTKEY
                    Debug.WriteLine("WM_HOTKEY");
                    Debug.WriteLine(wParam.ToInt32());
                    break;*/
                case 0x0319: //WM_APPCOMMAND
                    var key = GetKeyFromWM_APPCOMMAND_LPARAM(lParam);
                    Debug.WriteLine("WM_APPCOMMAND : {0}", key);
                    Interpreter(key);
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }

        public static List<ShortcutKeyEntry>
            EntryList = new List<ShortcutKeyEntry>();

        private static void Interpreter(Key key)
        {
            if (EntryList == null || EntryList.Count == 0)
                return;

            if (key == Key.None)
                return;

            var modifiers = Keyboard.Modifiers;

            Debug.WriteLine("Key : {0} Mod : {1}", key, modifiers);

            ShortcutKeyEntry entry = GetEntryByKey(key, modifiers);
            try
            {
                object target = commandTargetObjects[entry.targetName];
                target.GetType().InvokeMember(entry.methodName,
                    BindingFlags.InvokeMethod, null, target, null);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public static ShortcutKeyEntry GetEntryByKey(Key key, ModifierKeys modifiers)
        {
            foreach (ShortcutKeyEntry entry in EntryList)
            {
                if (entry.key == Key.None)
                    continue;

                if (entry.key == key && entry.modifiers == modifiers)
                    return entry;
            }

            return null;
        }

        public static void Add(string name, string methodName, string targetName, Key keys = Key.None, ModifierKeys modifiers = ModifierKeys.None, string description = "")
        {
            int lastIndex = 0;
            for (int i = 0; i < EntryList.Count; i++)
            {
                if (lastIndex < EntryList[i].index)
                    lastIndex = EntryList[i].index;

                if (EntryList[i].key == keys && EntryList[i].modifiers == modifiers)
                    EntryList.RemoveAt(i);
            }

            var newEntry = new ShortcutKeyEntry
            {
                index = lastIndex + 1,
                name = name,
                methodName = methodName,
                targetName = targetName,
                key = keys,
                modifiers = modifiers,
                description = description
            };

            ShortcutKey.EntryList.Add(newEntry);
        }

        private static Key GetKeyFromWM_APPCOMMAND_LPARAM(IntPtr lParam)
        {
            int cmd = (int)(lParam.ToInt64() >> 16) & 0xFFF;

            Key key = Key.None;

            switch (cmd)
            {
                case 11: //APPCOMMAND_MEDIA_NEXTTRACK
                    key = Key.MediaNextTrack;
                    break;
                case 12: //APPCOMMAND_MEDIA_PREVIOUSTRACK
                    key = Key.MediaPreviousTrack;
                    break;
                case 13: //APPCOMMAND_MEDIA_STOP
                    key = Key.MediaStop;
                    break;
                case 14: //APPCOMMAND_MEDIA_PLAY_PAUSE
                    key = Key.MediaPlayPause;
                    break;
                case 46: //APPCOMMAND_MEDIA_PLAY
                    key = Key.Play;
                    break;
                case 47: //APPCOMMAND_MEDIA_PAUSE
                    key = Key.Pause;
                    break;
            }

            return key;
        }

        public static void WriteXML()
        {
            XmlSerializer writer =
                new XmlSerializer(typeof(List<ShortcutKeyEntry>));

            FileStream file = File.Create(AppData.KeyboardShortcutXml);

            writer.Serialize(file, EntryList);
            file.Close();
        }

        public static List<ShortcutKeyEntry> GetDefaultXml()
        {
            XmlSerializer reader =
                new XmlSerializer(typeof(List<ShortcutKeyEntry>));

            var info = Application.GetResourceStream(
                new Uri("KeyboardShortcut/default.xml", UriKind.Relative));

            return (List<ShortcutKeyEntry>)reader.Deserialize(info.Stream);
        }

        public static void ReadXml()
        {
            if (File.Exists(AppData.KeyboardShortcutXml))
            {
                using (Stream reader = new FileStream
                    (AppData.KeyboardShortcutXml, FileMode.Open))
                {
                    XmlSerializer serializer =
                        new XmlSerializer(typeof(List<ShortcutKeyEntry>));

                    EntryList =
                        (List<ShortcutKeyEntry>)serializer.Deserialize(reader);
                }
            }
            else
            {
                EntryList = GetDefaultXml();
            }

            CheckDuplicatedIndex();
            //Add("ToggleFullScreen", "ToggleWindowState", "MainWindow", Key.F, ModifierKeys.None, "Switch between full-screen and normal window mode");
        }

        public static void CheckDuplicatedIndex()
        {
            int safeIndex = 0;
            for (int i = 0; i < EntryList.Count; i++)
            {
                if (safeIndex <= EntryList[i].index)
                    safeIndex = EntryList[i].index + 1;
            }

            for (int i = 0; i < EntryList.Count; i++)
            {
                for (int j = i + 1; j < EntryList.Count; j++)
                {
                    if (EntryList[j].index == EntryList[i].index)
                    {
                        EntryList[j].index = safeIndex;
                        safeIndex = safeIndex + 1;
                    }
                }
            }
        }
    }

    public class ShortcutKeyEntry : INotifyPropertyChanged
    {
        public int index { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string methodName { get; set; }
        public string targetName { get; set; }
        public Key key { get; set; }
        public ModifierKeys modifiers { get; set; }
        public ShortcutKeyEntry()
        {
        }

        public void NotifyAssign(string propertyName, object value)
        {
            GetType().GetProperty(propertyName).SetValue(this, value);

            PropertyChanged?.Invoke(
                this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public static class ShortcutCommand
    {
        public static string TargetName(string methodName)
        {
            string targetName = String.Empty;
            switch (methodName)
            {
                case "ToggleWindowState":
                    targetName = "MainWindow";
                    break;
                case "JumpForwardLargeStep":
                case "JumpBackwardLargeStep":
                case "JumpForwardMediumStep":
                case "JumpBackwardMediumStep":
                case "JumpForwardSmallStep":
                case "JumpBackwardSmallStep":
                case "ToggleStopAndPlay": 
                case "TogglePauseAndResum":
                case "NextMedia": 
                case "PreviousMedia":
                case "JumpToBegin": 
                case "JumpToEnd":
                case "VolumeUp":
                case "VolumeDown":
                case "SpeedUp":
                case "SpeedDown":
                    targetName = "MediaHelper";
                    break;
            }

            return targetName;
        }

    }


}
