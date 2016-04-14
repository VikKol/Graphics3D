using System;
using SharpDX;

namespace Graphics3D
{
    public class Drawing
    {
        protected readonly int Width;
        protected readonly int Height;
        protected readonly byte[] PixelBuffer;

        public Drawing(byte[] bgraArray, int width, int height)
        {
            this.PixelBuffer = bgraArray;
            this.Width = width;
            this.Height = height;
        }

        public void DrawLine(Vector2 point0, Vector2 point1, Color color)
        {
            if ((point1 - point0).Length() < 2)
                return;

            var middle = point0 + ((point1 - point0) / 2);

            DrawPoint((int)middle.X, (int)middle.Y, color);
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
                DrawPoint(x0, y0, color);

                if ((x0 == x1) && (y0 == y1)) break;
                var e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }

        public void DrawPoint(int x, int y, Color color)
        {
            if (x >= 0 && y >= 0 && x < Width && y < Height)
            {
                var index4 = (x + (y * Width)) * 4;
                PixelBuffer[index4] = color.B;
                PixelBuffer[index4 + 1] = color.G;
                PixelBuffer[index4 + 2] = color.R;
                PixelBuffer[index4 + 3] = color.A;
            }
        }

        public void Clear(Color color)
        {
            for (var index = 0; index < PixelBuffer.Length; index += 4)
            {
                PixelBuffer[index] = color.B;
                PixelBuffer[index + 1] = color.G;
                PixelBuffer[index + 2] = color.R;
                PixelBuffer[index + 3] = color.A;
            }
        }
    }
}
