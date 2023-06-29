using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ZPrinterConfig.Converters
{
    internal class CSVStringToArray : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            string val = (string)value;
            
            var spl = val.Split(',');

            List<string> values = new List<string>();
            foreach(var s in spl)
                values.Add( s.Trim());

            return values;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}
