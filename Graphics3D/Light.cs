using SharpDX;

namespace Graphics3D
{
    public abstract class Light
    {
        protected readonly float MinFraction;

        protected Light(float minFraction)
        {
            MinFraction = minFraction;
        }

        public Vector3 Position { get; set; }

        public abstract float CalcLightFraction(Vector3 vertex, Vector3 normal);
    }
}
