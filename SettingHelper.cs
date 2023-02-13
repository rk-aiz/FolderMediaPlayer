using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using System.Windows.Interop;
using System.Diagnostics;
using System.Xml.Linq;
using System.ComponentModel.DataAnnotations;

namespace FolderMediaPlayer
{
    public class SettingHelper : INotifyPropertyChanged, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Key _key;
        public Key key
        {
            get { return this._key; }
            set
            {
                this._key = value;
                OnPropertyChanged();
            }
        }

        private int _index;
        public int index
        {
            get { return this._index; }
            set { this._index = value; OnPropertyChanged(); }
        }

        private string _command;
        public string command
        {
            get { return this._command; }
            set { this._command = value; OnPropertyChanged(); }
        }

        private string _methodName;
        public string methodName
        {
            get { return this._methodName; }
            set { this._methodName = value; OnPropertyChanged(); }
        }

        private string _description;
        public string description
        {
            get { return this._description; }
            set { this._description = value; OnPropertyChanged(); }
        }

        private bool _modCtrl;
        public bool modCtrl
        {
            get { return this._modCtrl; }
            set { this._modCtrl = value; OnPropertyChanged(); }
        }

        private bool _modAlt;
        public bool modAlt
        {
            get { return this._modAlt; }
            set { this._modAlt = value; OnPropertyChanged(); }
        }

        private bool _modShift;
        public bool modShift
        {
            get { return this._modShift; }
            set { this._modShift = value; OnPropertyChanged(); }
        }

        public string Error
        {
            get
            {
                return null;
            }
        }

        public string this[string columnName]
        {
            get
            {
                return null;
            }
        }

        private void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(
                this, new PropertyChangedEventArgs(propertyName));
        }

        public SettingHelper()
        {
        }
    }
}
