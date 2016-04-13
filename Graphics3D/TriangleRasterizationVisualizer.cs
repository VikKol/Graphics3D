using SharpDX;

namespace Graphics3D
{
    public class TriangleRasterizationVisualizer : IVisualizer
    {
        const int AlphaValue = 130;

        private readonly int renderWidth;
        private readonly int renderHeight;
        private readonly byte[] backBuffer;
        private readonly Drawing drawing;

        public TriangleRasterizationVisualizer(int renderWidth, int renderHeight, byte[] backBuffer)
        {
            this.renderWidth = renderWidth;
            this.renderHeight = renderHeight;
            this.backBuffer = backBuffer;
            this.drawing = new Drawing(backBuffer, renderWidth, renderHeight);
        }

        public void RenderTriangle(Vertex v1, Vertex v2, Vertex v3, Color color, Texture texture = null)
        {
            Vector3 p1 = v1.Coordinates2D;
            Vector3 p2 = v2.Coordinates2D;
            Vector3 p3 = v3.Coordinates2D;
            color.A = AlphaValue;

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

            for (var y = v3.Y; y >= v2.Y; y--)
            {
                drawing.DrawBline((int)curx1, (int)y, (int)curx2, (int)y, color);
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

            for (var y = v1.Y; y <= v2.Y; y++)
            {
                drawing.DrawBline((int)curx1, (int)y, (int)curx2, (int)y, color);
                curx1 += invslope1;
                curx2 += invslope2;
            }
        }
    }
}
