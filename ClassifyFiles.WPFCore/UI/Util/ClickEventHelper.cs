using System;
using System.Windows;
using System.Windows.Input;

namespace ClassifyFiles.UI.Util
{

    /// <summary>
    /// 为没有Click事件的控件添加Click事件
    /// </summary>
    public class ClickEventHelper
    {
        private DateTime downTime;
        public Point downPosition;
        private FrameworkElement downSender;

        public ClickEventHelper(FrameworkElement element)
        {
            element.PreviewMouseDown += MouseDown;
            element.PreviewMouseUp += MouseUp;
        }

        private void MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                downPosition = e.GetPosition(null);
                downSender = sender as FrameworkElement;
                downTime = DateTime.Now;
            }
        }

        private void MouseUp(object sender, MouseButtonEventArgs e)
        {
            var newPosition = e.GetPosition(null);
            double distance = Math.Sqrt(Math.Pow(newPosition.X - downPosition.X, 2) + Math.Pow(newPosition.Y - downPosition.Y, 2));
            if (distance < 5 &&
                e.LeftButton == MouseButtonState.Released &&
                sender == downSender)
            {
                TimeSpan timeSinceDown = DateTime.Now - this.downTime;
                if (timeSinceDown.TotalMilliseconds < 500)
                {
                    Click?.Invoke(downSender, new EventArgs());
                }
            }
        }

        public event EventHandler Click;
    }
}
