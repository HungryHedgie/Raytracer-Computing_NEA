﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer___Raymarching__Computing_NEA_
{
    public enum comboType
    {
        Union,
        Intersection,
        Exclusion




    }
    
    abstract public class Shape
    {
        public abstract double SDF(Vec3 rayLocation);
        public abstract Vec3 FindNormal(Vec3 rayLocation);
        public Vec3 position;
        
        public Vec3 k_s;
        public Vec3 k_d;
        public double alpha;

        public Vec3 lightStrength = new Vec3(0, 0, 0);

        public Shape(Vec3 position, Vec3 specularComponent, Vec3 diffuseComponent, double alpha, Vec3 lightStrength = null) //  Need to change
        {
            //  General info all shapes will need
            this.position = position;   
            this.k_s = specularComponent;
            this.k_d = diffuseComponent;
            this.alpha = alpha;

            if(lightStrength == null)
            {
                this.lightStrength = new(0, 0, 0);
            }
            else
            {
                this.lightStrength = lightStrength;
            }
        }

        public Vec3 BRDF_phong(Vec3 omega_i, Vec3 omega_o, Vec3 normal)
        {
            //  NOT SAME AS PSEUDOCODE
            omega_o *= -1;  //  Added line, accounts for that we want the direction to heading outwards
            omega_i.Normalise();    //  Incoming light (From a physics perspective)
            omega_o.Normalise();    //  Outgoing from a physics perspective, so these are the opposite of the order we got our rays
            normal.Normalise();

            //  Max is in place in case the dot product is negative
            Vec3 reflectedRay = 2 * Math.Max(omega_i * normal, 0) * normal - omega_i;

            //  Reflectance is the sum of the seperate components of specular and diffuse reflection
            Vec3 specularComponent = this.k_s * Math.Pow(Math.Max(omega_o * reflectedRay, 0), this.alpha);
            Vec3 reflectanceComponent = this.k_d * Math.Max(omega_o * normal, 0);

            //  The dot product term accounts for the shallow angles, which have a lower contribution
            Vec3 reflectance = (specularComponent + reflectanceComponent) * Math.Max(normal * omega_i, 0);

            //  Reflectance is in the form (R, G, B)
            //  Each component is how much of the colour is reflected
            return reflectance;
        }

        public Vec3 FindNormalNumerically(Vec3 rayLocation)
        {
            //  Hardcoded precision
            double epsilon = 0.0001;
            Vec3 normal = 1 / (2 * epsilon) * new Vec3(
                this.SDF(new Vec3(rayLocation.x + epsilon, rayLocation.y, rayLocation.z)) - this.SDF(new Vec3(rayLocation.x - epsilon, rayLocation.y, rayLocation.z)),
                this.SDF(new Vec3(rayLocation.x, rayLocation.y + epsilon, rayLocation.z)) - this.SDF(new Vec3(rayLocation.x, rayLocation.y - epsilon, rayLocation.z)),
                this.SDF(new Vec3(rayLocation.x, rayLocation.y, rayLocation.z + epsilon)) - this.SDF(new Vec3(rayLocation.x, rayLocation.y, rayLocation.z - epsilon)));
            normal.Normalise();
            return normal;
        }

    }

    class Sphere : Shape
    {
        double radius {get; set;}
        public Sphere(Vec3 position, Vec3 specularComponent, Vec3 diffuseComponent, double alpha, double radius, Vec3 lightStrength = null) : base (position, specularComponent, diffuseComponent, alpha, lightStrength)
        {
            //  Only extra info that a sphere needs is the radius
            this.radius = radius;

        }

        public override double SDF(Vec3 rayLocation)
        {
            double signedDistance =  Math.Sqrt(Math.Pow((rayLocation.x - position.x), 2) + Math.Pow((rayLocation.y - position.y), 2) + Math.Pow((rayLocation.z - position.z), 2)) - radius;
            //double signedDistance = rayLocation.x - position.x + rayLocation.y - position.y - radius; (Manhattan distance, didn't work)
            return signedDistance;    //  Absolute should enable rendering from inside the sphere - Did not work, rays cannot intersect with shape they started in
        }

        

        public override Vec3 FindNormal(Vec3 rayLocation)
        {
            //Vec3 normal = FindNormalNumerically(rayLocation);
            Vec3 normal = rayLocation - this.position;
            normal.Normalise();
            return normal;
        }
    }

    

    class Plane : Shape
    {
        Vec3 pointOnPlane { get; set; }
        Vec3 normal { get; set; }
        public Plane(Vec3 position, Vec3 specularComponent, Vec3 diffuseComponent, double alpha, Vec3 pointOnPlane, Vec3 lightStrength = null) : base(position, specularComponent, diffuseComponent, alpha, lightStrength)
        {

            this.pointOnPlane = pointOnPlane;
            this.normal = Vec3.Normalise(normal);

        }

        public override double SDF(Vec3 rayLocation)
        {
            double signedDistance = rayLocation * this.normal - this.pointOnPlane * this.normal;
            return signedDistance;    
        }

        public override Vec3 FindNormal(Vec3 rayLocation)
        {
            return this.normal;
        }
    }

    class Line : Shape  //  NOT FINISHED YET
    {
        //  Radius is how much area around the line is taken up
        double radius { get; set; }
        public Line(Vec3 position, Vec3 specularComponent, Vec3 diffuseComponent, double alpha, double radius, Vec3 lightStrength = null) : base(position, specularComponent, diffuseComponent, alpha, lightStrength)
        {

            this.radius = radius;

        }

        public override double SDF(Vec3 rayLocation)
        {
            double signedDistance = Math.Sqrt(Math.Pow((rayLocation.x - position.x), 2) + Math.Pow((rayLocation.y - position.y), 2) + Math.Pow((rayLocation.z - position.z), 2)) - radius;
            //double signedDistance = rayLocation.x - position.x + rayLocation.y - position.y - radius; (Manhattan distance, didn't work)
            return signedDistance;    //  Absolute should enable rendering from inside the sphere - Did not work, rays cannot intersect with shape they started in
        }

        public override Vec3 FindNormal(Vec3 rayLocation)
        {
            Vec3 normal = rayLocation - this.position;
            normal.Normalise();
            return normal;
        }
    }

    class Combination : Shape       //  Not implemented yet, normal needs some work to be done
    {
        public Shape shape1;
        public Shape shape2;

        public double sdfMergeStrength;    //  Weighting for how smooth combination is
        public double colourMergeStrength;
        public comboType type;
        public Combination(Vec3 specularComponent, Vec3 diffuseComponent, double alpha, Shape shape1, Shape shape2, double sdfWeighting, comboType type, double colourMergeStrength = 15, Vec3 lightStrength = null, Vec3 position = null) : base(position, specularComponent, diffuseComponent, alpha, lightStrength)
        {

            this.shape1 = shape1;
            this.shape2 = shape2;
            this.sdfMergeStrength = sdfWeighting;
            this.colourMergeStrength = colourMergeStrength;
            this.type = type;
            

        }

        public override double SDF(Vec3 rayLocation)
        {
            double signedDistance;
            double shape1Dist = shape1.SDF(rayLocation);
            double shape2Dist = shape2.SDF(rayLocation);
            if (type == comboType.Union)
            {
                signedDistance = -Math.Log(Math.Exp(-sdfMergeStrength * shape1Dist) + Math.Exp(-sdfMergeStrength * shape2Dist)) / sdfMergeStrength;
            }
            else if(type == comboType.Intersection)
            {
                signedDistance = Math.Log(Math.Exp(sdfMergeStrength * shape1Dist) + Math.Exp(sdfMergeStrength * shape2Dist)) / sdfMergeStrength;


            }
            else
            {
                //  Only union is implemented so far
                throw new Exception();
            }

            
            
            
            return signedDistance;
        }
        private double f(double lt)
        {
            double v1 = lt * lt;
            double v2 = Math.Sqrt(lt);

            return v1;
            //return lerpDoub(v1, v2, lt); // f(lt)
        }
        private Vec3 lerp(Vec3 lP0, Vec3 lP1, double lt)
        {
            double newX = (1 - lt) * lP0.x + lt * lP1.x;
            double newY = (1 - lt) * lP0.y + lt * lP1.y;
            double newZ = (1 - lt) * lP0.z + lt * lP1.z;

            Vec3 lP2 = new Vec3(newX, newY, newZ);
            return lP2;
        }

        public override Vec3 FindNormal(Vec3 rayLocation)   //  Also calculates new shape colour at the point
        {
            Vec3 normal = FindNormalNumerically(rayLocation);

            double shape1Dist = shape1.SDF(rayLocation);
            double shape2Dist = shape2.SDF(rayLocation);
            double t = Math.Exp(shape1Dist/ colourMergeStrength) / (Math.Exp(shape1Dist/ colourMergeStrength) + Math.Exp(shape2Dist/ colourMergeStrength));
            this.k_s = lerp(shape1.k_s, shape2.k_s, t);
            this.k_d = lerp(shape1.k_d, shape2.k_d, t);
            return normal;
        }

        
        
    }

    class InfiniteSphere : Shape
    {
        double radius { get; set; }
        Vec3 repetitionDistancesVector;
        public InfiniteSphere(Vec3 position, Vec3 specularComponent, Vec3 diffuseComponent, double alpha, double radius, Vec3 repetitionVector, Vec3 lightStrength = null) : base(position, specularComponent, diffuseComponent, alpha, lightStrength)
        {
            //  Only extra info that a sphere needs is the radius
            this.radius = radius;
            
            this.repetitionDistancesVector = repetitionVector;
            this.position = VecModulus(position, this.repetitionDistancesVector);

        }

        public override double SDF(Vec3 rayLocation)
        {
            rayLocation = VecModulus(rayLocation, repetitionDistancesVector);
            double signedDistance = Math.Sqrt(Math.Pow((rayLocation.x - position.x), 2) + Math.Pow((rayLocation.y - position.y), 2) + Math.Pow((rayLocation.z - position.z), 2)) - radius;
            //double signedDistance = rayLocation.x - position.x + rayLocation.y - position.y - radius; (Manhattan distance, didn't work)
            return signedDistance;    //  Absolute should enable rendering from inside the sphere - Did not work, rays cannot intersect with shape they started in
        }



        public override Vec3 FindNormal(Vec3 rayLocation)
        {
            rayLocation = VecModulus(rayLocation, repetitionDistancesVector);
            Vec3 normal = rayLocation - this.position;
            normal.Normalise();
            return normal;
        }

        public double Modulus(double a, double b)   //  Used in infite repetitions
        {
            double k = Math.Floor(a / b);
            double modVal = a - k * b;
            return modVal;
        }

        public Vec3 VecModulus(Vec3 a, Vec3 b)
        {
            Vec3 modVec = new Vec3(Modulus(a.x, b.x), Modulus(a.y, b.y), Modulus(a.z, b.z));
            return modVec;
        }
    }
}
