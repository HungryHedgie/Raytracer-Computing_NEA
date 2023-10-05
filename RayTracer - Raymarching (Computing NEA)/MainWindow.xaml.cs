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
        int rayCountPerPixel = 1;

        public MainWindow()
        {
            InitializeComponent();
        }


        Color getPixelColor(int x, int y)
        {
            for (int i = 0; i < rayCountPerPixel; i++)
            {
                findPixelsRayDirection(x, y);
            }
        }
        
        vec3 findPixelsRayDirection(int x, int y)
        {
            //   (x, y) is the pixel co-ordinate
            //   (0, 0) is the bottom left of the image
            // res_x and res_y is the amount of pixels in the x and y directions

            float xScale = ((x + 0.5) / res_x) - 0.5;
            float zScale = ((y + 0.5) / res_y) - 0.5;
    #	The + 0.5 means the ray is sent to the center of a pixel
    #	Without it, the ray would head towards the bottom left of a pixel
	
    #	Random Sampling Section
    xOffset <= 0
    zOffset <= 0
    IF AntiAliasing is true
        R <= (AA_Strength) * sqrt(Random(0, 1))  #   Sqrt ensures uniform distribution
        theta <= Random(0, 2 * pi)
        xOffset <= R * cos(theta)
        zOffset <= R * sin(theta)
    END IF

    # FoVScale accounts for current FoV angle, screenRatio accounts for the image size ratio
            newX = (xScale + xOffset) * FoVScale

    newZ = (zScale + zOffset) * FoVScale * screenRatio
    float3 pixelVector <= (newX, 1, newZ)

    rayDirection <= camera.camToWorldSpace(pixelVector)
    rayOrigin <= camera.Location

    return (rayOrigin, rayDirection - rayOrigin)
        }

    }
}
