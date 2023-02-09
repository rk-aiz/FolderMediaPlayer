using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using System.Windows.Input;
using System.Diagnostics;

namespace FolderMediaPlayer
{
    internal class Commands
    {
        static readonly RoutedUICommand _ok = new RoutedUICommand();

        public static RoutedUICommand Ok
        {
            get { return _ok; }
        }

        static readonly RoutedUICommand _cancel = new RoutedUICommand();

        public static RoutedUICommand Cancel
        {
            get { return _cancel; }
        }
    }

    public class ApplyCommand : ICommand
    {
        private Settings _settings;

        public ApplyCommand(Settings settings)
        {
            this._settings = settings;
            this._settings.PropertyChanged += (sender, e) =>
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            };
        }
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            Debug.WriteLine("CanExecute (propertyChangeCounter : {0})",
                this._settings.propertyChangeCounter);
            return 0 < this._settings.propertyChangeCounter;
        }

        public void Execute(object Parameter)
        {
            this._settings.UpdateShortcutKey();
            this._settings.Save();
        }
    }
}
