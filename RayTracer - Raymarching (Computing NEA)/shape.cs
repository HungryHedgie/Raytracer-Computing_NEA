﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer___Raymarching__Computing_NEA_
{
    abstract class Shape
    {
        public abstract double SDF(Vec3 rayLocation);
        public abstract Vec3 findNormal(Vec3 rayLocation);
        public Vec3 position;

        public Vec3 k_s;
        public Vec3 k_d;
        public double alpha;

        public Vec3 lightStrength = new Vec3(0, 0, 0);

        public Shape(Vec3 position, Vec3 specularComponent, Vec3 diffuseComponent, double alpha, Vec3 lightStrength = null) //  Need to change
        {
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
            omega_i.normalise();    //  Incoming light (From a physics perspective)
            omega_o.normalise();    //  Outgoing from a physics perspective, so these are the opposite of the order we got our rays
            normal.normalise();

            //  Max is in place in case the dot product is negative
            Vec3 reflectedRay = 2 * Math.Max(omega_i * normal, 0) * normal - omega_i;

            //  Reflectance is the sum of the seperate components of specular and diffuse reflection    HERE ONWARDS DONE with VS CODE, NEEDS CHECKING WITH Visual Studio
            Vec3 specularComponent = this.k_s * Math.Pow(Math.Max(omega_i * reflectedRay, 0), this.alpha);
            Vec3 reflectanceComponent = this.k_d * Math.Max(omega_o * normal, 0);

            //  The dot product term accounts for the shallow angles, which have a lower contribution
            Vec3 reflectance = (specularComponent + reflectanceComponent) * Math.Max(normal * omega_i, 0);

            //  Reflectance is in the form (R, G, B)
            //  Each component is how much of the colour is reflected
            return reflectance;
        }
    
    }

    class Sphere : Shape
    {
        double radius {get; set;}
        public Sphere(Vec3 position, Vec3 specularComponent, Vec3 diffuseComponent, double alpha, double radius, Vec3 lightStrength = null) : base (position, specularComponent, diffuseComponent, alpha, lightStrength)
        {
            
            this.radius = radius;

        }

        public override double SDF(Vec3 rayLocation)
        {
            double signedDistance =  Math.Sqrt(Math.Pow((rayLocation.x - position.x), 2) + Math.Pow((rayLocation.y - position.y), 2) + Math.Pow((rayLocation.z - position.z), 2)) - radius;
            return Math.Abs(signedDistance);    //  Absolute should enable rendering from inside the sphere
        }

        public override Vec3 findNormal(Vec3 rayLocation)
        {
            Vec3 normal = rayLocation - this.position;
            normal.normalise();
            return normal;
        }
    }
}
