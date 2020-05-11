using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClassifyFiles.UI
{

    public class NotFirst2VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value>0?Visibility.Visible:Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }      public class Bool2VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value)?Visibility.Visible:Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Visibility)value) == Visibility.Visible;
        }

    }   
    public class IsNotNull2BoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }

    public class TimeSpan2Ms : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int interval = (int)value;
            return DateTime.Today + TimeSpan.FromMilliseconds(interval);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime? interval = value as DateTime?;
            if (interval.HasValue)
            {
                int ms = (int)(interval.Value - DateTime.Today).TotalMilliseconds;
                if (ms == 0)
                {
                    ms = 1000 * 60;
                }
                return ms;
            }
            return 1000 * 600;

        }
    }
 

    public sealed class MethodToValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(parameter is string methodName))
            {
                return null;
            }
            var methodInfo = value.GetType().GetMethod(methodName, new Type[0]);
            if (methodInfo == null)
            {
                return null;
            }
            return methodInfo.Invoke(value, new object[0]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    public sealed class LogTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string type = value as string;
            return Strings.Get(type);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    public class ValueMinusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value - double.Parse(parameter as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
