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

        public vec3 k_s = new( 0.5, 0.5, 0.5 );
        public vec3 k_d = new( 0, 0, 1 );
        public double alpha = 1;

        public Shape(vec3 position, vec3 specularComponent, vec3 diffuseComponent, double alpha)
        {
            this.position = position;
            this.k_s = specularComponent;
            this.k_d = diffuseComponent;
            this.alpha = alpha;
        }

        public vec3 BRDF_phong(vec3 omega_i, vec3 omega_o, vec3 normal)
        {
            //  SAME AS PSEUDOCODE
            omega_i.normalise();
            omega_o.normalise();
            normal.normalise();

            //  Max is in place in case the dot product is negative
            vec3 reflectedRay = 2 * Math.Max(omega_i * normal, 0) * normal - omega_i;

            //  Reflectance is the sum of the seperate components of specular and diffuse reflection    HERE ONWARDS DONE with VS CODE, NEEDS CHECKING WITH Visual Studio
            vec3 specularComponent = this.k_s * Math.Pow(Math.Max(omega_i * reflectedRay, 0), this.alpha);
            vec3 reflectanceComponent = this.k_d * Math.Max(omega_i * normal, 0);

            //  The dot product term accounts for the shallow angles, which have a lower contribution
            vec3 reflectance = (specularComponent + reflectanceComponent) * Math.Max(normal * omega_i, 0);

            //  Reflectance is in the form (R, G, B)
            //  Each component is how much of the colour is reflected
            return reflectance;
        }
    
    }

    class Circle : Shape
    {
        double radius {get; set;}
        public Circle(vec3 position, vec3 specularComponent, vec3 diffuseComponent, double alpha, double radius) : base (position, specularComponent, diffuseComponent, alpha)
        {
            this.position = position;
            this.k_s = specularComponent;
            this.k_d = diffuseComponent;
            this.alpha = alpha;
            this.radius = radius;

        }

        public override double SDF(vec3 rayLocation)
        {
            return Math.Sqrt(Math.Pow((rayLocation.x - position.x), 2) + Math.Pow((rayLocation.y - position.y), 2) + Math.Pow((rayLocation.z - position.z), 2)) - radius;
        }
    }
}
