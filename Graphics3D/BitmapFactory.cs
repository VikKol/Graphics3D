using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Graphics3D
{
    public class BitmapFactory
    {
        const double DPI = 96.0;

        public static WriteableBitmap Create(int width, int height) =>
            new WriteableBitmap(width, height, DPI, DPI, PixelFormats.Pbgra32, null);
    }
}
