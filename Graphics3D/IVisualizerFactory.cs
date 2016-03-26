namespace Graphics3D
{
    public interface IVisualizerFactory
    {
        IVisualizer Create(
            int renderWidth,
            int renderHeight,
            byte[] backBuffer,
            float[] depthBuffer, 
            Light light);
    }
}
