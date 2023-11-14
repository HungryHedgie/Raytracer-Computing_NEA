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
        Bitmap bmpFinalImage;


        //  Controls sensitivity of user controls
        double distMovedPerKeyPress = 10;
        double distRotPerKeyPress = 15;


    //  Soft constants - Changed on circumstance
    //  Anti-Aliasing
        

        //  Controls screen resolution
        //int res_x = 260;
        //int res_y = 100;

        //  Controls initial camera sections
        
        static Vec3 camLocation = new Vec3(-30, 0, 20);
        double[] camRotations = new double[] { -20, 0, 0 };   //  Rotations in xy, yz, and xz planes respectively
        Vec3 newMovement = new(0, 0, 0);

        //  All Shapes
        List<Shape> shapes = new List<Shape>();
        
        //  Currently lights are shapes, will need changing at some point
        List<Shape> lights = new List<Shape>();


        SettingInfo currentSettings = new(
                res_x: 250,
                res_y: 170,
                rayCountPerPixel: 120,

                maxIterations: 150,
                maxJumpDistance: 400,
                minJumpDistance: 0.01d,
                maxBounceCount: 12,

                isAntiAliasing: true,
                AA_Strength: 0.02d,
                FoVangle: 110

                );


        Camera cameraOne;

        public struct SettingInfo
        {
            //  Given values here are all place holders
        //  High performance impact
            public int res_x = 10;
            public int res_y = 10;
            public int rayCountPerPixel = 3; //  Rays sent out for each pixel

            //  Unkown/Medium performance impact
            //  Controls cutoff and precision the ray-marching uses
            public int maxIterations = 150;
            public double maxJumpDistance = 400;
            public double minJumpDistance = 0.01;
            public int maxBounceCount = 12;

            //  Low performance impact
            public bool isAntiAliasing = true;
            public double AA_Strength = 0.02d;
            public double FoVangle = 110;

            //  Precomputed
            public double screenRatio;
            public double FoVScale;
            public SettingInfo(int res_x, int res_y, int rayCountPerPixel, int maxIterations, double maxJumpDistance, double minJumpDistance, int maxBounceCount, bool isAntiAliasing, double AA_Strength, double FoVangle)
            {
                this.res_x = res_x;
                this.res_y = res_y;
                this.rayCountPerPixel = rayCountPerPixel;

                this.maxIterations = maxIterations;
                this.maxJumpDistance = maxJumpDistance;
                this.minJumpDistance = minJumpDistance;
                this.maxBounceCount = maxBounceCount;

                this.isAntiAliasing = isAntiAliasing;
                this.AA_Strength = AA_Strength;
                this.FoVangle = FoVangle;

                
                this.screenRatio = (double)this.res_y / (double)this.res_x;
                this.FoVScale = Math.Tan(this.FoVangle * (Math.PI / 180) / 2);  //  FoVangle is in degrees, and must be converted to radians


            }
        }

        public MainWindow()
        {

            InitializeComponent();

            //  Constant initialisation (Can't be statics as they may be changed)

            bmpFinalImage = new Bitmap(currentSettings.res_x, currentSettings.res_y);
            
            //  Shape and camera initialisation

            cameraOne = new Camera(camLocation, camRotations);

            //  First sphere
            Vec3 pos1 = new Vec3(100, -50, 0);
            Vec3 k_s1 = new Vec3(0.1, 0.8, 0.9);
            Vec3 k_d1 = new Vec3(0.9, 0.2, 0.1);
            double alpha1 = 2;
            double radius1 = 15;
            Vec3 lightStrength1 = 15 * new Vec3(1, 1, 1);
            //lights.Add(new Sphere(pos1, k_s1, k_d1, alpha1, radius1, lightStrength1));
            
            //  Second sphere
            Vec3 pos2 = new Vec3(0, 0, -20000);
            Vec3 k_s2 = 1*new Vec3(0.3, 0.2, 0.9);
            Vec3 k_d2 = 1*new Vec3(0.7, 0.8, 0.1);
            double alpha2 = 3;
            double radius2 = 20000;
            shapes.Add(new Sphere(pos2, k_s2, k_d2, alpha2, radius2));

            //  Third sphere
            Vec3 pos3 = new Vec3(0, -20050, 0);
            Vec3 k_s3 = new Vec3(.9, 0.9, 0.1);
            Vec3 k_d3 = new Vec3(0.1, 0.1, 0.9);
            double alpha3 = 20;
            double radius3 = 20000;
            shapes.Add(new Sphere(pos3, k_s3, k_d3, alpha3, radius3));
            
            //  Fourth sphere
            Vec3 pos4 = new Vec3(50, 0, 30);
            Vec3 k_s4 = new Vec3(0, 0, 0);
            Vec3 k_d4 = new Vec3(1,1, 1);
            double alpha4 = 14;
            double radius4 = 15;
            Vec3 lightStrength4 = 14 * new Vec3(1, 1, 1);
            lights.Add(new Sphere(pos4, k_s4, k_d4, alpha4, radius4, lightStrength4));

            //  Fifth sphere 105,76,179
            Vec3 pos5 = new Vec3(25, -25, 20);
            Vec3 k_s5 = 1 * new Vec3(0.3, 0.2, 0.9);
            Vec3 k_d5 = 1 * new Vec3(0.4, 0.3, 0.8);
            double alpha5 = 3;
            double radius5 = 10;
            shapes.Add(new Sphere(pos5, k_s5, k_d5, alpha5, radius5));

            //  DEBUG CODE
            //SettingsWindow SettingsWindow01 = new SettingsWindow();
            //SettingsWindow.
            //SettingsWindow01.Show();

            //  END OF DEBUG CODE


            //  Main loop
            GenerateAllPixels();
            


            
        }

        void GenerateAllPixels()
        {
            //  Loops through each pixel, generating rays to find the pixel's colour
            for (int x = 0; x < currentSettings.res_x; x++)
            {
                for (int y = 0; y < currentSettings.res_y; y++)
                {
                    //Color pixelColor = GetPixelColor(x, currentSettings.res_y - y);
                    Color pixelColor = GetPixelColor(x, currentSettings.res_y - y);
                    //int brightness = (255 * x) / currentSettings.res_x;
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
            //  This will be added to track the contributions of each ray towards the colour
            Vec3 finalColor = new(0, 0, 0);

            //  Loop through to run a ray calculation as many times as needed
            for (int i = 0; i < currentSettings.rayCountPerPixel; i++)
            {

                Vec3 rayDirection = FindPixelsRayDirection(x, y);

                

                
                Ray currentRay = new Ray(cameraOne.position, rayDirection);
                bool checkForNewIntersections = true;
                for (int j = 0; j < currentSettings.maxBounceCount && checkForNewIntersections && !currentRay.hasHitLight; j++)
                {
                    Vec3 initialDirection = currentRay.direction;
                    DetermineIntersections(currentRay);
                    if (currentRay.previousShape != null && !currentRay.hasHitLight)
                    {
                        Vec3 normal = currentRay.previousShape.FindNormal(currentRay.position);

                        currentRay.direction = FindingNewRayDirection(normal);
                        Vec3 shapeReflectance = currentRay.previousShape.BRDF_phong(currentRay.direction, initialDirection, normal);
                        currentRay.sumOfReflectance = Vec3.ColorCombination(shapeReflectance, currentRay.sumOfReflectance);

                    }
                    else
                    {

                        checkForNewIntersections = false;
                        //  Get colour from lights
                        Vec3 lightStrength = new(0, 0, 0);
                        if (currentRay.previousShape != null && currentRay.hasHitLight)
                        {
                            lightStrength = currentRay.previousShape.lightStrength;
                        }
                        
                        //  Simulate a sun and skyline
                        double sunMagnitude = 10 * Math.Pow(Math.Max(initialDirection * new Vec3(1, 0, 0), 0), 128);
                        Vec3 sunColour = 0 * new Vec3(1, 0.8, 0.4);
                        double skyMagnitude = Math.Pow(Math.Max(initialDirection * new Vec3(0, 0, 1), 0), 0.4);
                        Vec3 skyColour = new Vec3(0.3, 0.3, 0.7);
                        Vec3 ambientColour = new Vec3(0.2, 0.2, 0.2);
                        Vec3 lighting = sunMagnitude * sunColour + skyMagnitude * skyColour + ambientColour + lightStrength;
                        finalColor += Vec3.ColorCombination(currentRay.sumOfReflectance, lighting);
                    }
                }
            }
            finalColor *= 1/(double)currentSettings.rayCountPerPixel;



            //  Code for checking specific directions
            //double testDirection = Math.Max(new vec3(0, -1, 0) * testRay, 0);
            //return Color.FromArgb((int)(255 * testDirection), (int)(255 * testDirection), (int)(255 * testDirection));

            return Color.FromArgb((int)Math.Min(255 * finalColor.x, 255), (int)Math.Min(finalColor.y * 255, 255), (int)Math.Min(finalColor.z * 255, 255));
        }

        Color GetPixelColor_DEBUG(int x, int y)
        {
            //double testDirection = Math.Max(new vec3(0, -1, 0) * testRay, 0);
            //return Color.FromArgb((int)(255 * testDirection), (int)(255 * testDirection), (int)(255 * testDirection));
            Vec3 rayDirection = FindPixelsRayDirection(x, y);

            return Color.FromArgb((int)Math.Max(255 * rayDirection.x, 0), (int)Math.Max(rayDirection.y * 255, 0), (int)Math.Max(rayDirection.z * 255, 0));
        }

        Vec3 FindPixelsRayDirection(int x, int y)
        {
            //   (x, y) is the pixel co-ordinate
            //   (0, 0) is the bottom left of the image
            // res_x and res_y is the amount of pixels in the x and y directions

            double xScale = ((x + 0.5) / currentSettings.res_x) - 0.5;  //  Could precompute?
            double zScale = ((y + 0.5) / currentSettings.res_y) - 0.5;
            //	The + 0.5 means the ray is sent to the center of a pixel
            //	Without it, the ray would head towards the bottom left of a pixel

            // Random offset generation
            double xOffset = 0;
            double zOffset = 0;
            if (currentSettings.isAntiAliasing == true) {
                double R = (currentSettings.AA_Strength) * Math.Sqrt(rnd.NextDouble());  //   Sqrt ensures uniform distribution
                double theta = 2 * Math.PI * rnd.NextDouble();
                xOffset = R * Math.Cos(theta);
                zOffset = R * Math.Sin(theta);
            }

            // FoVScale accounts for current FoV angle, screenRatio accounts for the image size ratio

            double newX = (xScale + xOffset) * currentSettings.FoVScale;
            double newZ = (zScale + zOffset) * currentSettings.FoVScale * currentSettings.screenRatio;
            //  We take negative of newX because we are in a right hand co-ordinate system, so a ray sent out to the left should have a positive value for y
            Vec3 pixelVector = new Vec3(1, -newX, newZ);


            //  We want to convert the ray direction to world space, to include rotation info, but then subtract camera position as we want a direction starting at the camera
            Vec3 rayDirection = cameraOne.camSpaceToWorldSpace(pixelVector) - cameraOne.position;
            rayDirection.Normalise();

            
            return rayDirection;
        }

        void DetermineIntersections(Ray currentRay)
        {
            Shape? previousShape = currentRay.previousShape;
            //	previousShape stops us colliding with what we just hit
            currentRay.direction.Normalise();
            //  We want normalised versions
            Vec3 currPos = currentRay.position;

            bool searching = true;
            int iterationCount = 0;
            bool hitLight = false;

            Shape closestObject = null;

            while(searching)
            {
                //  Find closest surface
                //  anything below current lowest distance will be set as te new lowest distance
                double lowestDistance = currentSettings.maxJumpDistance;
                foreach (Shape currentShape in shapes)
                {
                    //  Check we aren't colliding with the same object as where we started
                    if (currentShape != previousShape)
                    {
                        double newDistance = currentShape.SDF(currPos);
                        // SDF - Signed Distance Function

                        if (newDistance < lowestDistance)
                        {
                            lowestDistance = newDistance;
                            closestObject = currentShape;
                            hitLight = false;
                        }
                    }
                }
                //  For now lights are treated (intersection wise) as the same as shape
                foreach (Shape currentLight in lights)
                {
                    if (currentLight != previousShape)
                    {
                        double newDistance = currentLight.SDF(currPos);
                        // SDF - Signed Distance Function

                        if (newDistance < lowestDistance)
                        {
                            lowestDistance = newDistance;
                            closestObject = currentLight;
                            hitLight = true;
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
                if(iterationCount > currentSettings.maxIterations || lowestDistance == currentSettings.maxJumpDistance){
                    searching = false;
                    closestObject = null;
                    hitLight=false;
                    //  NO INTERSECTION
                }
                else if(lowestDistance <= currentSettings.minJumpDistance){
                    searching = false;
                    //  INTERSECTION
                }
            }
            currentRay.hasHitLight = hitLight;
            currentRay.position = currPos;
            currentRay.previousShape = closestObject;
            currentRay.direction = null;
            //  NEED TO SORT OUT RETURN TYPE
            //  return (exitCode, currPos, closestObject);
        }

        Vec3 FindingNewRayDirection(Vec3 normal) {
            Vec3 newDir = new(rnd.NextDouble() * 2 - 1, rnd.NextDouble() * 2 - 1, rnd.NextDouble() * 2 - 1);
            double magnitude = newDir.Magnitude();
            while (magnitude > 1 || magnitude < 0.00001)
            {
                newDir = new(rnd.NextDouble() * 2 - 1, rnd.NextDouble() * 2 - 1, rnd.NextDouble() * 2 - 1);
                magnitude = newDir.Magnitude();
            }
            if (newDir * normal < 0)
            {
                newDir *= -1;
            }

            return newDir * (1 / magnitude);
        }
    
    

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            bool change = false;
            
            
            if (e.Key == Key.Left) 
            {
                camRotations[0] += distRotPerKeyPress;
            }
            else if(e.Key == Key.Right)
            {
                camRotations[0] += -distRotPerKeyPress;
            }
            else if (e.Key == Key.Up)
            {
                if(camRotations[2] <= 90 - distRotPerKeyPress)
                {
                    camRotations[2] += distRotPerKeyPress;
                }
                
            }
            else if (e.Key == Key.Down)
            {
                if(camRotations[2] >= -90 + distRotPerKeyPress)
                {
                    camRotations[2] += -distRotPerKeyPress;
                }
            }
            else if(e.Key == Key.W)
            {
                newMovement.x += distMovedPerKeyPress;
            }
            else if(e.Key == Key.D)
            {
                newMovement.y -= distMovedPerKeyPress;
            }
            else if(e.Key == Key.A)
            {
                newMovement.y += distMovedPerKeyPress;
            }
            else if(e.Key == Key.S)
            {
                newMovement.x -= distMovedPerKeyPress;
            }
            else if(e.Key == Key.LeftCtrl)
            {
                newMovement.z -= distMovedPerKeyPress;
            }
            else if(e.Key == Key.Space)
            {
                newMovement.z += distMovedPerKeyPress;
            }
            else if(e.Key == Key.D1)    //  "1" brings up updating image
            {
                MessageBox.Show("Image will start updating upon pressing Ok", "Updating");

                change = true;

                
            }
            if (change)
            {
                cameraOne.newDirection(newMovement);
                cameraOne.newRotation(camRotations);

                GenerateAllPixels();
                MessageBox.Show("Finished updating", "Updating complete");
                newMovement = new(0, 0, 0);
                change = false;
            }

            if (e.Key == Key.D2)  //  "2" key brings up save menu
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
