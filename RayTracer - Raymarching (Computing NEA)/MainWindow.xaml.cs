﻿using System;
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
        int rayCountPerPixel = 60;
        bool isAntiAliasing = true;

        
        int maxIterations = 100;
        double maxJumpDistance = 30;
        double minJumpDistance = 0.001;

        int maxBounceCount = 4;
        
        
        
        //  Soft constants - Changed on circumstance
        double AA_Strength = 0.007d;
        static double FoVangle = 110;
        static int res_x = 220;
        static int res_y = 110;

        static Vec3 camLocation = new Vec3(-40, 0, 0);
        double[] camRotations = new double[] { 0, 0, 0 };   //  Rotations in xy, yz, and xz planes respectively


        //  Shapes
        List<Shape> shapes = new List<Shape>();
        
        //  Currently lights are shapes, will need changing at some point
        List<Shape> lights = new List<Shape>();



        //  Precomputed
        double screenRatio = (double)res_y / (double)res_x;
        double FoVScale = Math.Tan(FoVangle * (Math.PI / 180) / 2);  //  FoVangle is in degrees, and must be converted to radians
        
        Camera cameraOne;



        public MainWindow()
        {

            InitializeComponent();

            cameraOne = new Camera(camLocation, camRotations);
            Vec3 pos1 = new Vec3(0, 0, 0);
            Vec3 k_s1 = new Vec3(1, 0, 0);
            Vec3 k_d1 = new Vec3(0.1, 0.6, 0.1);
            double alpha1 = 2;
            double radius1 = 2;
            shapes.Add(new Sphere(pos1, k_s1, k_d1, alpha1, radius1));

            Vec3 pos2 = new Vec3(0, 0, -50);
            Vec3 k_s2 = new Vec3(0, 0, 0);
            Vec3 k_d2 = new Vec3(0.4, 0.2, 0.5);
            double alpha2 = 1;
            double radius2 = 45;
            shapes.Add(new Sphere(pos2, k_s2, k_d2, alpha2, radius2));

            generateAllPixels();
            


            
        }

        void generateAllPixels()
        {
            for (int x = 0; x < res_x; x++)
            {
                for (int y = 0; y < res_y; y++)
                {
                    Color pixelColor = GetPixelColor(x, res_y - y);
                    //int brightness = (255 * x) / res_x;
                    //Color pixelColor = Color.FromArgb(brightness, brightness, brightness);
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
            Vec3 finalColor = new(0, 0, 0);
            for (int i = 0; i < rayCountPerPixel; i++)
            {

                Vec3 rayDirection = FindPixelsRayDirection(x, y);

                

                //Vec3 rayDirection = new(1, 0, 0);
                Ray currentRay = new Ray(cameraOne.position, rayDirection);
                bool checkForNewIntersections = true;
                for (int j = 0; j < maxBounceCount && checkForNewIntersections; j++)
                {
                    Vec3 initialDirection = currentRay.direction;
                    DetermineIntersections(currentRay);
                    if (currentRay.previousShape != null)
                    {
                        Vec3 normal = currentRay.previousShape.findNormal(currentRay.position);

                        currentRay.direction = findingNewRayDirection(normal);
                        Vec3 shapeReflectance = currentRay.previousShape.BRDF_phong(initialDirection, currentRay.direction, normal);
                        currentRay.runningTotalOfReflectance = Vec3.colorCombination(shapeReflectance, currentRay.runningTotalOfReflectance);

                    }
                    else
                    {

                        checkForNewIntersections = false;
                        //  Simulate a sun
                        double sunMagnitude = 10 * Math.Pow(Math.Max(initialDirection * new Vec3(0, 0, 1), 0), 6);
                        Vec3 sunColour = new Vec3(1, 0.8, 0.4);
                        double skyMagnitude = Math.Pow(Math.Max(initialDirection * new Vec3(0, 0, 1), 0), 0.4);
                        Vec3 skyColour = new Vec3(0.3, 0.3, 0.7);
                        Vec3 ambientColour = new Vec3(0.1, 0.1, 0.1);
                        Vec3 lighting = sunMagnitude * sunColour + skyMagnitude * skyColour + ambientColour;
                        finalColor += Vec3.colorCombination(currentRay.runningTotalOfReflectance, lighting);
                    }
                }
            }
            finalColor *= 1/(double)rayCountPerPixel;



            //  Code for checking specific directions
            //double testDirection = Math.Max(new vec3(0, -1, 0) * testRay, 0);
            //return Color.FromArgb((int)(255 * testDirection), (int)(255 * testDirection), (int)(255 * testDirection));

            return Color.FromArgb((int)Math.Min(255 * finalColor.x, 255), (int)Math.Min(finalColor.y * 255, 255), (int)Math.Min(finalColor.z * 255, 255));
        }
        
        Vec3 FindPixelsRayDirection(int x, int y)
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
            //  We take negative of newX because we are in a right hand co-ordinate system, so a ray sent out to the left should have a positive value for y
            Vec3 pixelVector = new Vec3(1, -newX, newZ);


            
            Vec3 rayDirection = cameraOne.camSpaceToWorldSpace(pixelVector) - cameraOne.position;
            rayDirection.normalise();

            
            return rayDirection;
        }

        void DetermineIntersections(Ray currentRay)
        {
            Shape? previousShape = currentRay.previousShape;
            //	previousShape stops us colliding with what we just hit
            currentRay.direction.normalise();
            //  We want normalised versions
            Vec3 currPos = currentRay.position;
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
                currPos += currentRay.direction * lowestDistance;

                //  Tolerance check:
                //  Have we travelled further than the max jump distance?
                //  Have we gone through too many iterations?
                if(iterationCount > maxIterations || lowestDistance == maxJumpDistance){
                    searching = false;
                    closestObject = null;
                    exitCode = "NO_INTERSECTION";
                }
                else if(lowestDistance <= minJumpDistance){
                    searching = false;
                    exitCode = "INTERSECTION";
                }
            }

            currentRay.position = currPos;
            currentRay.previousShape = closestObject;
            currentRay.direction = null;
            //  NEED TO SORT OUT RETURN TYPE
            //  return (exitCode, currPos, closestObject);
        }

        Vec3 findingNewRayDirection(Vec3 normal) {
            double theta = rnd.NextDouble() * 2 * Math.PI;  //   Get a random number for the trig functions
            double phi = rnd.NextDouble() * 2 * Math.PI;    //  Could decrease precision, 2 or 3 dp is likely to be enough

            double cosTheta = Math.Cos(theta);
            double sinTheta = Math.Sin(theta);

            double cosPhi = Math.Cos(phi);
            double sinPhi = Math.Sin(phi);

            //  This method will give us an already normalised value
            Vec3 newDir = new(cosPhi * cosTheta, cosPhi * sinTheta, sinPhi);
            if(newDir * normal < 0)
            {
                newDir *= -1;
            }
            return newDir;
        }
    
    

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            bool change = false;

            if(e.Key == Key.Left) 
            {
                camRotations[0] += 10;
            }
            else if(e.Key == Key.Right)
            {
                camRotations[0] += -10;
            }
            else if (e.Key == Key.Up)
            {
                if(camRotations[2] <= 80)
                {
                    camRotations[2] += 10;
                }
                
            }
            else if (e.Key == Key.Down)
            {
                if(camRotations[2] >= -80)
                {
                    camRotations[2] += -10;
                }
                
            }
            else if(e.Key == Key.U)
            {
                MessageBox.Show("Updating", "Image will start updating upon pressing Ok");
                change = true;
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
