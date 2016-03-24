using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
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

        public Device(WriteableBitmap bmp)
        {
            this.bmp = bmp;
            width = bmp.PixelWidth;
            height = bmp.PixelHeight;
            bmpRect = new Int32Rect(0, 0, width, height);

            backBuffer = new byte[width * height * 4];
            depthBuffer = new float[width * height];
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

        public void Render(Camera camera, params Mesh[] meshes)
        {
            float FoV = 0.78f;
            float aspectRatio = (float)width / height;
            float nearPlane = 0.01f, farPlane = 1.0f;

            var viewMatrix = SharpDX.Matrix.LookAtLH(camera.Position, camera.Target, Vector3.UnitY);
            var projectionMatrix = SharpDX.Matrix.PerspectiveFovRH(FoV, aspectRatio, nearPlane, farPlane);

            foreach (Mesh mesh in meshes)
            {
                var worldMatrix =
                    SharpDX.Matrix.Translation(mesh.Position) * 
                    SharpDX.Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z);

                var transformMatrix = worldMatrix * viewMatrix * projectionMatrix;

                Parallel.For(0, mesh.Faces.Length, faceIndex =>
                {
                    var vertexA = mesh.Vertices[mesh.Faces[faceIndex].A];
                    var vertexB = mesh.Vertices[mesh.Faces[faceIndex].B];
                    var vertexC = mesh.Vertices[mesh.Faces[faceIndex].C];

                    var pixelA = ProjectTo2D(vertexA, transformMatrix);
                    var pixelB = ProjectTo2D(vertexB, transformMatrix);
                    var pixelC = ProjectTo2D(vertexC, transformMatrix);

                    FillTriangle(pixelA, pixelB, pixelC, Color.Gray);
                });
            }
        }

        private Vector3 ProjectTo2D(Vector3 coord, SharpDX.Matrix transformationMatrix)
        {
            var point = Vector3.TransformCoordinate(coord, transformationMatrix);

            // Fit from center to left-top corner.
            var x = point.X * width + width / 2.0f;
            var y = point.Y * height + height / 2.0f;

            return new Vector3(x, y, point.Z);
        }

        private void DrawPoint(int x, int y, float z, Color color)
        {
            var index = x + (y * width);

            if (x >= 0 && y >= 0
                && x < width 
                && y < height
                && z < depthBuffer[index]) //is new point closer?
            {
                Interlocked.Exchange(ref depthBuffer[index], z);
                backBuffer.AsIntArray((buffer, length) => Interlocked.Exchange(ref buffer[index], (int)color));
            }
        }

        private void ProcessScanLine(int currY, Vector3 v11, Vector3 v12, Vector3 v21, Vector3 v22, Color color)
        {
            // Gradient is max if two vertices are on the same Y.
            var leftGradient = v11.Y == v12.Y ? 1 : (currY - v11.Y) / (v12.Y - v11.Y);
            var rightGradient = v21.Y == v22.Y ? 1 : (currY - v21.Y) / (v22.Y - v21.Y);

            int leftX = (int)MathEx.Interpolate(v11.X, v12.X, leftGradient);
            int rightX = (int)MathEx.Interpolate(v21.X, v22.X, rightGradient);

            float leftZ = MathEx.Interpolate(v11.Z, v12.Z, leftGradient);
            float rightZ = MathEx.Interpolate(v21.Z, v22.Z, rightGradient);

            for (var currX = leftX; currX < rightX; currX++)
            {
                float gradient = (currX - leftX) / (float)(rightX - leftX);
                var currZ = MathEx.Interpolate(leftZ, rightZ, gradient);

                DrawPoint(currX, currY, currZ, color);
            }
        }
        
        public void FillTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Color color)
        {
            // Sort by Y
            if (p1.Y > p2.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;
            }
            if (p2.Y > p3.Y)
            {
                var temp = p2;
                p2 = p3;
                p3 = temp;
            }
            if (p1.Y > p2.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;
            }

            float invSlope1 = 0, invSlope2 = 0;

            if (p2.Y - p1.Y > 0)
                invSlope1 = (p2.X - p1.X) / (p2.Y - p1.Y);
            if (p3.Y - p1.Y > 0)
                invSlope2 = (p3.X - p1.X) / (p3.Y - p1.Y);

            // Triangle is:
            // P1
            // -
            // -- 
            // - -
            // -  -
            // -   - P2
            // -  -
            // - -
            // -
            // P3
            if (invSlope1 > invSlope2)
            {
                for (int currY = (int)p1.Y; currY < (int)p2.Y; currY++)
                {
                    ProcessScanLine(currY, p1, p3, p1, p2, color);
                }
                for (int currY = (int)p2.Y; currY <= (int)p3.Y; currY++)
                {
                    ProcessScanLine(currY, p1, p3, p2, p3, color);
                }
            }
            // Triangle is:
            //       P1
            //        -
            //       -- 
            //      - -
            //     -  -
            // P2 -   - 
            //     -  -
            //      - -
            //        -
            //       P3
            else
            {
                for (int currY = (int)p1.Y; currY <= (int)p2.Y; currY++)
                {
                    ProcessScanLine(currY, p1, p2, p1, p3, color);
                }
                for (int currY = (int)p2.Y; currY <= (int)p3.Y; currY++)
                {
                    ProcessScanLine(currY, p2, p3, p1, p3, color);
                }
            }
        }
    }
}
