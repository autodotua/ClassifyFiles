using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;

namespace ClassifyFiles.UI.Util
{
    public class EnumToItemsSource : MarkupExtension
    {
        private Type Type { get; }

        public EnumToItemsSource(Type type)
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
