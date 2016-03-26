using System;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using SharpDX;

namespace Graphics3D
{
    public class Device
    {
        private readonly byte[] backBuffer;
        private readonly float[] depthBuffer;
        private readonly WriteableBitmap bmp;
        private readonly int width;
        private readonly int height;
        private readonly Int32Rect bmpRect;

        private readonly Camera camera;
        private readonly Light light;
        private readonly IVisualizer visualizer;

        public Device(
            Camera camera, 
            Light light, 
            WriteableBitmap bmp,
            IVisualizerFactory visualizerFactory)
        {
            this.camera = camera;
            this.light = light;

            this.bmp = bmp;
            width = bmp.PixelWidth;
            height = bmp.PixelHeight;
            bmpRect = new Int32Rect(0, 0, width, height);

            backBuffer = new byte[width * height * 4];
            depthBuffer = new float[width * height];

            visualizer = visualizerFactory.Create(width, height, backBuffer, depthBuffer, light);
        }

        public void Clear(byte r, byte g, byte b, byte a)
        {
            for (var index = 0; index < backBuffer.Length; index += 4)
            {
                // BGRA in Windows
                backBuffer[index]     = b;
                backBuffer[index + 1] = g;
                backBuffer[index + 2] = r;
                backBuffer[index + 3] = a;
            }

            for (var index = 0; index < depthBuffer.Length; index++)
            {
                depthBuffer[index] = float.MaxValue;
            }
        }

        public void Present()
        {
            bmp.Lock();

            Marshal.Copy(backBuffer, 0, bmp.BackBuffer, backBuffer.Length);
            bmp.AddDirtyRect(bmpRect);

            bmp.Unlock();
        }

        public void Render(params Mesh[] meshes)
        {
            float FoV = 0.8f;
            float aspectRatio = (float)width / height;
            float nearPlane = 0.01f, farPlane = 1.0f;

            var viewMatrix = SharpDX.Matrix.LookAtLH(camera.Position, camera.Target, Vector3.UnitY);
            var projectionMatrix = SharpDX.Matrix.PerspectiveFovLH(FoV, aspectRatio, nearPlane, farPlane);

            foreach (Mesh mesh in meshes)
            {
                var worldMatrix =
                    SharpDX.Matrix.Translation(mesh.Position) * 
                    SharpDX.Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z);
                var worldView = worldMatrix * viewMatrix;
                var transformMatrix = worldView * projectionMatrix;

                Parallel.For(0, mesh.Faces.Length, faceIndex =>
                {
                    // Face-back culling
                    //var transformedNormal = Vector3.TransformNormal(mesh.Faces[faceIndex].Normal, worldView);
                    //if (transformedNormal.Z > 0.0)
                    //{
                    //    return;
                    //}

                    var vertexA = mesh.Vertices[mesh.Faces[faceIndex].A];
                    var vertexB = mesh.Vertices[mesh.Faces[faceIndex].B];
                    var vertexC = mesh.Vertices[mesh.Faces[faceIndex].C];

                    var pixelA = Project(vertexA, transformMatrix, worldMatrix);
                    var pixelB = Project(vertexB, transformMatrix, worldMatrix);
                    var pixelC = Project(vertexC, transformMatrix, worldMatrix);

                    visualizer.RenderTriangle(pixelA, pixelB, pixelC, Color.Black, mesh.Texture);
                });
            }
        }

        private Vertex Project(Vertex vertex, Matrix transformationMatrix, Matrix worldMatrix)
        {
            var point2d = Vector3.TransformCoordinate(vertex.Coordinates, transformationMatrix);
            var point3dWorld = Vector3.TransformCoordinate(vertex.Coordinates, worldMatrix);
            var normal3dWorld = Vector3.TransformCoordinate(vertex.Normal, worldMatrix);

            // Fit from center to left-top corner.
            var x = point2d.X * width + width / 2.0f;
            var y = -point2d.Y * height + height / 2.0f;

            vertex.WorldNormal = normal3dWorld;
            vertex.WorldCoordinates = point3dWorld;
            vertex.Coordinates2D = new Vector3(x, y, point2d.Z);

            return vertex;
        }
    }
}
