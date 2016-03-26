using System;
using SharpDX;

namespace Graphics3D
{
    internal class Drawing
    {
        private readonly int width;
        private readonly int height;
        private readonly byte[] pixelBuffer;

        public Drawing(byte[] bgraArray, int width, int height)
        {
            this.pixelBuffer = bgraArray;
            this.width = width;
            this.height = height;
        }

        private void DrawLine(Vector3 point0, Vector3 point1, Color color)
        {
            if ((point1 - point0).Length() < 2)
                return;

            var middle = point0 + ((point1 - point0) / 2);

            PutPixel((int)middle.X, (int)middle.Y, color);
            DrawLine(point0, middle, color);
            DrawLine(middle, point1, color);
        }

        public void DrawBline(int x0, int y0, int x1, int y1, Color color)
        {
            var dx = Math.Abs(x1 - x0);
            var dy = Math.Abs(y1 - y0);
            var sx = (x0 < x1) ? 1 : -1;
            var sy = (y0 < y1) ? 1 : -1;
            var err = dx - dy;

            while (true)
            {
                PutPixel(x0, y0, color);

                if ((x0 == x1) && (y0 == y1)) break;
                var e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }

        public void PutPixel(int x, int y, Color color)
        {
            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                var index4 = (x + (y * width)) * 4;
                pixelBuffer[index4] = color.B;
                pixelBuffer[index4 + 1] = color.G;
                pixelBuffer[index4 + 2] = color.R;
                pixelBuffer[index4 + 3] = color.A;
            }
        }
    }
}
