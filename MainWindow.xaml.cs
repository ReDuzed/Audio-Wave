using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using NAudio;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using NAudio.MediaFoundation;

namespace AudioWave
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            var wave = new Wave();
        }
    }
    public class Wave
    {
        private AudioFileReader reader;
        private float[] data;
        private MainWindow Window;
        private WasapiOut audioOut;
        public Wave()
        {
            Window = MainWindow.Instance;
            Init();
            Display();
        }
        private void Init()
        {
            reader = new AudioFileReader("song.wav");
            data = Buffer();
            audioOut = new WasapiOut();
            audioOut.Init(reader);
            audioOut.Play();
        }
        Action method;
        private void Display()
        {
            method = delegate ()
            {
                Thread.Sleep(1000 / 50);
                GenerateImage();
                MainWindow.Instance.graph.Dispatcher.BeginInvoke(method, System.Windows.Threading.DispatcherPriority.Background);
            };
            MainWindow.Instance.graph.Dispatcher.BeginInvoke(method, System.Windows.Threading.DispatcherPriority.Background);
        }
        private void GenerateImage()
        {
            int width = (int)Window.graph.Width;
            int height = (int)Window.graph.Height;
            using (FileStream image = File.Open("image.bmp", FileMode.Open, FileAccess.ReadWrite))
            {
                using (Bitmap bmp = new Bitmap(image))
                {
                    using (Graphics graphic = Graphics.FromImage(bmp))
                    {
                        PointF[] points = new PointF[width];
                        for (int i = 0; i < points.Length; i++)
                        {
                            points[i] = new PointF(i, height / 2 * data[i + reader.Position / 4] + height / 2);
                        }
                        graphic.FillRectangle(System.Drawing.Brushes.Black, new System.Drawing.Rectangle(0, 0, width, height));
                        graphic.DrawCurve(System.Drawing.Pens.White, points);
                    }
                    bmp.Save(image, System.Drawing.Imaging.ImageFormat.Bmp);
                    var bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
                    int stride = width * ((PixelFormats.Bgr24.BitsPerPixel + 7) / 8);
                    Window.graph.Source = BitmapSource.Create(width, height, 96f, 96f, PixelFormats.Bgr24, null, bmpData.Scan0, stride * height, stride);
                    bmp.UnlockBits(bmpData);
                }
            }
        }
        private float[] Buffer()
        {
            float[] buffer = new float[reader.Length * 4];
            reader.ToSampleProvider().Read(buffer, 0, buffer.Length);
            reader.Position = 0;
            return buffer;
        }
    }
}
