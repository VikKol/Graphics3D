using SharpDX;
using System.Threading.Tasks;

namespace Graphics3D
{
    public class Mesh
    {
        public string Name { get; private set; }
        public Vertex[] Vertices { get; private set; }
        public Face[] Faces { get; private set; }

        public Texture Texture { get; set; }

        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public Mesh(string name, int verticesCount, int facesCount)
        {
            Name = name;
            Vertices = new Vertex[verticesCount];
            Faces = new Face[facesCount];
        }

        public void ComputeFacesNormals()
        {
            Parallel.For(0, Faces.Length, faceIndex =>
            {
                var face = Faces[faceIndex];
                var vertexA = Vertices[face.A];
                var vertexB = Vertices[face.B];
                var vertexC = Vertices[face.C];

                Faces[faceIndex].Normal = (vertexA.Normal + vertexB.Normal + vertexC.Normal) / 3.0f;
                Faces[faceIndex].Normal.Normalize();
            });
        }
    }
}
