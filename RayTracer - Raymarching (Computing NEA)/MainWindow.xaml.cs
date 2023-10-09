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
//using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Numerics;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;



namespace RayTracer___Raymarching__Computing_NEA_
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window
    {
        //  Hard constants (never change)
        Random rnd = new Random();  //  For multithreaded, I would need a different random method
        Bitmap bmpFinalImage = new Bitmap(res_x, res_y);

        //  Medium constants - Can be changed for fine tuning algorithm
        int rayCountPerPixel = 1;
        bool isAntiAliasing = false;

        
        int maxIterations = 100;
        double maxJumpDistance = 50;
        double minJumpDistance = 0.01;
        
        
        
        //  Soft constants - Changed on circumstance
        double AA_Strength = 0.2d;
        static double FoVangle = 90;
        static int res_x = 600;
        static int res_y = 300;

        static vec3 camLocation = new vec3(-5, 0, 0);
        double[] camRotations = new double[] { 0, 0, 0 };   //  Rotations in xy, yz, and xz planes respectively


        //  Shapes
        List<Shape> shapes = new List<Shape>();
        //  Currently lights are shapes, will need changing at some point
        List<Shape> lights = new List<Shape>();



        //  Precomputed
        double screenRatio = (double)res_y / (double)res_x;
        double FoVScale = Math.Tan(FoVangle * (Math.PI / 180) / 2);  //  FoVangle is in degrees, and must be converted to radians
        
        camera cameraOne;



        public MainWindow()
        {

            InitializeComponent();

            cameraOne = new camera(camLocation, camRotations);

            generateAllPixels();
            


            
        }

        void generateAllPixels()
        {
            for (int x = 0; x < res_x; x++)
            {
                for (int y = 0; y < res_y; y++)
                {
                    //Color pixelColor = GetPixelColor(x, res_y - y);
                    Color pixelColor = Color.FromArgb(255 * y/res_y, 255 * y / res_y, 255 * y / res_y);
                    bmpFinalImage.SetPixel(x, y, pixelColor);
                }
            }
            

            //  COPIED FROM INTERNET
            //  Converts from Bitmap to BitmapSource, which can be shown on screen
            using (MemoryStream memory = new MemoryStream())
            {
                bmpFinalImage.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                // Set the BitmapImage as the source for the Image control
                imgFinalImage.Source = bitmapImage;
            }
            //  END OF COPIED CODE

            
        }

        Color GetPixelColor(int x, int y)
        {
            /*
            for (int i = 0; i < rayCountPerPixel; i++)
            {
                vec3[] originAndDirection = FindPixelsRayDirection(x, y);
            }*/

            vec3 testRay = FindPixelsRayDirection(x, y);
            double upwards = Math.Max(new vec3(0, 0, 1) * testRay, 0);
            return Color.FromArgb((int)(255 * upwards), (int)(255 * upwards), (int)(255 * upwards));

            //return Color.FromArgb((int)(255 * Math.Max(testRay.x, 0)), (int)(255 * Math.Max(testRay.y, 0)), (int)(255 * Math.Max(testRay.z, 0)));
        }
        
        vec3 FindPixelsRayDirection(int x, int y)
        {
            //   (x, y) is the pixel co-ordinate
            //   (0, 0) is the bottom left of the image
            // res_x and res_y is the amount of pixels in the x and y directions

            double xScale = ((x + 0.5) / res_x) - 0.5;  //  Could precompute?
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
            vec3 pixelVector = new vec3(1, newX, newZ);


            //  Need to make camera
            vec3 rayDirection = cameraOne.camSpaceToWorldSpace(pixelVector) - cameraOne.position;
            rayDirection.normalise();

            
            return rayDirection;
        }

        void DetermineIntersections(vec3 rayOrigin, vec3 rayDirection, Shape previousShape)
        {
            //	previousShape stops us colliding with what we just hit
            rayDirection.normalise();
            //  We want normalised versions
            vec3 currPos = rayOrigin;
            bool searching = true;
            int iterationCount = 0;

            string exitCode = "ERROR";
            Shape closestObject = null;

            while(searching)
            {
                //  Find closest surface
                double lowestDistance = maxJumpDistance;
                foreach (Shape currentShape in shapes)
                {
                    //  Is this a valid comparison?             !IMPORTANT!
                    if (currentShape != previousShape)
                    {
                        double newDistance = currentShape.SDF(currPos);
                        // SDF - Signed Distance Function

                        if (newDistance < lowestDistance)
                        {
                            lowestDistance = newDistance;
                            closestObject = currentShape;
                        }
                    }
                }
                //  For now lights are treated (intersection wise) as the same as shape
                foreach (Shape currentLight in lights)
                {
                    //  Is this a valid comparison?             !IMPORTANT!
                    if (currentLight != previousShape)
                    {
                        double newDistance = currentLight.SDF(currPos);
                        // SDF - Signed Distance Function

                        if (newDistance < lowestDistance)
                        {
                            lowestDistance = newDistance;
                            closestObject = currentLight;
                        }
                    }
                }
                //  Iteration counts prevents iteration going on forever
                iterationCount++;

                //  Dist to closest surface used to find new position
                currPos += rayDirection * lowestDistance;

                //  Tolerance check:
                //  Have we travelled further than the max jump distance?
                //  Have we gone through too many iterations?
                if(iterationCount > maxIterations || lowestDistance == maxJumpDistance){
                    searching = false;
                    exitCode = "NO_INTERSECTION";
                }
                else if(lowestDistance <= minJumpDistance){
                    searching = false;
                    exitCode = "INTERSECTION";
                }
            }
            //  NEED TO SORT OUT RETURN TYPE
            //  return (exitCode, currPos, closestObject);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            bool change = true;
            if(e.Key == Key.Left) 
            {
                camRotations[0] += 10;
            }
            else if(e.Key == Key.Right)
            {
                camRotations[0] -= 10;
            }
            else if (e.Key == Key.Up)
            {
                if(camRotations[2] >= -80)
                {
                    camRotations[2] -= 10;
                }
                
            }
            else if (e.Key == Key.Down)
            {
                if(camRotations[2] <= 80)
                {
                    camRotations[2] += 10;
                }
                
            }
            else
            {
                change = false;
            }
            if(change)
            {
                cameraOne.newRotation(camRotations);
                generateAllPixels();
            }
            if(e.Key == Key.S)  //  S key brings up save menu
            {
                DateTime time = DateTime.Now;
                MessageBoxResult result = MessageBox.Show("Do you want to save your image?", "Saving", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    bmpFinalImage.Save("..\\..\\..\\..\\Images\\" + time.Year + "." + time.Month + "." + time.Day + "." + time.Hour + "." + time.Minute + "_RayTracer_" + time.Millisecond + ".png", ImageFormat.Png);
                    //bmpFinalImage.Save("TEST.png", ImageFormat.Png);

                }
            }
            

        }
    }


}
