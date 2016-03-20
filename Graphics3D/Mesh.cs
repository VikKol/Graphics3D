using SharpDX;

namespace Graphics3D
{
    public class Mesh
    {
        public string Name { get; private set; }
        public Vector3[] Vertices { get; private set; }
        public Face[] Faces { get; private set; }

        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public Mesh(string name, int verticesCount, int facesCount)
        {
            Name = name;
            Vertices = new Vector3[verticesCount];
            Faces = new Face[facesCount];
        }
    }
}
