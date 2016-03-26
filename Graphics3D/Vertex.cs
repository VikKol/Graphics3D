using SharpDX;

namespace Graphics3D
{
    public struct Vertex
    {
        public Vector3 Normal;
        public Vector3 Coordinates;
        
        public Vector3 WorldNormal;
        public Vector3 WorldCoordinates;

        public Vector3 Coordinates2D;

        public Vector2 TextureCoordinates;
    }
}
