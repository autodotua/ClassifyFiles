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
using ClassifyFiles.UI.Model;
using ClassifyFiles.UI.Util;
using ClassifyFiles.UI.Converter;
using ClassifyFiles.UI.Dialog;

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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //不知道为什么，Xaml里绑定不上，只好在代码里绑定了
            btnDelete.SetBinding(IsEnabledProperty, new Binding(nameof(classes.SelectedUIClass))
            {
                ElementName = nameof(classes),
                Converter = new IsNotNull2BoolConverter()
            });
            //btnRename.SetBinding(IsEnabledProperty, new Binding(nameof(classes.SelectedUIClass))
            //{
            //    ElementName = nameof(classes),
            //    Converter = new IsNotNull2BoolConverter()
            //});
            btnAddMatchCondition.SetBinding(IsEnabledProperty, new Binding(nameof(classes.SelectedUIClass))
            {
                ElementName = nameof(classes),
                Converter = new IsNotNull2BoolConverter()
            });
            //但是最后一个无论如何都还是绑定不上
            //txtName.SetBinding(TextBox.TextProperty,
            //    new Binding($"{nameof(classes.SelectedUIClass)}.{nameof(UIClass.Class)}.{nameof(Class.Name)}")
            //    {
            //        ElementName = nameof(classes),
            //        Mode = BindingMode.TwoWay
            //    });
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

        private void ApplyMatchConditions(Class c)
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
            }
        public async Task SaveClassesAsync()
        {
            await Task.Run(() =>
            {
                if (classes.SelectedUIClass != null)
                {
                    ApplyMatchConditions(classes.SelectedUIClass.Class);
                }
                var cs = classes.UIClasses.ToArray();
                for (int i = 0; i < cs.Length; i++)
                {
                    cs[i].Class.Index = i;
                }
                ClassUtility.SaveClasses(cs.Select(p => p.Class));
            });
        }

        private async void AddClassInButton_Click(object sender, RoutedEventArgs e)
        {
            GetProgress().Show();
            await classes.AddAsync();
            GetProgress().Close();
            //btnRename.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        }

        private async void DeleteClassButton_Click(object sender, RoutedEventArgs e)
        {
            GetProgress().Show();
            await classes.DeleteSelectedAsync();
            flyDelete.Hide();
            GetProgress().Close();
        }

        private void AddMatchConditionButton_Click(object sender, RoutedEventArgs e)
        {
            MatchConditions.Add(new MatchCondition() { Index = MatchConditions.Count });
        }

        private void SelectedUIClassesChanged(object sender, SelectedClassChangedEventArgs e)
        {
            Class old = e.OldValue;
            if (old != null && e.NewValue != null)
            {
                ApplyMatchConditions(old);
            }
            if (classes.SelectedUIClass == null)
            {
                MatchConditions = null;
            }
            else
            {
                MatchConditions = new ObservableCollection<MatchCondition>
                    (classes.SelectedUIClass.Class.MatchConditions.OrderBy(p => p.Index));
            }
        }



        private async void classes_ClassDragDroped(object sender, EventArgs e)
        {
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            new DisplayNameHelpDialog() { Owner = Window.GetWindow(this) }.ShowDialog();
        }
    }


}
