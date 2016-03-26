namespace Graphics3D
{
    public class VisualizerFactory<T> : IVisualizerFactory where T : IVisualizer
    {
        public IVisualizer Create(int renderWidth, int renderHeight, byte[] backBuffer, float[] depthBuffer, Light light)
        {
            if (typeof(T) == typeof(TextureVisualizer))
            {
                return new TextureVisualizer(renderWidth, renderHeight, backBuffer, depthBuffer, light);
            }
            if (typeof(T) == typeof(WireframeVisualizer))
            {
                return new WireframeVisualizer(renderWidth, renderHeight, backBuffer);
            }
            if (typeof(T) == typeof(TriangleRasterizationVisualizer))
            {
                return new TriangleRasterizationVisualizer(renderWidth, renderHeight, backBuffer);
            }
            return null;
        }
    }
}
