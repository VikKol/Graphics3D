using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using SharpDX;
using Graphics3D;

namespace Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const double DPI = 96.0;

        private Device device;
        private Mesh[] meshes;
        private Vector3 light = new Vector3(0, 10, 25);
        private Graphics3D.Camera camera = new Graphics3D.Camera();
        private static DateTime previousDate = DateTime.Now;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var bmp = new WriteableBitmap(
                (int)ImgContainer.ActualWidth, (int)ImgContainer.ActualHeight,
                DPI, DPI, PixelFormats.Pbgra32, null);

            Img.Source = bmp;
            camera.Target = Vector3.Zero;
            camera.Position = new Vector3((float)HorizontalPos.Value, (float)VerticalPos.Value, (float)DepthPos.Value);
            device = new Device(camera, light, bmp);

            //meshes = MeshHelper.LoadFromJsonFile("Meshes/monkey.babylon");
            meshes = MeshHelper.LoadFromJsonFile("Meshes/car.babylon");

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        void CompositionTarget_Rendering(object sender, object e)
        {
            // Fps
            var now = DateTime.Now;
            var currentFps = 1000.0 / (now - previousDate).TotalMilliseconds;
            fps.Content = string.Format("{0:0.00} fps", currentFps);
            previousDate = now;

            // Rendering loop
            device.Clear(0, 0, 0, 255);

            foreach(var mesh in meshes)
            {
                mesh.Rotation = new Vector3(mesh.Rotation.X, mesh.Rotation.Y + 0.01f, mesh.Rotation.Z);
            }

            device.Render(meshes);
            device.Present();
        }

        #region Move camera

        private void left_Click(object sender, RoutedEventArgs e)
        {
            camera.Target = new Vector3(
                camera.Target.X + 0.1f,
                camera.Target.Y,
                camera.Target.Z);
        }

        private void right_Click(object sender, RoutedEventArgs e)
        {
            camera.Target = new Vector3(
                camera.Target.X - 0.1f,
                camera.Target.Y,
                camera.Target.Z);
        }

        private void up_Click(object sender, RoutedEventArgs e)
        {
            camera.Target = new Vector3(
                camera.Target.X,
                camera.Target.Y + 0.1f,
                camera.Target.Z);
        }

        private void down_Click(object sender, RoutedEventArgs e)
        {
            camera.Target = new Vector3(
                camera.Target.X,
                camera.Target.Y - 0.1f,
                camera.Target.Z);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    up_Click(null, null);
                    break;
                case Key.Left:
                    left_Click(null, null);
                    break;
                case Key.Right:
                    right_Click(null, null);
                    break;
                case Key.Down:
                    down_Click(null, null);
                    break;
            }
        }
        
        private void HorizontalPos_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            camera.Position = new Vector3(
                camera.Position.X + (float)(e.NewValue - e.OldValue),
                camera.Position.Y,
                camera.Position.Z);
        }
        private void VerticalPos_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            camera.Position = new Vector3(
                camera.Position.X,
                camera.Position.Y + (float)(e.NewValue - e.OldValue),
                camera.Position.Z);
        }

        private void DepthPos_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            camera.Position = new Vector3(
                camera.Position.X,
                camera.Position.Y,
                camera.Position.Z + (float)(e.NewValue - e.OldValue));
        }

        #endregion
    }
}
