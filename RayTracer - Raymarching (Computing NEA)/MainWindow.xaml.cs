﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Input;
//using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RayTracer___Raymarching__Computing_NEA_
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        //  Constants for testing:



        //  Hard constants (never change)
        Random rnd = new Random();  //  For multithreaded, I would need a different random method
        Bitmap bmpFinalImage;



        //  Controls sensitivity of user controls
        double distMovedPerKeyPress = 10;
        double distRotPerKeyPress = 15;


        //  Controls initial camera sections

        static Vec3 camLocation = new Vec3(-80, -65, 90);
        double[] camRotations = new double[] { 35, 0, -25 };   //  Rotations in xy, yz, and xz planes respectively
        Vec3 newMovement = new(0, 0, 0);

        //  All Shapes
        List<Shape> shapes = new List<Shape>();

        //  All non point light sources
        List<Shape> lights = new List<Shape>();

        //  All Point Light source
        List<PointLight> lightPoints = new List<PointLight>();

        //  Settings for each image
        SettingInfo currentSettings = new(

                res_x: 100,
                res_y: 80,
                rayCountPerPixel: 2000,

                maxIterations: 400,
                maxJumpDistance: 300,
                minJumpDistance: 0.01d,

                maxBounceCount: 20,
                startOffset: 0.3,

                lightArea: 14,  //  Higher is sharper shadows from point light sources

                isAntiAliasing: true,
                AA_Strength: 0.005d,
                FoVangle: 100

                );


        Camera cameraOne;

        public struct SettingInfo
        {

            //  High performance impact
            public int res_x;
            public int res_y;
            public int rayCountPerPixel; //  Rays sent out for each pixel

            //  Unkown/Medium performance impact
            //  Controls cutoff and precision the ray-marching uses
            public int maxIterations;
            public double maxJumpDistance;
            public double minJumpDistance;
            public int maxBounceCount;
            public double startOffset;

            //  Low performance impact
            public bool isAntiAliasing;
            public double AA_Strength;
            public double FoVangle;
            public double lightArea; // Used in soft shadows for point light sources

            //  Precomputed
            public double screenRatio;
            public double FoVScale;
            public SettingInfo(int res_x, int res_y, int rayCountPerPixel, int maxIterations, double maxJumpDistance, double minJumpDistance, int maxBounceCount, double startOffset, bool isAntiAliasing, double AA_Strength, double lightArea, double FoVangle)
            {
                this.res_x = res_x;
                this.res_y = res_y;
                this.rayCountPerPixel = rayCountPerPixel;

                this.maxIterations = maxIterations;
                this.maxJumpDistance = maxJumpDistance;
                this.minJumpDistance = minJumpDistance;
                this.maxBounceCount = maxBounceCount;
                this.startOffset = startOffset;
                this.lightArea = lightArea;

                this.isAntiAliasing = isAntiAliasing;
                this.AA_Strength = AA_Strength;
                this.FoVangle = FoVangle;

                //  Precalculated constants, used in assinging initial ray direction given a start pixel
                this.screenRatio = (double)this.res_y / (double)this.res_x;
                this.FoVScale = Math.Tan(this.FoVangle * (Math.PI / 180) / 2);  //  FoVangle is in degrees, and must be converted to radians


            }
        }

        public struct IntersectionInfo  //  Used as the return type for DetermineIntersections
        {
            public bool hasHitLight;
            public Vec3 position = new Vec3(0, 0, 0);
            public Shape previousShape;

            public double distance;
            public double shadowContribution;

            public IntersectionInfo(bool hasHitLight, Vec3 position, Shape previousShape, double distance, double shadowContribution)
            {

                this.hasHitLight = hasHitLight;
                this.position = position;
                this.previousShape = previousShape;
                this.distance = distance;
                this.shadowContribution = shadowContribution;
            }
        }

        public MainWindow()
        {

            InitializeComponent();

            //  Constant initialisation

            bmpFinalImage = new Bitmap(currentSettings.res_x, currentSettings.res_y);

            //  Shape and camera initialisation

            cameraOne = new Camera(camLocation, camRotations);

            #region Testing shapes:
            //  First sphere
            //Vec3 pos1 = new Vec3(100, -50, 0);
            //Vec3 k_s1 = new Vec3(0.1, 0.8, 0.9);
            //Vec3 k_d1 = new Vec3(0.9, 0.2, 0.1);
            //double alpha1 = 2;
            //double radius1 = 15;
            //Vec3 lightStrength1 = 10 * new Vec3(0.1, .1, 1);
            //lights.Add(new Sphere(pos1, k_s1, k_d1, alpha1, radius1, lightStrength1));



            //  Fourth sphere
            //Vec3 pos4 = new Vec3(50, 0, 30);
            //Vec3 k_s4 = new Vec3(0, 0, 0);
            //Vec3 k_d4 = new Vec3(1,1, 1);
            //double alpha4 = 14;
            //double radius4 = 7.5;
            //Vec3 lightStrength4 = 75*new Vec3(1, 1, 1);
            //lights.Add(new Sphere(pos4, k_s4, k_d4, alpha4, radius4, lightStrength4));

            //  Fifth sphere 105,76,179
            //Vec3 pos5 = new Vec3(25, -25, 20);
            //Vec3 k_s5 = 1 * new Vec3(0.3, 0.2, 0.9);
            //Vec3 k_d5 = 1 * new Vec3(0.4, 0.3, 0.8);
            //double alpha5 = 3;
            //double radius5 = 10;
            //shapes.Add(new Sphere(pos5, k_s5, k_d5, alpha5, radius5));



            //  Tenth Sphere
            //shapes.Add(new Sphere(
            //    position: new Vec3(60, 40, 140)
            //    , specularComponent: new Vec3(.1, .1, .9)
            //    , diffuseComponent: new Vec3(0.9, .9, 0.1)
            //    , alpha: 6
            //    , radius: 50
            //    ));


            //  Combo sphere 1
            //Shape comboSphere1 = new Sphere(
            //    position: new Vec3(45, -25, 25)
            //    , specularComponent: new Vec3(.9, 0.9, 0)
            //    , diffuseComponent: new Vec3(.1, .1, 1)
            //    , alpha: 6
            //    , radius: 32
            //    );

            ////  Combo Sphere 2
            //Shape comboSphere2 = new Sphere(
            //    position: new Vec3(45, -35, 60)
            //    , specularComponent: new Vec3(0, .9, .9)
            //    , diffuseComponent: new Vec3(1, .1, 0.1)
            //    , alpha: 6
            //    , radius: 20
            //    );
            //  First Combo sphere
            //shapes.Add(new Combination(
            //    specularComponent: new Vec3(.9, 0, .9),
            //    diffuseComponent: new Vec3(.1, 1, .1),
            //    alpha: 6,
            //    shape1: comboSphere1,
            //    shape2: comboSphere2,
            //    sdfWeighting: 0.7,
            //    type: comboType.Union



            //    ));

            ////  First Point light source
            //lightPoints.Add(new PointLight(
            //    position: new Vec3(-20, -60, 60),
            //    lightColour: new Vec3(1, 1, 1),
            //    lightBrightness: 5

            //    ));

            ////  Second Point light source
            //lightPoints.Add(new PointLight(
            //    position: new Vec3(130, 80, 150),
            //    lightColour: new Vec3(1, 1, 1),
            //    lightBrightness: 2

            //    ));

            //  Infinite sphere
            //shapes.Add(new InfiniteSphere(
            //    position: new Vec3(60, 40, 140)
            //    , specularComponent: new Vec3(.7, .1, .9)
            //    , diffuseComponent: new Vec3(0.3, 0.9, 0.1)
            //    , alpha: 6
            //    , radius: 25
            //    , repetitionVector: new Vec3(100, 100, 100)
            //    ));

            //shapes.Add(new InfiniteSphere(
            //    position: new Vec3(50, 50, 50)
            //    , specularComponent: new Vec3(.7, .9, .1)
            //    , diffuseComponent: new Vec3(0.3, 0.1, 0.9)
            //    , alpha: 6
            //    , radius: 5
            //    , repetitionVector: new Vec3(100, 100, 200)
            //    ));

            //  First line
            //lights.Add(new Line(
            //    specularComponent: new Vec3(0.9, .9, .3),
            //    diffuseComponent: new Vec3(0.1, 0.1, 0.7),
            //    alpha: 6,
            //    pointA: new Vec3(50,-90,60),
            //    pointB: new Vec3(50,60,70),
            //    radius: 30,
            //    lightStrength: 20 * new Vec3(.5, 0.2, 0.8),
            //    haltB: false

            //    ));

            //  Bottom plane
            //shapes.Add(new Plane(
            //    pointOnPlane: new Vec3(0, 0, 0),
            //    specularComponent: new Vec3(0.3, .3, .9),
            //    diffuseComponent: new Vec3(.7, .7, .1),
            //    alpha: 6,
            //    normal: new Vec3(0, 0, 1)
            //    ));

            //  Rightside plane

            //shapes.Add(new Plane(
            //    pointOnPlane: new Vec3(0, -60, 0),
            //    specularComponent: new Vec3(0.7, .3, .9),
            //    diffuseComponent: new Vec3(.3, .7, .1),
            //    alpha: 6,
            //    normal: new Vec3(0, 1, 0)
            //    ));

            //  Back plane

            //shapes.Add(new Plane(
            //    pointOnPlane: new Vec3(200, 0, 0),
            //    specularComponent: new Vec3(0.5, .9, .5),
            //    diffuseComponent: new Vec3(.5, .1, .5),
            //    alpha: 6,
            //    normal: new Vec3(-1, 0, 0)
            //    ));

            //  Box One
            //shapes.Add(new Cuboid(
            //    position: new Vec3(-60, 0, 70),
            //    cornerPosition: new Vec3(10, 10, 20),
            //    fullRotationInfo: new double[] {90, 45, 0},
            //    diffuseComponent: new Vec3(.5, .9, .6),
            //    specularComponent: new Vec3(.5, .1, .4),
            //    alpha: 6,
            //    cornerSmoothing: 4
            //    ));

            #endregion

            #region Table scene

            ////  Lights

            //lights.Add(new Line(
            //    pointA: new Vec3(0, 55, 80),
            //    pointB: new Vec3(0, 55, 130),
            //    radius: 10,
            //    lightStrength: 30 * new Vec3(1, 1, 1),
            //    diffuseComponent: null,
            //    alpha: 6
            //    ));

            //lights.Add(new Line(
            //    pointA: new Vec3(0, -55, 80),
            //    pointB: new Vec3(0, -55, 130),
            //    radius: 10,
            //    lightStrength: 30 * new Vec3(1, 1, 1),
            //    diffuseComponent: null,
            //    alpha: 6
            //    ));

            ////  Shapes

            ////  Floor plane
            //shapes.Add(new Plane(
            //    position: new Vec3(0, 0, 0),
            //    normal: new Vec3(0, 0, 1),
            //    diffuseComponent: new Vec3(0.43, 0.31, 0),
            //    alpha: 6,
            //    specularComponent: new Vec3(0.3, 0.3, 0.5)
            //    ));

            ////  Back wall
            //shapes.Add(new Plane(
            //    position: new Vec3(200, 0, 0),
            //    normal: new Vec3(-1, 0, 0),
            //    diffuseComponent: new Vec3(0.90, 0.86, 0.59),
            //    alpha: 6,
            //    specularComponent: new Vec3(0.05, 0.07, 0.2)
            //    ));

            ////  Side wall
            //shapes.Add(new Plane(
            //    position: new Vec3(0, -300, 0),
            //    normal: new Vec3(0, 1, 0),
            //    diffuseComponent: new Vec3(0.90, 0.86, 0.59),
            //    alpha: 6,
            //    specularComponent: new Vec3(0.05, 0.07, 0.2)
            //    ));

            ////  Carpet

            //shapes.Add(new Cuboid(
            //    position: new Vec3(0, 0, 0.4),
            //    cornerPosition: new Vec3(50, 150, 0.5),
            //    cornerSmoothing: 0.5,
            //    alpha: 6,
            //    diffuseComponent: new Vec3(0.78, 0.31, 0.31),
            //    specularComponent: new Vec3(0, 0, 0)
            //    ));

            ////  Table top
            //shapes.Add(new Cuboid(
            //    position: new Vec3(0, 0, 54.8),
            //    cornerPosition: new Vec3(50, 75, 4.1),
            //    cornerSmoothing: 0.1,
            //    alpha: 6,
            //    diffuseComponent: new Vec3(.54, .51, .33)
            //    ));

            ////  Table leg One   -   These really should be done as the same object and repeated
            //shapes.Add(new Cuboid(
            //    position: new Vec3(45, 65, 25.8),
            //    cornerPosition: new Vec3(3, 3, 25),
            //    diffuseComponent: new Vec3(.54, .51, .33),
            //    cornerSmoothing: 0.5,
            //    alpha: 6
            //    ));

            ////  Table leg Two   -   These really should be done as the same object and repeated
            //shapes.Add(new Cuboid(
            //    position: new Vec3(45, -65, 25.8),
            //    cornerPosition: new Vec3(3, 3, 25),
            //    diffuseComponent: new Vec3(.54, .51, .33),
            //    cornerSmoothing: 0.5,
            //    alpha: 6
            //    ));

            ////  Table leg Three   -   These really should be done as the same object and repeated
            //shapes.Add(new Cuboid(
            //    position: new Vec3(-45, 65, 25.8),
            //    cornerPosition: new Vec3(3, 3, 25),
            //    diffuseComponent: new Vec3(.54, .51, .33),
            //    cornerSmoothing: 0.5,
            //    alpha: 6
            //    ));

            ////  Table leg Four   -   These really should be done as the same object and repeated
            //shapes.Add(new Cuboid(
            //    position: new Vec3(-45, -65, 25.8),
            //    cornerPosition: new Vec3(3, 3, 25),
            //    diffuseComponent: new Vec3(.54, .51, .33),
            //    cornerSmoothing: 0.5,
            //    alpha: 6
            //    ));

            ////  Sphere on table
            //shapes.Add(new Sphere(
            //    position: new Vec3(015, -10, 68.8),
            //    radius: 10,
            //    alpha: 6,
            //    diffuseComponent: new Vec3(0, 0.75, 1)
            //    ));

            #endregion

            #region Tree test
            //  Hasn't really worked, though i haven't had that long to test it either though
            //shapes.Add(new DistortedSphere(
            //    position: new Vec3(0, 0, 60),
            //    radius: 40,

            //    alpha: 6,
            //    diffuseComponent: new Vec3(.2, .55, .31),
            //    specularComponent: new Vec3(0.2, .2, .2)

            //    ));

            //lightPoints.Add(new PointLight(
            //    position: new Vec3(30, -90, 90),
            //    lightColour: new Vec3(1, 1, 1),
            //    lightBrightness: 100
            //    ));

            #endregion

            #region Spheres with light scene

            ////  Sphere 1 - Light
            //lights.Add(new Sphere(
            //    position: new Vec3(3, -3, 15),
            //    radius: 4,
            //    alpha: 6,
            //    lightStrength: 15 * new Vec3(0, 1, 1),
            //    diffuseComponent: null
            //    ));

            ////  Sphere 2
            //shapes.Add(new Sphere(
            //    position: new Vec3(4, 1, 19),
            //    radius: 4.5,
            //    alpha: 2,
            //    diffuseComponent: new Vec3(0.75, 0.75, .75)
            //    ));

            ////  Sphere 3 - Light
            //lights.Add(new Sphere(
            //    position: new Vec3(0, 2, 17),
            //    radius: 3.7,
            //    alpha: 10,
            //    lightStrength: 15 * new Vec3(.64, 0, 1),
            //    diffuseComponent: null
            //    ));

            ////  Sphere 4
            //shapes.Add(new Sphere(
            //    position: new Vec3(-1, -1, 23),
            //    radius: 4.2,
            //    alpha: 4,
            //    diffuseComponent: new Vec3(0.08, 0.66, 0)
            //    ));

            ////  Sphere 5
            //shapes.Add(new Sphere(
            //    position: new Vec3(-3, -3, 17),
            //    radius: 5.1,
            //    alpha: 6,
            //    diffuseComponent: new Vec3(1, .43, .43)
            //    ));

            //// Sphere 6 - Light
            //lights.Add(new Sphere(
            //    position: new Vec3(015, -10, 68.8),
            //    radius: 3.2,
            //    alpha: 6,
            //    lightStrength: 15*new Vec3(1, 0.8, .85),
            //    diffuseComponent: null
            //    ));


            //// Stalk/Capsule 1
            //Line stalk = new Line(
            //    pointA: new Vec3(0, 0, 0),
            //    pointB: new Vec3(0, 0, 20),
            //    diffuseComponent: new Vec3(.8, .8, .8),
            //    radius: 2.5,
            //    alpha: 5

            //    );

            ////  Floor plane
            //Plane floorPlane = new Plane(
            //    position: new Vec3(0, 0, 0),
            //    normal: new Vec3(0, 0, 1),
            //    diffuseComponent: new Vec3(.7, .7, .7),
            //    alpha: 5

            //    );

            //shapes.Add(new Combination(
            //    alpha: 5,
            //    shape1: stalk,
            //    shape2: floorPlane,
            //    sdfWeighting: 0.2,
            //    type: comboType.Union
            //    ));

            ////  Point light source
            //lightPoints.Add(new PointLight(
            //    position: new Vec3(-10, -30, 50),
            //    lightColour: new Vec3(1, 1, 1),
            //    lightBrightness: 1
            //    ));

            #endregion

            #region Showing off phong model

            ////  Sphere 1 - Light
            //lights.Add(new Sphere(
            //    position: new Vec3(15, -15, 50),
            //    radius: 15,
            //    alpha: 6,
            //    lightStrength: 15 * new Vec3(1, 1, 1),
            //    diffuseComponent: null
            //    ));

            ////  Sphere 2
            //shapes.Add(new Sphere(
            //    position: new Vec3(8, 15, 30),
            //    radius: 13,
            //    alpha: 6,
            //    diffuseComponent: new Vec3(.6, 0, .6)
            //    ));


            ////  Floor plane
            //shapes.Add(new Plane(
            //    position: new Vec3(0, 0, 0),
            //    normal: new Vec3(0, 0, 1),
            //    diffuseComponent: new Vec3(0, 0.7, 1),
            //    alpha: 6
            //    //specularComponent: new Vec3(0.3, 0.3, 0.5)
            //    ));

            ////  Back wall
            //shapes.Add(new Plane(
            //    position: new Vec3(100, 0, 0),
            //    normal: new Vec3(-1, 0, 0),
            //    diffuseComponent: new Vec3(.54, .44, 1),
            //    alpha: 6
            //    //specularComponent: new Vec3(0.05, 0.07, 0.2)
            //    ));

            ////  Side wall
            //shapes.Add(new Plane(
            //    position: new Vec3(0, 34, 0),
            //    normal: new Vec3(0, -1, 0),
            //    diffuseComponent: new Vec3(.4, .3, 1),
            //    alpha: 6
            //    //specularComponent: new Vec3(0.05, 0.07, 0.2)
            //    ));


            #endregion

            #region Showing individual SDFs

            //  Sphere on plane background:
            //  Floor plane
            shapes.Add(new Plane(
                position: new Vec3(0, 0, 0),
                normal: new Vec3(0, 0, 1),
                diffuseComponent: 1 * new Vec3(.4, .54, 0),
                alpha: 20
                //specularComponent: new Vec3(0.3, 0.3, 0.5)
                ));

            //  Sphere 2
            //Sphere comboSphere1 = new Sphere(
            //    position: new Vec3(0, 0, 30),
            //    radius: 20,
            //    alpha: 10,
            //    diffuseComponent: new Vec3(.6, 0, .8)
            //    );

            //  Sphere 3
            lights.Add(new Line(
                pointA: new Vec3(-5, 30, 50),
                pointB: new Vec3(30, 10, 40),
                alpha: 10,
                radius: 16,
                lightStrength: 15 * new Vec3(1, 1, 1),
                diffuseComponent: null
                ));

            ////shapes.Add(comboSphere1);
            ////shapes.Add(combo2 );
            ////  First Combo
            //shapes.Add(new Combination(
            //    //specularComponent: new Vec3(.9, 0, .9),
            //    //diffuseComponent: new Vec3(.1, 1, .1),
            //    alpha: 20,
            //    shape1: comboSphere1,
            //    shape2: combo2,
            //    sdfWeighting: 0.7,
            //    type: comboType.Union
            //    ));

            //  Cuboid
            //shapes.Add(new Cuboid(
            //    position: new Vec3(5, 5, 40),
            //    cornerPosition: new Vec3(8, 20, 6),
            //    fullRotationInfo: new double[] { 0, 20, 55 },
            //    alpha: 100,
            //    diffuseComponent: 1 * new Vec3(.55, 1, .41),
            //    specularComponent: 0 * new Vec3(1, .4, .4),
            //    cornerSmoothing: 8
            //    ));

            ////  Light points
            //lightPoints.Add(new PointLight(
            //    position: new Vec3(-50, -40, 50),
            //    lightColour: new Vec3(1, 1, 1),
            //    lightBrightness: 2
            //    ));
            #endregion




            //  Main loop
            Stopwatch timer = Stopwatch.StartNew();
            GenerateAllPixels();
            timer.Stop();
            double secOverallTime = timer.ElapsedMilliseconds / 1000;

            //  Assume overall time, T is given by T = res_x * res_y * rayCountPerPixel * meanTimePerRay, for predicted time calculations
            double totalRayCount = currentSettings.res_x * currentSettings.res_y * currentSettings.rayCountPerPixel;
            double meanTimePerRay = secOverallTime / totalRayCount;
            MessageBox.Show("There were " + totalRayCount + " ray(s) in total, taking " + secOverallTime + " seconds in total, with a mean time of " + 1000 * meanTimePerRay + " milliseconds per ray", "Timing info");

        }

        void GenerateAllPixels()
        {
            //  Loops through each pixel, generating rays to find the pixel's colour
            for (int x = 0; x < currentSettings.res_x; x++)
            {
                for (int y = 0; y < currentSettings.res_y; y++)
                {

                    Color pixelColor = GetPixelColor(x, currentSettings.res_y - y);

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
            //  This will be added to by each ray to track the contributions of each ray towards the pixels colour
            Vec3 finalColor = new(0, 0, 0);
            int pseudoRayCount = 0;

            //  Loop through to run a ray calculation as many times as needed
            for (int i = 0; i < currentSettings.rayCountPerPixel; i++)
            {

                //  Based of the pixels co-ordinate, finds the ray direction
                Vec3 rayDirection = FindPixelsRayDirection(x, y);
                Ray currentRay = new Ray(cameraOne.position, rayDirection);


                bool checkForNewIntersections = true;

                //  Checks if ray has bounced more than the maximun count
                //  Checks if ray is still looking for new intersections eg if it hit a light or hit nothing at all

                for (int j = 0; j < currentSettings.maxBounceCount && checkForNewIntersections; j++)
                {
                    Vec3 initialDirection = currentRay.direction;

                    //  Calculates what object (if any) the ray hits, using the direction last calculated
                    IntersectionInfo intersectionReturnInfo0 = DetermineIntersections(currentRay);

                    //  We won't need to update all these class variables in some cases of running determine intersections, so we handle it seperately
                    currentRay.hasHitLight = intersectionReturnInfo0.hasHitLight;
                    currentRay.position = intersectionReturnInfo0.position;
                    currentRay.previousShape = intersectionReturnInfo0.previousShape;
                    currentRay.direction = null;

                    //  Result of collision checks is saved to currentRay

                    //  If we have hit an object and it isn't a light, we work out the colour of the object and account for point light sources and send out a new ray
                    if (currentRay.previousShape != null && !currentRay.hasHitLight)
                    {
                        //  Get the normal to the shape at the intersection point
                        Vec3 normal = currentRay.previousShape.FindNormal(currentRay.position);

                        //  Accounting for Point Light sources:

                        foreach (PointLight currLight in lightPoints)
                        {
                            //  Determine intersections changes these variables, but we don't want that (yet)

                            //  Check occlusion
                            Vec3 currPosToLight = currLight.position - currentRay.position;
                            double lightDistance = currPosToLight.Magnitude();

                            currentRay.direction = currPosToLight;
                            IntersectionInfo intersectionReturnInfo1 = DetermineIntersections(currentRay, lightDistance);
                            double intersectionDistance = intersectionReturnInfo1.distance;
                            Shape shapeIntersected = intersectionReturnInfo1.previousShape;


                            double shadowContribution = intersectionReturnInfo1.shadowContribution;

                            //  If distance to nearest shape is further than the light, then the ray will reach the light source unoccluded
                            //  If the ray intersects nothing then the ray reaches the light source as well
                            if (lightDistance < intersectionDistance || shapeIntersected == null)
                            {
                                Vec3 finalShapeReflectance = currentRay.previousShape.BRDF_phong(currPosToLight, initialDirection, normal);
                                Vec3 tmpProductOfReflectance = Vec3.ColorCombination(finalShapeReflectance, currentRay.productOfReflectance);
                                Vec3 pointLightContribution = Vec3.ColorCombination(tmpProductOfReflectance, currLight.lightStrength);
                                finalColor += pointLightContribution * shadowContribution;
                            }

                        }
                        //  Find new direction and that affect on the reflectance
                        currentRay.direction = FindingNewRayDirection(normal);
                        Vec3 shapeReflectance = currentRay.previousShape.BRDF_phong(currentRay.direction, initialDirection, normal);
                        currentRay.productOfReflectance = Vec3.ColorCombination(shapeReflectance, currentRay.productOfReflectance);
                    }
                    else
                    {
                        //  If we have hit a light or the sky we start our final pixel calculations
                        checkForNewIntersections = false;

                        //  Get colour from lights
                        Vec3 lightStrength = new(0, 0, 0);
                        Vec3 lighting = new(0, 0, 0);
                        if (currentRay.previousShape != null && currentRay.hasHitLight)
                        {
                            //  If we hit a light then we add that lighting to finalColor
                            lightStrength = currentRay.previousShape.lightStrength;
                        }
                        if (currentRay.previousShape == null)
                        {
                            //  Simulate a sun and skyline
                            double sunMagnitude = 10 * Math.Pow(Math.Max(initialDirection * new Vec3(1, 0, 0), 0), 128);
                            Vec3 sunColour = 0 * new Vec3(1, 0.8, 0.4); //  Set to zero for no sun
                            double skyMagnitude = Math.Pow(Math.Max(initialDirection * new Vec3(0, 0, 1), 0), 0.4);
                            Vec3 skyColour = 1 * new Vec3(0.4, .65, 1);
                            Vec3 ambientColour = 1 * new Vec3(0.1, 0.1, 0.1);
                            lighting = sunMagnitude * sunColour + skyMagnitude * skyColour + ambientColour;
                        }


                        finalColor += Vec3.ColorCombination(currentRay.productOfReflectance, lighting + lightStrength);
                    }
                    pseudoRayCount++;

                }
            }
            finalColor *= 1 / (double)currentSettings.rayCountPerPixel;

            return Color.FromArgb((int)Math.Min(255 * finalColor.x, 255), (int)Math.Min(finalColor.y * 255, 255), (int)Math.Min(finalColor.z * 255, 255));
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
            if (currentSettings.isAntiAliasing == true)
            {
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

        IntersectionInfo DetermineIntersections(Ray currentRay, double maxTravelDistance = double.MaxValue)
        {




            currentRay.direction.Normalise();
            //  We want normalised versions
            Vec3 currPos = currentRay.position + currentRay.direction * currentSettings.startOffset;

            bool searching = true;
            int iterationCount = 0;
            bool hitLight = false;

            //  Distance not used, but provides a point to check against
            double distance = 0;
            double shadowContribution = 1;


            Shape closestObject = null;

            while (searching)
            {
                //  Find closest surface
                //  anything below current lowest distance will be set as te new lowest distance
                double lowestDistance = currentSettings.maxJumpDistance;



                foreach (Shape currentShape in shapes)
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
                //  Lights are treated (intersection wise) as the same as shapes
                //  They are handled in a seperate loop as we want to different things if we hit one
                foreach (Shape currentLight in lights)
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
                //  Iteration counts prevents iteration going on forever
                iterationCount++;

                //  Dist to closest surface used to find new position
                currPos += currentRay.direction * lowestDistance;
                distance += lowestDistance;

                double newShadowContribution = currentSettings.lightArea * lowestDistance / distance;

                //  We want to track and find the greatest shadow
                if (shadowContribution > newShadowContribution)
                {
                    shadowContribution = newShadowContribution;
                }
                //  Tolerance check:
                //  Have we travelled further than the max jump distance?
                //  Have we gone through too many iterations?
                if (iterationCount > currentSettings.maxIterations || lowestDistance == currentSettings.maxJumpDistance || distance > maxTravelDistance)
                {
                    searching = false;
                    closestObject = null;
                    hitLight = false;
                    //  NO INTERSECTION
                }
                else if (lowestDistance <= currentSettings.minJumpDistance)
                {
                    searching = false;
                    //  INTERSECTION
                }
            }

            IntersectionInfo intersectionReturnInfo = new IntersectionInfo(
                hasHitLight: hitLight,
                position: currPos,
                previousShape: closestObject,
                distance: distance,
                shadowContribution: shadowContribution
                );

            return intersectionReturnInfo;
        }

        Vec3 FindingNewRayDirection(Vec3 normal)
        {
            //  Generates a point in a box
            Vec3 newDir = new(rnd.NextDouble() * 2 - 1, rnd.NextDouble() * 2 - 1, rnd.NextDouble() * 2 - 1);
            double magnitude = newDir.Magnitude();

            //  Rejects points outside the defined circle, and points that could cause division by zero errors
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
            bool startUpdatingImage = false;

            //  Movement and camera rotation controls
            if (e.Key == Key.Left)
            {
                camRotations[0] += distRotPerKeyPress;
            }
            else if (e.Key == Key.Right)
            {
                camRotations[0] += -distRotPerKeyPress;
            }
            else if (e.Key == Key.Up)
            {
                if (camRotations[2] <= 90 - distRotPerKeyPress)
                {
                    camRotations[2] += distRotPerKeyPress;
                }

            }
            else if (e.Key == Key.Down)
            {
                if (camRotations[2] >= -90 + distRotPerKeyPress)
                {
                    camRotations[2] += -distRotPerKeyPress;
                }
            }
            else if (e.Key == Key.W)
            {
                newMovement.x += distMovedPerKeyPress;
            }
            else if (e.Key == Key.D)
            {
                newMovement.y -= distMovedPerKeyPress;
            }
            else if (e.Key == Key.A)
            {
                newMovement.y += distMovedPerKeyPress;
            }
            else if (e.Key == Key.S)
            {
                newMovement.x -= distMovedPerKeyPress;
            }
            else if (e.Key == Key.LeftCtrl)
            {
                newMovement.z -= distMovedPerKeyPress;
            }
            else if (e.Key == Key.Space)
            {
                newMovement.z += distMovedPerKeyPress;
            }
            else if (e.Key == Key.D1)    //  "1" brings up updating image
            {
                MessageBox.Show("Image will start updating upon pressing Ok", "Updating");

                startUpdatingImage = true;


            }
            if (startUpdatingImage)
            {
                cameraOne.newDirection(newMovement);
                cameraOne.newRotation(camRotations);

                GenerateAllPixels();
                MessageBox.Show("Finished updating", "Updating complete");
                newMovement = new(0, 0, 0);
                startUpdatingImage = false;
            }

            if (e.Key == Key.D2)  //  "2" key brings up save menu
            {
                DateTime time = DateTime.Now;
                string fileName = time.Year + "." + time.Month + "." + time.Day + "." + time.Hour + "." + time.Minute + "_RayTracer_RaysPerPixel_" + currentSettings.rayCountPerPixel + "_" + time.Millisecond + ".png";
                MessageBoxResult result = MessageBox.Show("Do you want to save your image?\n" + fileName, "Saving", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    bmpFinalImage.Save("..\\..\\..\\..\\Images\\" + fileName, ImageFormat.Png);
                    //bmpFinalImage.Save("TEST.png", ImageFormat.Png);

                }
            }

            if (e.Key == Key.D3)
            {
                //  Shows current camera position and rotation
                string camPosAsString = "(" + cameraOne.position.x + ", " + cameraOne.position.y + ", " + cameraOne.position.z + ")";
                string camRotAsString = "(" + camRotations[0] + ", " + camRotations[1] + ", " + camRotations[2] + ")";

                MessageBox.Show("Camera pos is: " + camPosAsString + "\nCamera rotation is: " + camRotAsString, "Camera Info");
            }


        }
    }


}
