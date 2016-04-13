using SharpDX;
using System;

namespace Graphics3D
{
    public class DirectLight : Light
    {
        public DirectLight(float minFration = 0f) : base(minFration) { }

        public override float CalcLightFraction(Vector3 vertex, Vector3 normal)
        {
            var lightDirection = Position - vertex;

            normal.Normalize();
            lightDirection.Normalize();

            return Math.Max(MinFraction, Vector3.Dot(normal, lightDirection)); //Cosine
        }
    }
}
