using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace StressAgent.Converters
{
    class UintToComboItem : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            uint uintVal = 0;
            var comboVal = value as ComboBoxItem;
            if (comboVal != null)
            {
                var strVal = comboVal.Content as string;
                if (!string.IsNullOrWhiteSpace(strVal))
                    uint.TryParse(strVal, out uintVal);
            }
            return uintVal;
        }
    }
}
