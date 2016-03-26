using System;
using SharpDX;

namespace Graphics3D
{
    public class SimpleTriangleRasterizationVisualizer : IVisualizer
    {
        private readonly int renderWidth;
        private readonly int renderHeight;
        private readonly byte[] backBuffer;
        private readonly float[] depthBuffer;
        private readonly Light light;

        public SimpleTriangleRasterizationVisualizer(
            int renderWidth,
            int renderHeight,
            byte[] backBuffer,
            float[] depthBuffer,
            Light light)
        {
            this.renderWidth = renderWidth;
            this.renderHeight = renderHeight;
            this.backBuffer = backBuffer;
            this.depthBuffer = depthBuffer;
            this.light = light;
        }

        public void RenderTriangle(Vertex v1, Vertex v2, Vertex v3, Color color, Texture texture = null)
        {
            Vector3 p1 = v1.Coordinates2D;
            Vector3 p2 = v2.Coordinates2D;
            Vector3 p3 = v3.Coordinates2D;

            // Sort by Y
            if (p1.Y > p2.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;
            }
            if (p2.Y > p3.Y)
            {
                var temp = p2;
                p2 = p3;
                p3 = temp;
            }
            if (p1.Y > p2.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;
            }

            if (p2.Y == p3.Y)
            {
                FillBottomFlatTriangle(p1, p2, p3, color);
            }
            else if (p1.Y == p2.Y)
            {
                FillTopFlatTriangle(p1, p2, p3, color);
            }
            else
            {
                // Find flat divider
                var x4 = p1.X + (((p2.Y - p1.Y) / (p3.Y - p1.Y)) * (p3.X - p1.X));
                var v4 = new Vector3(x4, p2.Y, 0);

                FillBottomFlatTriangle(p1, p2, v4, color);
                FillTopFlatTriangle(p2, v4, p3, color);
            }
        }

        private void FillTopFlatTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Color color)
        {
            float curx1 = v3.X;
            float curx2 = v3.X;
            float invslope1 = (v3.X - v1.X) / (v3.Y - v1.Y);
            float invslope2 = (v3.X - v2.X) / (v3.Y - v2.Y);

            for (int y = (int)v3.Y; y >= v1.Y; y--)
            {
                DrawBline((int)curx1, y, (int)curx2, y, color);
                curx1 -= invslope1;
                curx2 -= invslope2;
            }
        }

        private void FillBottomFlatTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Color color)
        {
            float curx1 = v1.X;
            float curx2 = v1.X;
            float invslope1 = (v2.X - v1.X) / (v2.Y - v1.Y);
            float invslope2 = (v3.X - v1.X) / (v3.Y - v1.Y);

            for (int y = (int)v1.Y; y <= v2.Y; y++)
            {
                DrawBline((int)curx1, y, (int)curx2, y, color);
                curx1 += invslope1;
                curx2 += invslope2;
            }
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

        private void DrawBline(int x0, int y0, int x1, int y1, Color color)
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

        private void PutPixel(int x, int y, Color color)
        {
            if (x >= 0 && y >= 0 && x < renderWidth && y < renderHeight)
            {
                var index4 = x + (y * renderWidth) * 4;
                backBuffer[index4] = color.B;
                backBuffer[index4 + 1] = color.G;
                backBuffer[index4 + 2] = color.R;
                backBuffer[index4 + 3] = color.A;
            }
        }
    }
}
