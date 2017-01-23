using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Pelco.Helpers.Converters
{
    public class UIntArrayToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            return string.Join(",", ((uint[])value).Select(x => x.ToString()).ToArray());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var list = ((string)value).Split(',').Select(str => { uint num; uint.TryParse(str, out num); return num; });
            var array = list.Where(val => val != 0).ToArray();
            return array;
        }
    }
}
