using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace FolderMediaPlayer
{
    class AppData
    {
        public static string AppGuid
        {
            get
            {
                return "{6e072a3a-d92e-458b-ab21-fa5a44253a56}";
            }
        }

        public static string KeyboardShortcutXml
        {
            get
            {
                return RoamingDataFolder + "//KeyboardShortcut.xml";
            }
        }

        public static string RoamingDataFolder
        {
            get
            {
                string folderBase = Environment.GetFolderPath
                                    (Environment.SpecialFolder.ApplicationData);

                var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

                return CheckDir(string.Format(@"{0}\{1}\{2}", folderBase, assemblyName, AppGuid));
            }
        }

        private static string CheckDir(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }
    }
}
