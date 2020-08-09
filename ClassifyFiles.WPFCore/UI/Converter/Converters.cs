using ClassifyFiles.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ClassifyFiles.UI.Converter
{
    public class Bool2intConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int[] nums = (parameter as string).Split(',', ' ').Select(p => int.Parse(p)).ToArray();
            if((bool)value)
            {
                return nums[0];
            }
            return nums[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Null2ZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return 0;
            }
            Class c = value as Class;
            switch (parameter as string)
            {
                case "1":
                    return c.DisplayProperty1 == null ? 0 : 200;
                case "2":
                    return c.DisplayProperty2 == null ? 0 : 200;
                case "3":
                    return c.DisplayProperty3 == null ? 0 : 200;
            }
            throw new Exception();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class String2EnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Enum.Parse(parameter.GetType(), value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
    }

    /// <summary>
    /// 筛选按钮文字转换器
    /// </summary>
    public class FilterLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value as string;
            if (str.Length == 0)
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

    /// <summary>
    /// 倍率转换器
    /// </summary>
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

    /// <summary>
    /// 如果不是第一个，就可见，否则不可见
    /// </summary>
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


    /// <summary>
    /// 绑定源和转换参数相等则为真
    /// </summary>
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

    /// <summary>
    /// 布尔转可见性
    /// </summary>
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

    /// <summary>
    /// 非空转布尔
    /// </summary>
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

    /// <summary>
    /// 非空转可见性
    /// </summary>
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

    /// <summary>
    /// 时间间隔转毫秒
    /// </summary>
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


    /// <summary>
    /// 绑定值减去参数值
    /// </summary>
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


    /// <summary>
    /// 匹配规则转控件的可见性，控件的转换参数为text或time
    /// </summary>
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
