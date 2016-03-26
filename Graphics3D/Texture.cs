using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using SharpDX;

namespace Graphics3D
{
    public class Texture
    {
        private int width;
        private int height;
        private byte[] internalBuffer;

        public Texture(string filename)
        {
            Load(filename);
        }

        private void Load(string filename)
        {
            using (var stream = File.OpenRead(filename))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = stream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                image.Freeze();

                WriteableBitmap bitmap = new WriteableBitmap(image);
                width = bitmap.PixelWidth;
                height = bitmap.PixelHeight;
                internalBuffer = new byte[width * height * 4];
                Marshal.Copy(bitmap.BackBuffer, internalBuffer, 0, internalBuffer.Length);
            }
        }

        public Color Map(float fractionU, float fractionV)
        {
            if (internalBuffer == null)
            {
                return Color.White;
            }

            //Repeat by modulo
            int u = Math.Abs((int)(fractionU * width) % width);
            int v = Math.Abs((int)(fractionV * height) % height);

            int pos = (u + v * width) * 4;
            byte b = internalBuffer[pos];
            byte g = internalBuffer[pos + 1];
            byte r = internalBuffer[pos + 2];
            byte a = internalBuffer[pos + 3];

            return new Color(r, g, b, a);
        }
    }
}
