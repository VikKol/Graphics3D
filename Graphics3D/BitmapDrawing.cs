using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Graphics3D
{
    public class BitmapDrawing : Drawing
    {
        private readonly WriteableBitmap bitmap;
        private readonly Int32Rect bmpRect;

        public BitmapDrawing(WriteableBitmap bmp)
            : base(new byte[bmp.PixelWidth * bmp.PixelHeight * 4], bmp.PixelWidth, bmp.PixelHeight)
        {
            bitmap = bmp;
            bmpRect = new Int32Rect(0, 0, Width, Height);
        }

        public IDisposable UsingDrawingContext()
        {
            bitmap.Lock();

            return Disposable.Create(() =>
            {
                Marshal.Copy(PixelBuffer, 0, bitmap.BackBuffer, PixelBuffer.Length);
                bitmap.AddDirtyRect(bmpRect);

                bitmap.Unlock();
            });
        }
    }
}
