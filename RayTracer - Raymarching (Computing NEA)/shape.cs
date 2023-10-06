using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer___Raymarching__Computing_NEA_
{
    abstract class Shape
    {
        public abstract double SDF(vec3 rayLocation);
        public vec3 position = new(1, 0, 0);

        public double[] k_s = new double[3] { 0.5, 0.5, 0.5 };
        public double[] k_d = new double[3] { 0, 0, 1 };
        public double alpha = 1;

        public Shape(vec3 position, double[] specularComponent, double[] diffuseComponent, double alpha)
        {
            this.position = position;
            this.k_s = specularComponent;
            this.k_d = diffuseComponent;
            this.alpha = alpha;
        }

        public double[] BRDF_phong(vec3 omega_i, vec3 omega_o, vec3 normal)
        {
            omega_i.normalise();
            omega_o.normalise();
            normal.normalise();

            //  Max is in place in case the dot product is negative
            vec3 reflectedRay = 2 * Math.Max(omega_i * normal, 0) * normal - omega_i;

            //  Reflectance is the sum of the seperate components of specular and diffuse reflection    HERE ONWARDS DONE with VS CODE, NEEDS CHECKING WITH Visual Studio
            double[] specularComponent = this.k_s * Math.Pow(Math.Max(omega_i * reflectedRay, 0), this.alpha);
            double[] reflectanceComponent = k_d* Math.Max(dot(n_omega_o, n_Normal), 0);

            //  The dot product term accounts for the shallow angles, which have a lower contribution
            double[] reflectance = (specularComponent + reflectanceComponent) * Math.Max(dot(Normal, n_omega_i), 0);

            //  Reflectance is in the form (R, G, B)
            //  Each component is how much of the colour is reflected
            return reflectance
        }
    
    }

    class Circle : Shape
    {
        double radius {get; set;};
        public Circle(vec3 position, double[] specularComponent, double[] diffuseComponent, double alpha, double radius) : base (position, specularComponent, diffuseComponent, alpha)
        {
            this.position = position;
            this.k_s = specularComponent;
            this.k_d = diffuseComponent;
            this.alpha = alpha;
            this.radius = radius;

        }

        public override double SDF(vec3 rayLocation)
        {
            return Math.Sqrt(Math.Pow((rayLocation[0] - position[0]), 2), Math.Pow((rayLocation[1] - position[1]), 2), Math.Pow((rayLocation[2] - position[2]), 2)) - radius
        }
    }
}
