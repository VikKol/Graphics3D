using System;
using SharpDX;

namespace Graphics3D
{
    public class AmbientLight : Light
    {
        public AmbientLight(float minFration = 0f) : base(minFration) { }

        public override float CalcLightFraction(Vector3 vertex, Vector3 normal)
        {
            var mappedNormal = vertex - normal;
            var lightDirection = vertex - Position;

            mappedNormal.Normalize();
            lightDirection.Normalize();

            return Math.Max(MinFraction, Vector3.Dot(mappedNormal, lightDirection)); //Cosine
        }
    }
}
