using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Controls;
using System.Diagnostics;
using System.ComponentModel;

namespace FolderMediaPlayer
{
    class SizeValueRule : ValidationRule
    {
        private const ValidationMessage MESSAGE = ValidationMessage.NumericOnly;
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double res;
            if (Double.TryParse(value.ToString(), out res) && 0 < res)
                return ValidationResult.ValidResult;
            else
                return new ValidationResult(false, MESSAGE);
        }
    }

    class ScalingValueRule : ValidationRule
    {
        private const ValidationMessage MESSAGE = ValidationMessage.ScalingValueOnly;
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double res;
            if (Double.TryParse(value.ToString(), out res) && 1 <= res && res <= 1000.0)
            {
                return ValidationResult.ValidResult;
            }
            else
                return new ValidationResult(false, MESSAGE);
        }
    }

    class VolumeValueRule : ValidationRule
    {
        private const ValidationMessage MESSAGE = ValidationMessage.VolumeRuleMessage;
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double res;
            if (Double.TryParse(value.ToString(), out res) && 0 <= res && res <= 100.0)
            {
                return ValidationResult.ValidResult;
            }
            else
                return new ValidationResult(false, MESSAGE);
        }
    }

    class PositiveIntegerRule : ValidationRule
    {
        private const ValidationMessage MESSAGE = ValidationMessage.PositiveIntegerOnly;
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int res;
            if (Int32.TryParse(value.ToString(), out res) && 1 <= res)
            {
                return ValidationResult.ValidResult;
            }
            else
                return new ValidationResult(false, MESSAGE);
        }
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ValidationMessage
    {
        [LocalizedDescription("NumericOnly",
        typeof(FolderMediaPlayer.Properties.EnumResources))]
        NumericOnly,
        [LocalizedDescription("ScalingValueOnly",
        typeof(FolderMediaPlayer.Properties.EnumResources))]
        ScalingValueOnly,
        [LocalizedDescription("VolumeRuleMessage",
        typeof(FolderMediaPlayer.Properties.EnumResources))]
        VolumeRuleMessage,
        [LocalizedDescription("PositiveIntegerOnly",
            typeof(FolderMediaPlayer.Properties.EnumResources))]
        PositiveIntegerOnly,
    }

}
