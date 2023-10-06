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

            //  Reflectance is the sum of the seperate components of specular and diffuse reflection
            double[] specularComponent = this.k_s * Math.Pow(Math.Max(omega_i * reflectedRay, 0), this.alpha);
        }
        
    reflectanceComponent <= k_d* max(dot(n_omega_o, n_Normal), 0)

    # The dot product term accounts for the shallow angles
        reflectance <= (specularComponent + reflectanceComponent) * max(dot(Normal, n_omega_i), 0)
    #   Reflectance is in the form (R, G, B)
    #   Each component is how much of the colour is reflected

    return reflectance
    }

    class Circle : Shape
    {
        public Circle(vec3 position, double[] specularComponent, double[] diffuseComponent, double alpha) : base (position, specularComponent, diffuseComponent, alpha)
        {
            this.position = position;
            this.k_s = specularComponent;
            this.k_d = diffuseComponent;
            this.alpha = alpha;
        }

        public override double SDF(vec3 rayLocation)
        {
            return position * position;
        }
    }
}
