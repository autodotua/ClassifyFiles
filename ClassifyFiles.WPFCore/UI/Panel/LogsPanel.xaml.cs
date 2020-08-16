using ClassifyFiles.Data;
using ClassifyFiles.Util;
using FzLib.Extension;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace ClassifyFiles.UI.Panel
{
    /// <summary>
    /// LogsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LogsPanel : UserControlBase
    {
        public LogsPanel()
        {
            DateBegin = DateTime.Today.AddDays(-5);
            DateEnd = DateTime.Today.AddDays(1);
            InitializeComponent();
        }

        private IList<Log> logs;

        public IList<Log> Logs
        {
            get => logs;
            set
            {
                logs = value;
                this.Notify(nameof(Logs));
            }
        }

        public DateTime DateBegin { get; set; }
        public DateTime DateEnd { get; set; }

        private async void OkButton_Click(object sender, RoutedEventArgs e)
        {
            List<Log> logs = null;
            await Task.Run(() => logs = LogUtility.GetLogs(DateBegin, DateEnd));
            Logs = logs;
        }
    }
}