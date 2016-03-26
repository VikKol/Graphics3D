using SharpDX;

namespace Graphics3D
{
    public class WireframeVisualizer : IVisualizer
    {
        private readonly int renderWidth;
        private readonly int renderHeight;
        private readonly byte[] backBuffer;
        private readonly Drawing drawing;

        public WireframeVisualizer(int renderWidth, int renderHeight, byte[] backBuffer)
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

            drawing.DrawBline((int)p1.X, (int)p1.Y, (int)p2.X, (int)p2.Y, Color.White);
            drawing.DrawBline((int)p2.X, (int)p2.Y, (int)p3.X, (int)p3.Y, Color.White);
            drawing.DrawBline((int)p3.X, (int)p3.Y, (int)p1.X, (int)p1.Y, Color.White);
        }
    }
}
