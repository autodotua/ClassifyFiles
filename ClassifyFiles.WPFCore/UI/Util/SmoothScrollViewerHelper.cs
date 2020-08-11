using ClassifyFiles.UI.Component;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClassifyFiles.UI.Util
{
    /// <summary>
    /// 平滑滚动的帮助类
    /// </summary>
    /// <remarks>
    /// 本来自然是想到用DoubleAnimation的，但是发现Offset并不能动画。那就干脆手动一下，来个循环。
    /// 为了能够让速度叠加，加了一个remainsDelta。没想到，这一加，直接把阻尼感都写出来了
    /// 可把我牛逼坏了
    /// </remarks>
    public static class SmoothScrollViewerHelper
    {
        static SmoothScrollViewerHelper()
        {
        }

        private static Dictionary<ScrollViewer, double> remainsDeltas = new Dictionary<ScrollViewer, double>();
        public static void Regist(Control ctrl, bool horizontal = false)
        {
            ScrollViewer scr = ctrl.GetVisualChild<ScrollViewer>();
            if (scr == null)
            {
                throw new Exception("Control内没有ScrollViewer");
            }
            Regist(scr);
        }
        public static void Regist(ScrollViewer scr, bool horizontal = false)
        {
            scr.PreviewMouseWheel +=  (p1, p2) =>
             {
                 p2.Handled = true;
                 HandleMouseWheel(p1 as ScrollViewer, p2.Delta, horizontal);
             };
        }


        public static async void HandleMouseWheel(ScrollViewer scr, int delta, bool horizontal = false)
        {
            Debug.Assert(scr != null);
            Debug.WriteLineIf(DebugSwitch.ScorllInfo, "Scroll Happened");

            //如果当前的滚动视图还没有进行过滚动，那么进行注册
            if (!remainsDeltas.ContainsKey(scr))
            {
                remainsDeltas.Add(scr, 0);
            }

            remainsDeltas[scr] = remainsDeltas[scr]*1.5 + delta;//乘一个系数，那么滚轮越快页面滑动也将越快
            if (remainsDeltas[scr] != delta)
            {
                //如果滚动正在进行，那么把滚动交给之前的方法即可
                return;
            }
            while (remainsDeltas[scr] != 0)
            {
                if (horizontal)
                {
                    scr.ScrollToHorizontalOffset(scr.HorizontalOffset
                        - (remainsDeltas[scr] > 0 ? 1 : -1) * Math.Sqrt(Math.Abs(remainsDeltas[scr])) / 1.5d //这个控制滑动的距离，值越大距离越短
                        * System.Windows.Forms.SystemInformation.MouseWheelScrollLines);
                }
                else
                {
                    scr.ScrollToVerticalOffset(scr.VerticalOffset 
                        - (remainsDeltas[scr] > 0 ? 1 : -1) * Math.Sqrt(Math.Abs(remainsDeltas[scr])) / 1.5d //这个控制滑动的距离，值越大距离越短
                        * System.Windows.Forms.SystemInformation.MouseWheelScrollLines);
                }
                remainsDeltas[scr] /= 1.5;//这个控制每一次滑动的时间，值越大时间越短
                await Task.Delay(5);
                //如果到目标距离不到10了，就直接停止滚动，因为不然的话会永远滚下去
                if (Math.Abs(remainsDeltas[scr]) < 1)
                {
                    remainsDeltas[scr] = 0;
                }
            }
            Debug.WriteLineIf(DebugSwitch.ScorllInfo, "Scroll End\r\n");

        }
    }
}
