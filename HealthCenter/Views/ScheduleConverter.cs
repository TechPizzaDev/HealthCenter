using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;
using NodaTime;

namespace HealthCenter.Views
{
    public sealed class ScheduleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Instant instant)
            {
                return instant.ToDateTimeUtc().ToLocalTime().ToString();
            }
            if (value is OffsetTime time)
            {
                return time.TimeOfDay.ToString();
            }
            if (value is BitArray bits && bits.Length == 5)
            {
                if (bits[0]) return "Monday";
                if (bits[1]) return "Tuesday";
                if (bits[2]) return "Wednesday";
                if (bits[3]) return "Thursday";
                if (bits[4]) return "Friday";
            }
            return value.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
