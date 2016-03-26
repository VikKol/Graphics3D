using System;

namespace Graphics3D
{
    public class VisualizerFactory<T> : IVisualizerFactory where T : IVisualizer
    {
        public IVisualizer Create(int renderWidth, int renderHeight, byte[] backBuffer, float[] depthBuffer, Light light)
            => (IVisualizer)Activator.CreateInstance(
                typeof(T),
                renderWidth,
                renderHeight,
                backBuffer,
                depthBuffer,
                light);
    }
}
