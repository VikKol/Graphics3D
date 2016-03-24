﻿using System;
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

        private readonly Camera camera;
        private readonly Vector3 lightPosition;
        
        public Device(Camera camera, Vector3 lightPosition, WriteableBitmap bmp)
        {
            this.camera = camera;
            this.lightPosition = lightPosition;

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

        public void Render(params Mesh[] meshes)
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

                    var pixelA = ProjectTo2D(vertexA, transformMatrix, worldMatrix);
                    var pixelB = ProjectTo2D(vertexB, transformMatrix, worldMatrix);
                    var pixelC = ProjectTo2D(vertexC, transformMatrix, worldMatrix);

                    FillTriangle(pixelA, pixelB, pixelC, Color.DarkGray);
                });
            }
        }

        private Vertex ProjectTo2D(Vertex vertex, Matrix transformationMatrix, Matrix worldMatrix)
        {
            var point2d = Vector3.TransformCoordinate(vertex.Coordinates, transformationMatrix);
            var point3dWorld = Vector3.TransformCoordinate(vertex.Coordinates, worldMatrix);
            var normal3dWorld = Vector3.TransformCoordinate(vertex.Normal, worldMatrix);

            // Fit from center to left-top corner.
            var x = point2d.X * width + width / 2.0f;
            var y = point2d.Y * height + height / 2.0f;

            vertex.WorldNormal = normal3dWorld;
            vertex.WorldCoordinates = point3dWorld;
            vertex.Coordinates2D = new Vector3(x, y, point2d.Z);

            return vertex;
        }

        private void ProcessScanLine(ScanLineData data, Vertex v11, Vertex v12, Vertex v21, Vertex v22, Color color)
        {
            int currY = data.CurrentY;
            Vector3 p11 = v11.Coordinates2D;
            Vector3 p12 = v12.Coordinates2D;
            Vector3 p21 = v21.Coordinates2D;
            Vector3 p22 = v22.Coordinates2D;

            // Gradient is max if two vertices are on the same Y.
            var leftGradient = p11.Y == p12.Y ? 1 : (currY - p11.Y) / (p12.Y - p11.Y);
            var rightGradient = p21.Y == p22.Y ? 1 : (currY - p21.Y) / (p22.Y - p21.Y);

            int leftX = (int)MathEx.Interpolate(p11.X, p12.X, leftGradient);
            int rightX = (int)MathEx.Interpolate(p21.X, p22.X, rightGradient);

            float leftZ = MathEx.Interpolate(p11.Z, p12.Z, leftGradient);
            float rightZ = MathEx.Interpolate(p21.Z, p22.Z, rightGradient);

            float leftNdotLight = MathEx.Interpolate(data.NdotLightV11, data.NdotLightV12, leftGradient);
            float rightNdotLight = MathEx.Interpolate(data.NdotLightV21, data.NdotLightV22, rightGradient);

            for (var currX = leftX; currX < rightX; currX++)
            {
                float gradient = (currX - leftX) / (float)(rightX - leftX);
                var currZ = MathEx.Interpolate(leftZ, rightZ, gradient);
                var currNdotLight = MathEx.Interpolate(leftNdotLight, rightNdotLight, gradient);

                DrawPoint(currX, currY, currZ, color * currNdotLight);
            }
        }

        private void FillTriangle(Vertex v1, Vertex v2, Vertex v3, Color color)
        {
            // Sort by Y
            if (v1.Coordinates2D.Y > v2.Coordinates2D.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }
            if (v2.Coordinates2D.Y > v3.Coordinates2D.Y)
            {
                var temp = v2;
                v2 = v3;
                v3 = temp;
            }
            if (v1.Coordinates2D.Y > v2.Coordinates2D.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }

            Vector3 p1 = v1.Coordinates2D;
            Vector3 p2 = v2.Coordinates2D;
            Vector3 p3 = v3.Coordinates2D;

            var data = new ScanLineData();
            float NdotLight1 = CalcCosineAlpha(v1.WorldCoordinates, v1.WorldNormal, lightPosition);
            float NdotLight2 = CalcCosineAlpha(v2.WorldCoordinates, v2.WorldNormal, lightPosition);
            float NdotLight3 = CalcCosineAlpha(v3.WorldCoordinates, v3.WorldNormal, lightPosition);

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
                    data.CurrentY = currY;
                    data.NdotLightV11 = NdotLight1;
                    data.NdotLightV12 = NdotLight3;
                    data.NdotLightV21 = NdotLight1;
                    data.NdotLightV22 = NdotLight2;
                    ProcessScanLine(data, v1, v3, v1, v2, color);
                }
                for (int currY = (int)p2.Y; currY <= (int)p3.Y; currY++)
                {
                    data.CurrentY = currY;
                    data.NdotLightV11 = NdotLight1;
                    data.NdotLightV12 = NdotLight3;
                    data.NdotLightV21 = NdotLight2;
                    data.NdotLightV22 = NdotLight3;
                    ProcessScanLine(data, v1, v3, v2, v3, color);
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
                    data.CurrentY = currY;
                    data.NdotLightV11 = NdotLight1;
                    data.NdotLightV12 = NdotLight2;
                    data.NdotLightV21 = NdotLight1;
                    data.NdotLightV22 = NdotLight3;
                    ProcessScanLine(data, v1, v2, v1, v3, color);
                }
                for (int currY = (int)p2.Y; currY <= (int)p3.Y; currY++)
                {
                    data.CurrentY = currY;
                    data.NdotLightV11 = NdotLight2;
                    data.NdotLightV12 = NdotLight3;
                    data.NdotLightV21 = NdotLight1;
                    data.NdotLightV22 = NdotLight3;
                    ProcessScanLine(data, v2, v3, v1, v3, color);
                }
            }
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

                int bgra = (int)new Color(color.B, color.G, color.R, color.A);
                backBuffer.AsIntArray((buffer, length) => Interlocked.Exchange(ref buffer[index], bgra));
            }
        }

        private static float CalcCosineAlpha(Vector3 vertex, Vector3 normal, Vector3 lightPosition)
        {
            var lightDirection = lightPosition - vertex;

            normal.Normalize();
            lightDirection.Normalize();

            return Math.Max(0, Vector3.Dot(normal, lightDirection));
        }
    }
}
