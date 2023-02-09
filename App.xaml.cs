using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Globalization;
using System.Diagnostics;

namespace FolderMediaPlayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            string language =
               ConfigurationManager.AppSettings["DefaultCulture"] ??
               CultureInfo.CurrentUICulture.Name;

            try
            {
                var dictionary = new ResourceDictionary
                {
                    Source = new Uri("/Locales/" + language + ".xaml", UriKind.Relative)
                };
                this.Resources.MergedDictionaries[0] = dictionary;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        // Process arguments.
        public string argFilePath = String.Empty;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            foreach (string arg in e.Args)
            {
                if (Directory.Exists(arg))
                {
                    Directory.SetCurrentDirectory(arg);
                }
                else if (File.Exists(arg))
                {
                    this.argFilePath = arg;
                }
            }
            if (String.Empty != this.argFilePath)
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(this.argFilePath));
            }
        }
    }
}
