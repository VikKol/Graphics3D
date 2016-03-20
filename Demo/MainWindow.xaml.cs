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
        private Device device;
        private Mesh[] meshes;
        private Graphics3D.Camera camera = new Graphics3D.Camera();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var bmp = new WriteableBitmap(
                (int)ImgContainer.ActualWidth, (int)ImgContainer.ActualHeight, 
                96.0, 96.0, PixelFormats.Pbgra32, null);

            Img.Source = bmp;

            device = new Device(bmp);

            meshes = MeshHelper.LoadFromJsonFile("Meshes/monkey.babylon");

            camera.Position = new Vector3(0, 0, (float)DepthPos.Value);
            camera.Target = new Vector3(0, 0, 0);

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        void CompositionTarget_Rendering(object sender, object e)
        {
            device.Clear(0, 0, 0, 255);

            foreach(var mesh in meshes)
            {
                mesh.Rotation = new Vector3(mesh.Rotation.X, mesh.Rotation.Y + 0.05f, mesh.Rotation.Z);
            }

            device.Render(camera, meshes);
            device.Present();
        }

        private void up_Click(object sender, RoutedEventArgs e)
        {
            camera.Target = new Vector3(
                camera.Target.X,
                camera.Target.Y + 0.1f,
                camera.Target.Z);
        }

        private void left_Click(object sender, RoutedEventArgs e)
        {
            camera.Target = new Vector3(
                camera.Target.X - 0.1f,
                camera.Target.Y,
                camera.Target.Z);
        }

        private void right_Click(object sender, RoutedEventArgs e)
        {
            camera.Target = new Vector3(
                camera.Target.X + 0.1f,
                camera.Target.Y,
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

        private void VerticalPos_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            camera.Position = new Vector3(
                camera.Position.X,
                camera.Position.Y + (float)(e.NewValue - e.OldValue),
                camera.Position.Z);
        }

        private void HorizontalPos_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            camera.Position = new Vector3(
                camera.Position.X + (float)(e.NewValue - e.OldValue),
                camera.Position.Y,
                camera.Position.Z);
        }

        private void DepthPos_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            camera.Position = new Vector3(
                camera.Position.X,
                camera.Position.Y,
                camera.Position.Z + (float)(e.NewValue - e.OldValue));
        }
    }
}
