using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Diagnostics;

namespace FolderMediaPlayer
{
    public class CultureAwareBinding : Binding
    {
        public CultureAwareBinding()
        {
            ConverterCulture = CultureInfo.CurrentUICulture;
        }
    }

    [ValueConversion(typeof(Enum), typeof(bool))]
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;
            
            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return Binding.DoNothing;
            if ((bool)value)
            {
                return Enum.Parse(targetType, parameter.ToString());
            }
            return Binding.DoNothing;
        }
    }

    [ValueConversion(typeof(Double), typeof(String))]
    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            if (parameter == null)
                return value.ToString();
            else
                return String.Format(parameter.ToString(), (Double)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Binding.DoNothing;

            double backValue;
            if (Double.TryParse((String)value, out backValue))
                return backValue;

            return Binding.DoNothing;
        }
    }

    [ValueConversion(typeof(Int32), typeof(String))]
    public class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Binding.DoNothing;

            int backValue;
            if (Int32.TryParse((String)value, out backValue))
                return backValue;

            return Binding.DoNothing;
        }
    }

    [ValueConversion(typeof(Double), typeof(Double))]
    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Double.NaN;

            return Math.Round((double)value * 100, 2, MidpointRounding.AwayFromZero);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Binding.DoNothing;

            double res;
            if (Double.TryParse(value.ToString(), out res))
            {
                return res / 100;
            }
            else
                return Binding.DoNothing;
        }
    }

    public class PlaybackTimeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
                return Binding.DoNothing;

            return ((TimeSpan)values[0]).ToString(@"h\:mm\:ss") + " / " +
                ((TimeSpan)values[1]).ToString(@"h\:mm\:ss");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return (object[])Binding.DoNothing;
        }
    }

    [ValueConversion(typeof(Key), typeof(String))]
    public class InputKeyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return String.Empty;

            return ((Key)value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || (string)value == String.Empty)
                return Binding.DoNothing;

            object key;
            if (false == Enum.TryParse(
                typeof(Key), (string)value, out key))
            {
                return Binding.DoNothing;
            }

            return (Key)key;
        }
    }

    [ValueConversion(typeof(Enum), typeof(String))]
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return String.Empty;

            object ignoreValue;
            if (Enum.TryParse(
                value.GetType(), parameter.ToString(), out ignoreValue))
            {
                if (ignoreValue.Equals(value))
                {
                    return String.Empty;
                }
            }

            return ((Enum)value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || (string)value == String.Empty)
                return Binding.DoNothing;

            object backValue;
            if (false == Enum.TryParse(
                targetType, (string)value, out backValue))
            {
                return Binding.DoNothing;
            }

            object ignoreValue;
            if (Enum.TryParse(
                targetType, parameter.ToString(), out ignoreValue))
            {
                if (ignoreValue.Equals(backValue))
                {
                    return Binding.DoNothing;
                }   
            }

            return (Enum)backValue;
        }
    }

    [ValueConversion(typeof(String), typeof(String))]
    public class TranslateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.ToString() == String.Empty)
                return Binding.DoNothing;

            Debug.WriteLine("Translater culture : {0}", culture.Name);

            string translatedValue = String.Empty;
            try
            {
                translatedValue = Properties.Resources.ResourceManager.GetString(value.ToString(), culture);
            }
            catch
            { }
            
            if (translatedValue != String.Empty)
            {
                return translatedValue;
            }
            else
            {
                return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    [ValueConversion(typeof(MouseCursorMode), typeof(Cursor))]
    public class CursorVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.ToString() == String.Empty)
                return Binding.DoNothing;

            Debug.WriteLine("Mouse Cursor {0}", (MouseCursorMode)value);

            switch ((MouseCursorMode)value)
            {
                case MouseCursorMode.Visible :
                    return Cursors.Arrow;
                case MouseCursorMode.Hidden :
                    return Cursors.None;
                case MouseCursorMode.AutoHide:
                    return Cursors.Arrow;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
