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

namespace ClassifyFiles.UI
{
    public partial class LogWindow : WindowBase
    {
        public LogWindow()
        {
            InitializeComponent();
            grdBar.Children.OfType<DatePicker>().ForEach(p => 
            p.BlackoutDates.Add(new CalendarDateRange(DateTime.Today.AddDays(1), DateTime.MaxValue)));
        }

        private DateTime beginTime = DateTime.Today;
        public DateTime BeginTime
        {
            get => beginTime;
            set
            {
                if(value>EndTime)
                {
                    value = EndTime;
                }

                beginTime = value;
                this.Notify();
            }
        }
        private DateTime endTime = DateTime.Today;
        public DateTime EndTime
        {
            get => endTime;
            set
            {
                if (value<BeginTime)
                {
                    value = BeginTime;
                }
                endTime = value;
                this.Notify();
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
        }
    }
    public class PastDateValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!DateTime.TryParse((value ?? "").ToString(),
                CultureInfo.CurrentCulture,
                DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces,
                out DateTime time)) return new ValidationResult(false, "");

            return time.Date > DateTime.Now.Date
                ? new ValidationResult(false, "")
                : ValidationResult.ValidResult;
        }
    }
}
