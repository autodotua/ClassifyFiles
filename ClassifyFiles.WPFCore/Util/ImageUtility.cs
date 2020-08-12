using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClassifyFiles.Util
{
    public static class ImageUtility
    {
        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            using var memory = new MemoryStream();
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        public static ImageSource CreateScreenshotOfFrameworkElement(FrameworkElement ele)
        {
            RenderTargetBitmap renderTargetBitmap =
      new RenderTargetBitmap((int)ele.ActualWidth,
      (int)ele.ActualHeight,
      96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(ele);
            PngBitmapEncoder pngImage = new PngBitmapEncoder();
            pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            using MemoryStream ms = new MemoryStream();
            pngImage.Save(ms);
            var imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.StreamSource = ms;
            imageSource.EndInit();
            return imageSource;
        }
    }
}