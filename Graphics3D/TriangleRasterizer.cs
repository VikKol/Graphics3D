//using SharpDX;

//namespace Graphics3D
//{
//    public class TriangleRasterizer
//    {
//        public void FillTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Color color)
//        {
//            // Sort by Y
//            if (v1.Y > v2.Y)
//            {
//                var temp = v2;
//                v2 = v1;
//                v1 = temp;
//            }
//            if (v2.Y > v3.Y)
//            {
//                var temp = v2;
//                v2 = v3;
//                v3 = temp;
//            }
//            if (v1.Y > v2.Y)
//            {
//                var temp = v2;
//                v2 = v1;
//                v1 = temp;
//            }

//            if (v2.Y == v3.Y)
//            {
//                FillBottomFlatTriangle(v1, v2, v3, color);
//            }
//            else if (v1.Y == v2.Y)
//            {
//                FillTopFlatTriangle(v1, v2, v3, color);
//            }
//            else
//            {
//                var x4 = v1.X + (((v2.Y - v1.Y) / (v3.Y - v1.Y)) * (v3.X - v1.X)); // Find flat divider
//                var v4 = new Vector3(x4, v2.Y, 0);

//                FillBottomFlatTriangle(v1, v2, v4, color);
//                FillTopFlatTriangle(v2, v4, v3, color);
//            }
//        }

//        private void FillTopFlatTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Color color)
//        {
//            float curx1 = v3.X;
//            float curx2 = v3.X;
//            float invslope1 = (v3.X - v1.X) / (v3.Y - v1.Y);
//            float invslope2 = (v3.X - v2.X) / (v3.Y - v2.Y);

//            for (int y = (int)v3.Y; y >= v1.Y; y--)
//            {
//                //DrawBline((int)curx1, y, (int)curx2, y, color);
//                curx1 -= invslope1;
//                curx2 -= invslope2;
//            }
//        }

//        private void FillBottomFlatTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Color color)
//        {
//            float curx1 = v1.X;
//            float curx2 = v1.X;
//            float invslope1 = (v2.X - v1.X) / (v2.Y - v1.Y);
//            float invslope2 = (v3.X - v1.X) / (v3.Y - v1.Y);

//            for (int y = (int)v1.Y; y <= v2.Y; y++)
//            {
//                //DrawBline((int)curx1, y, (int)curx2, y, color);
//                curx1 += invslope1;
//                curx2 += invslope2;
//            }
//        }


//        private void DrawLine(Vector3 point0, Vector3 point1, Color color)
//        {
//            if ((point1 - point0).Length() < 2)
//                return;

//            var middle = point0 + ((point1 - point0) / 2);

//            DrawPoint((int)middle.X, (int)middle.Y, middle.Z, color);
//            DrawLine(point0, middle, color);
//            DrawLine(middle, point1, color);
//        }

//        private void DrawBline(int x0, int y0, int x1, int y1, Color color)
//        {
//            var dx = Math.Abs(x1 - x0);
//            var dy = Math.Abs(y1 - y0);
//            var sx = (x0 < x1) ? 1 : -1;
//            var sy = (y0 < y1) ? 1 : -1;
//            var err = dx - dy;

//            while (true)
//            {
//                DrawPoint(x0, y0, 0, color);

//                if ((x0 == x1) && (y0 == y1)) break;
//                var e2 = 2 * err;
//                if (e2 > -dy) { err -= dy; x0 += sx; }
//                if (e2 < dx) { err += dx; y0 += sy; }
//            }
//        }

//        private void DrawPoint(int x, int y, float z, Color color)
//        {
//            var index = x + (y * width);

//            if (x >= 0 && y >= 0
//                && x < bmp.PixelWidth && y < bmp.PixelHeight
//                && z < depthBuffer[index]) //is new point closer?
//            {
//                depthBuffer[index] = z;

//                var index4 = index * 4;
//                backBuffer[index4] = color.B;
//                backBuffer[index4 + 1] = color.G;
//                backBuffer[index4 + 2] = color.R;
//                backBuffer[index4 + 3] = color.A;
//            }
//        }
//    }
//}
