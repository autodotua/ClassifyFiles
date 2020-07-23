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
using ClassifyFiles.UI.Event;

namespace ClassifyFiles.UI.Page
{
    public partial class ClassSettingPanel : ProjectPageBase
    {
        public ClassSettingPanel()
        {
            InitializeComponent();
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
        private void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void RemoveMatchConditionButton_Click(object sender, RoutedEventArgs e)
        {
            MatchConditions.Remove((sender as FrameworkElement).Tag as MatchCondition);
            for (int i = 0; i < MatchConditions.Count; i++)
            {
                MatchConditions[i].Index = i;
            }
        }

        public override async Task LoadAsync(Project project)
        {
            await base.LoadAsync(project);
            await classes.LoadAsync(project);
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
                await ClassUtility.SaveClassAsync(c);
            }
        }

        private async void AddClassInButton_Click(object sender, RoutedEventArgs e)
        {
            await classes.AddAsync();
        }

        private async void DeleteClassButton_Click(object sender, RoutedEventArgs e)
        {
            await classes.DeleteSelectedAsync();
        }

        private async void RenameClassButton_Click(object sender, RoutedEventArgs e)
        {
            await classes.RenameButton();
        }

        private void AddMatchConditionButton_Click(object sender, RoutedEventArgs e)
        {
            MatchConditions.Add(new MatchCondition() { Index = MatchConditions.Count });
        }



        private async void classes_SelectedItemChanged_1(object sender, SelectedClassChangedEventArgs e)
        {
            var old = e.OldValue as Class;
            if (old != null)
            {
                await SaveClassAsync(old);
            }
            if (classes.SelectedItem == null)
            {
                MatchConditions = null;
            }
            else
                MatchConditions = new ObservableCollection<MatchCondition>
                    (classes.SelectedItem.MatchConditions.OrderBy(p => p.Index));

        }
    }


}
