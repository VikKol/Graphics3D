using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using SharpDX;

namespace Graphics3D
{
    public class Device
    {
        private byte[] backBuffer;
        private WriteableBitmap bmp;

        public Device(WriteableBitmap bmp)
        {
            this.bmp = bmp;
            backBuffer = new byte[bmp.PixelWidth * bmp.PixelHeight * 4]; //R,G,B,A 
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
        }

        public void Present()
        {
            bmp.Lock();

            Marshal.Copy(backBuffer, 0, bmp.BackBuffer, backBuffer.Length);
            bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));

            bmp.Unlock();
        }

        public void PutPixel(int x, int y, System.Windows.Media.Color color)
        {
            var index = (x + (y * bmp.PixelWidth)) * 4;
            backBuffer[index]     = color.B;
            backBuffer[index + 1] = color.G;
            backBuffer[index + 2] = color.R;
            backBuffer[index + 3] = color.A;
        }

        public Vector ProjectTo2D(Vector3 coord, SharpDX.Matrix transformationMatrix)
        {
            var point = Vector3.TransformCoordinate(coord, transformationMatrix);

            // Fit from center to left-top corner.
            var x = point.X * bmp.PixelWidth + bmp.PixelWidth / 2.0f;
            var y = point.Y * bmp.PixelHeight + bmp.PixelHeight / 2.0f;

            return new Vector(x, y);
        }

        public void DrawPoint(Vector point)
        {
            if (point.X >= 0 && point.Y >= 0 
                && point.X < bmp.PixelWidth && point.Y < bmp.PixelHeight)
            {
                PutPixel((int)point.X, (int)point.Y, Colors.LightGray);
            }
        }

        public void DrawLine(Vector point0, Vector point1)
        {
            if ((point1 - point0).Length < 2)
                return;

            var middle = point0 + ((point1 - point0) / 2);

            DrawPoint(middle);
            DrawLine(point0, middle);
            DrawLine(middle, point1);
        }

        public void DrawBline(Vector point0, Vector point1)
        {
            int x0 = (int)point0.X;
            int y0 = (int)point0.Y;
            int x1 = (int)point1.X;
            int y1 = (int)point1.Y;

            var dx = Math.Abs(x1 - x0);
            var dy = Math.Abs(y1 - y0);
            var sx = (x0 < x1) ? 1 : -1;
            var sy = (y0 < y1) ? 1 : -1;
            var err = dx - dy;

            while (true)
            {
                DrawPoint(new Vector(x0, y0));

                if ((x0 == x1) && (y0 == y1)) break;
                var e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }

        public void Render(Camera camera, params Mesh[] meshes)
        {
            var viewMatrix = SharpDX.Matrix.LookAtLH(camera.Position, camera.Target, Vector3.UnitY);

            float FoV = 0.78f;
            float aspectRatio = (float)bmp.PixelWidth / bmp.PixelHeight;
            float nearPlane = 0.01f, farPlane = 1.0f;
            var projectionMatrix = SharpDX.Matrix.PerspectiveFovRH(FoV, aspectRatio, nearPlane, farPlane);

            foreach (Mesh mesh in meshes)
            {
                var worldMatrix =
                    SharpDX.Matrix.Translation(mesh.Position) * 
                    SharpDX.Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z);

                var transformMatrix = worldMatrix * viewMatrix * projectionMatrix;

                foreach (var face in mesh.Faces)
                {
                    var vertexA = mesh.Vertices[face.A];
                    var vertexB = mesh.Vertices[face.B];
                    var vertexC = mesh.Vertices[face.C];

                    var pixelA = ProjectTo2D(vertexA, transformMatrix);
                    var pixelB = ProjectTo2D(vertexB, transformMatrix);
                    var pixelC = ProjectTo2D(vertexC, transformMatrix);

                    DrawBline(pixelA, pixelB);
                    DrawBline(pixelB, pixelC);
                    DrawBline(pixelC, pixelA);
                }
            }
        }
    }
}
