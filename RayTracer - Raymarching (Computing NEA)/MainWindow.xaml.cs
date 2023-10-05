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



namespace RayTracer___Raymarching__Computing_NEA_
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window
    {
        //  Hard constants (never change)
        Random rnd = new Random();  //  For multithreaded, I would need a different random method

        //  Medium constants - Can be changed for fine tuning algorithm
        int rayCountPerPixel = 1;
        bool isAntiAliasing = true;
        
        
        //  Soft constants - Changed on circumstance
        double AA_Strength = 0.2d;
        static double FoVangle = 90;
        static int res_x = 600;
        static int res_y = 400;

        //  Precomputed
        double screenRatio = res_y / res_x;
        double FoVScale = Math.Tan(FoVangle * (Math.PI / 180) / 2);  //  FoVangle is in degrees, and must be converted to radians
        public MainWindow()
        {
            //  Precomputed values:
            double FoVScale = Math.Tan(FoVangle * (Math.PI / 180) / 2);  //  FoVangle is in degrees, and must be converted to radians
            double screenRatio = res_y / res_x;


            InitializeComponent();
        }


        Color getPixelColor(int x, int y)
        {
            for (int i = 0; i < rayCountPerPixel; i++)
            {
                findPixelsRayDirection(x, y);
            }

            return
        }
        
        vec3 findPixelsRayDirection(int x, int y)
        {
            //   (x, y) is the pixel co-ordinate
            //   (0, 0) is the bottom left of the image
            // res_x and res_y is the amount of pixels in the x and y directions

            double xScale = ((x + 0.5) / res_x) - 0.5;
            double zScale = ((y + 0.5) / res_y) - 0.5;
            //	The + 0.5 means the ray is sent to the center of a pixel
            //	Without it, the ray would head towards the bottom left of a pixel

            // Random Sampling Section
            double xOffset = 0;
            double zOffset = 0;
            if (isAntiAliasing == true) {
                double R = (AA_Strength) * Math.Sqrt(rnd.NextDouble());  //   Sqrt ensures uniform distribution
                double theta = 2 * Math.PI * rnd.NextDouble();
                xOffset = R * Math.Cos(theta);
                zOffset = R * Math.Sin(theta);
            }

            // FoVScale accounts for current FoV angle, screenRatio accounts for the image size ratio

            double newX = (xScale + xOffset) * FoVScale;
            double newZ = (zScale + zOffset) * FoVScale * screenRatio;
            vec3 pixelVector = new vec3(newX, 1, newZ);

            //  Need to make camera
            vec3 rayDirection = camera.camSpaceToWorldSpace(pixelVector);
            vec3 rayOrigin = camera.position;

            return (rayOrigin, rayDirection - rayOrigin);
        }

    }
}
