using FzLib.Basic;
using FzLib.Extension;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ClassifyFiles.Data;
using ClassifyFiles.Util;
using ClassifyFiles.UI;
using System.Windows.Markup;
using System.Diagnostics;

namespace ClassifyFiles.UI.Panel
{
    public partial class ClassSettingPanel : ProjectPanelBase
    {
        public ClassSettingPanel()
        {
            InitializeComponent();
        }

        public override ListPanelBase GetItemsPanel()
        {
            return classes;
        }
        public ObservableCollection<MatchCondition> matchConditions;

        public ObservableCollection<MatchCondition> MatchConditions
        {
            get => matchConditions;
            set
            {
                matchConditions = value;
                this.Notify(nameof(MatchConditions));
            }
        }
        private async void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MatchConditions.Remove((sender as FrameworkElement).Tag as MatchCondition);
            for (int i = 0; i < MatchConditions.Count; i++)
            {
                MatchConditions[i].Index = i;
            }
        }



        public Task SaveClassAsync()
        {

            return SaveClassAsync(classes.SelectedItem as Class);

        }
        public async Task SaveClassAsync(Class c)
        {
            if (c != null)
            {
                foreach (var m in c.MatchConditions)
                {
                    if (m.Value == null)
                    {
                        Debug.Assert(false);
                        m.Value = "";
                    }
                }
                c.MatchConditions.Clear();
                c.MatchConditions.AddRange(MatchConditions);
                await DbUtility.SaveClassAsync(c);
            }
        }

        private async void AddClassInButton_Click(object sender, RoutedEventArgs e)
        {
            await classes.AddClass();
        }

        private async void DeleteClassButton_Click(object sender, RoutedEventArgs e)
        {
            await classes.DeleteClass();
        }

        private async void RenameClassButton_Click(object sender, RoutedEventArgs e)
        {
            await classes.RenameButton();
        }

        private void AddMatchConditionButton_Click(object sender, RoutedEventArgs e)
        {
            MatchConditions.Add(new MatchCondition() { Index = MatchConditions.Count });

        }



        private async void classes_SelectedItemChanged_1(object sender, SelectedItemChanged e)
        {
            var old = e.OldValue as Class;
            if (old != null)
            {
                await SaveClassAsync(old);
            }
            if (classes.SelectedItem == null)
            {
                MatchConditions = new ObservableCollection<MatchCondition>();
            }
            else
                MatchConditions = new ObservableCollection<MatchCondition>
                    ((classes.SelectedItem as Class).MatchConditions.OrderBy(p => p.Index));

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

    public class EnumToItemsSource : MarkupExtension
    {
        private Type Type { get; }

        public EnumToItemsSource(Type type)
        {
            Type = type;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enum.GetValues(Type).Cast<object>()
                .Select(e =>
                {
                    var enumItem = e.GetType().GetMember(e.ToString()).First();
                    var desc = (enumItem.GetCustomAttributes(false).First() as DescriptionAttribute).Description;
                    return new { Value = e, DisplayName = desc };
                });
        }
    }
}
