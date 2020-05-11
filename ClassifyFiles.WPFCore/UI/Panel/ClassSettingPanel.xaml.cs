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

namespace ClassifyFiles.UI.Panel
{
    public partial class ClassSettingPanel : ProjectPanelBase
    {
        public ClassSettingPanel()
        {
            InitializeComponent();
        }

        public override ClassesPanel GetClassesPanel()
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
            MatchConditions.Add(new MatchCondition() { Index = MatchConditions.Count });
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MatchConditions.Remove((sender as FrameworkElement).Tag as MatchCondition);
            for (int i = 0; i < MatchConditions.Count; i++)
            {
                MatchConditions[i].Index = i;
            }
        }



        private async void classes_SelectedClassChanged(object sender, SelectedItemChanged<Class> e)
        {
            var old = e.OldValue;
            if (old != null)
            {
                old.MatchConditions.Clear();
                old.MatchConditions.AddRange(MatchConditions);
                await DbUtility.SaveClassAsync(old);
            }
            MatchConditions = new ObservableCollection<MatchCondition>(classes.SelectedClass.MatchConditions);
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
