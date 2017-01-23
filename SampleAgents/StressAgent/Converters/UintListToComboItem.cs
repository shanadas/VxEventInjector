using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;

namespace StressAgent.Converters
{
    class UintListToComboItem : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var uintList = new List<uint>();
            var comboVal = value as ComboBoxItem;
            if (comboVal != null)
            {
                var strVal = comboVal.Content as string;
                if (!string.IsNullOrWhiteSpace(strVal))
                {
                    uint tempUint = 0;
                    foreach (var uintVal in strVal.Split(','))
                    {
                        if (uint.TryParse(uintVal, out tempUint))
                            uintList.Add(tempUint);
                    }
                }
            }
            return uintList;
        }
    }
}
