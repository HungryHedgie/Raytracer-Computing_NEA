using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Drawing;
using System.IO;

namespace Modelling___Computing_NEA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    
    public partial class MainWindow : Window
    {
        Random rnd = new Random();
        Bitmap originalCircle = new Bitmap(@"..\..\..\BlackCircle.jpg");

        int samples = 10;    //  For testing
        
        int pixelWidthX = 10, pixelWidthY = 10; //  For testing, 40, 40 gives grey square
        bool circularRandom = true; //  For testing
        public MainWindow()
        {
            InitializeComponent();
            lblResolution.Content += " " + pixelWidthX + ", " + pixelWidthY;
            imgOriginalImage.Source = BitmapToImageSource(originalCircle);
        }

        private Bitmap drawSquare(Bitmap input, int x, int y, int width, int height, System.Drawing.Color colour)
        {
            for (int verticalDist = 0; verticalDist < width; verticalDist++)
            {
                for (int horizontalDist = 0; horizontalDist < height; horizontalDist++)
                {
                    input.SetPixel(x + horizontalDist, y + verticalDist, colour);
                }
            }
            return input;
        }

        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            int transformedWidth = originalCircle.Width, transformedHeight = originalCircle.Height;
            Bitmap transformedImage = new Bitmap(transformedWidth, transformedHeight);
            for (int _x = 0; _x < transformedImage.Width; _x++)  //  INEFICIENT
            {
                for (int _y = 0; _y < transformedImage.Height; _y++)
                {

                    System.Drawing.Color newColor = System.Drawing.Color.FromArgb(255, 255, 255);
                    transformedImage.SetPixel(_x, _y, newColor);
                }
            }

            
            
            for (int x = 0; x < transformedWidth- pixelWidthX; x += pixelWidthX)
            {
                for (int y = 0; y < transformedHeight - pixelWidthX; y += pixelWidthY)
                {
                    int[] offset = new int[2] {0, 0};
                    if (samples == 1)
                    {

                    }
                    else if(circularRandom)
                    {
                        double theta = rnd.Next(360);
                        double radius = rnd.Next(pixelWidthX);
                        offset[0] = Convert.ToInt32(radius * Math.Cos(theta * Math.PI / 180));
                        offset[1] = Convert.ToInt32(radius * Math.Sin(theta * Math.PI / 180));
                    }
                    else
                    {
                        offset[0] = rnd.Next(0, 2 * pixelWidthX) - pixelWidthX;
                        offset[1] = rnd.Next(0, 2 * pixelWidthY) - pixelWidthY;
                    }
                    System.Drawing.Color[] locationColours;
                    int[] colours = new int[3] {0, 0, 0};
                    for (int i = 0; i < samples; i++)
                    {
                        
                        System.Drawing.Color locationColour = originalCircle.GetPixel(x + pixelWidthX / 2 + offset[0], y + pixelWidthY / 2 + offset[1]);
                        colours[0] += locationColour.R;
                        colours[1] += locationColour.G;
                        colours[2] += locationColour.B;
                    }
                    System.Drawing.Color locationColourFinal = System.Drawing.Color.FromArgb(colours[0] / samples, colours[1] / samples, colours[2] / samples);
                    transformedImage = drawSquare(transformedImage, x, y, pixelWidthX, pixelWidthY, locationColourFinal);

                }
            }
            imgTransformedImage.Source = BitmapToImageSource(transformedImage);
        }

        private BitmapImage BitmapToImageSource(System.Drawing.Bitmap bitmap)   //  Copied from stack overflow
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }
    }
}
