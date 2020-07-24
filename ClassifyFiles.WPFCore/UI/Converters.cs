using ClassifyFiles.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ClassifyFiles.UI
{
    public class FilterLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value as string;
            if (str == "")
            {
                return "筛选";
            }
            else if (str.Length <= 5)
            {
                return $"筛选（{str}）";
            }
            else
            {
                return $"筛选（{str.Substring(0, 4)}…）";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class MagnificationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value * (double)parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class NotFirst2VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }  
    public class Equal2BoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }
    public class Bool2VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
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

    public class IsNotNull2VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }

    public class TimeSpan2MsConverter : IValueConverter
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
    //public sealed class LogTypeConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        string type = value as string;
    //        return Strings.Get(type);
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotSupportedException();
    //    }
    //}
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
    public class MatchConditionType2ControlVisibilityConverter : IValueConverter
    {
        private static Dictionary<MatchType, string> MatchConditionTypeWithControlType = new Dictionary<MatchType, string>()
        {
            [MatchType.InFileName] = "text",
            [MatchType.InDirName] = "text",
            [MatchType.InDirNameWithRegex] = "text",
            [MatchType.InFileNameWithRegex] = "text",
            [MatchType.InPath] = "text",
            [MatchType.WithExtension] = "text",
            [MatchType.InPathWithRegex] = "text",
            [MatchType.SizeLargerThan] = "text",
            [MatchType.SizeSmallerThan] = "text",
            [MatchType.TimeEarlierThan] = "time",
            [MatchType.TimeLaterThan] = "time",

        };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (MatchConditionTypeWithControlType[(MatchType)value] == parameter as string)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumToItemsSourceExtension : MarkupExtension
    {
        private Type Type { get; }

        public EnumToItemsSourceExtension(Type type)
        {
            Type = type;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return System.Enum.GetValues(Type).Cast<object>()
                .Select(e =>
                {
                    var enumItem = e.GetType().GetMember(e.ToString()).First();
                    var desc = (enumItem.GetCustomAttributes(false).First() as DescriptionAttribute).Description;
                    return new { Value = e, DisplayName = desc };
                });
        }
    }
}
