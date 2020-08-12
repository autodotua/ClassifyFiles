using ClassifyFiles.Data;
using ClassifyFiles.UI.Converter;
using ClassifyFiles.UI.Dialog;
using ClassifyFiles.UI.Event;
using ClassifyFiles.UI.Model;
using ClassifyFiles.UI.Util;
using ClassifyFiles.Util;
using FzLib.Extension;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ClassifyFiles.UI.Page
{
    public partial class ClassSettingPanel : ProjectPageBase
    {
        public ClassSettingPanel()
        {
            InitializeComponent();
            SmoothScrollViewerHelper.Regist(scrMathConditions);
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
            await MainWindow.Current.DoProcessAsync(Do());
            async Task Do()
            {
                await classes.AddAsync();
            }
        }

        private async void DeleteClassButton_Click(object sender, RoutedEventArgs e)
        {
            await MainWindow.Current.DoProcessAsync(Do());
            async Task Do()
            {
                await classes.DeleteSelectedAsync();
                flyDelete.Hide();
            }
        }

        private void AddMatchConditionButton_Click(object sender, RoutedEventArgs e)
        {
            MatchConditions.Add(new MatchCondition() { Index = MatchConditions.Count });
        }

        private async void SelectedUIClassesChanged(object sender, SelectedClassChangedEventArgs e)
        {
            if (needToSelectedClass != null && classes.UIClasses.Contains(needToSelectedClass))
            {
                UIClass c = needToSelectedClass;
                needToSelectedClass = null;
                await Task.Delay(1);
                classes.SelectedUIClass = c;
                return;
            }
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            new DisplayNameHelpDialog() { Owner = Window.GetWindow(this) }.ShowDialog();
        }

        private UIClass needToSelectedClass = null;

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //分组修改后，会取消选择，需要手动重新选择
            needToSelectedClass = classes.SelectedUIClass;
        }
    }
}