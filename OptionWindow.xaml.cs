﻿using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Reflection;
using System.Xml.Linq;

namespace FolderMediaPlayer
{
    /// <summary>
    /// OptionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class OptionWindow : Window
    {
        public Settings settings = new Settings();

        public SettingHelper helper { get; set; } = new SettingHelper();
        public OptionWindow()
        {
            this.settings.Load();
            this.settings.LoadShortcutKey();
            this.DataContext = this.settings;

            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Debug.WriteLine("Option window closing result : {0}",
                this.settings.propertySaved);
            this.DialogResult = this.settings.propertySaved;
            base.OnClosing(e);
        }
        
        private void OnApply(object sender, ExecutedRoutedEventArgs e)
        {
            this.settings.UpdateShortcutKey();
            this.settings.Save();

            if ((string)e.Parameter == "OK")
                Close();
        }

        private void OnCancel(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void KeyboardInputBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //Debug.WriteLine(e.Key | e.SystemKey);

            switch (e.Key)
            {
                case Key.None:
                case Key.LeftCtrl:
                case Key.RightCtrl:
                case Key.LeftShift:
                case Key.RightShift:
                case Key.LeftAlt:
                case Key.Apps:
                case Key.System:
                    return;
            }
            
            this.helper.key = (e.Key | e.SystemKey);

            e.Handled = true;
        }

        private void KeyboardShortcutDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("SelectionChanged item count {0}", e.AddedItems.Count);

            if (e.AddedItems.Count == 0) { return; }

            if (e.AddedItems[0] is ShortcutKeyEntry)
            {
                var entry = ((ShortcutKeyEntry)e.AddedItems[0]);

                this.helper.key = entry.key;
                this.helper.command = entry.name;
                this.helper.methodName = entry.methodName;
                this.helper.description = entry.description;
                this.helper.modCtrl =
                    (ModifierKeys.Control & entry.modifiers) == ModifierKeys.Control;
                this.helper.modShift =
                    (ModifierKeys.Shift & entry.modifiers) == ModifierKeys.Shift;
                this.helper.modAlt =
                    (ModifierKeys.Alt & entry.modifiers) == ModifierKeys.Alt;
                this.helper.index = entry.index;

            }
        }

        private void ShortcutKeyChangeButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("ModifierKeys {0}", (this.helper.modCtrl ? ModifierKeys.Control : ModifierKeys.None) |
                         (this.helper.modAlt ? ModifierKeys.Alt : ModifierKeys.None) |
                         (this.helper.modShift ? ModifierKeys.Shift : ModifierKeys.None));

            var entry = new ShortcutKeyEntry
            {
                index = this.helper.index,
                name = this.helper.command,
                methodName = this.helper.methodName,
                targetName = ShortcutCommand.TargetName(this.helper.methodName),
                key = this.helper.key,
                modifiers =
                    (this.helper.modCtrl ? ModifierKeys.Control : ModifierKeys.None) |
                    (this.helper.modAlt ? ModifierKeys.Alt : ModifierKeys.None) |
                    (this.helper.modShift ? ModifierKeys.Shift : ModifierKeys.None),
                description = this.helper.description
            };

            this.settings.SetShortcutKeyEntry(entry);
        }

        private void ShortcutKeyResetButton_Click(object sender, RoutedEventArgs e)
        {
            this.settings.ResetShortcutKeyEntry(this.helper.index);
        }
    }
}