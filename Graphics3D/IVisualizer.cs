using SharpDX;

namespace Graphics3D
{
    public interface IVisualizer
    {
        void RenderTriangle(Vertex v1, Vertex v2, Vertex v3, Color color, Texture texture = null);
    }
}
