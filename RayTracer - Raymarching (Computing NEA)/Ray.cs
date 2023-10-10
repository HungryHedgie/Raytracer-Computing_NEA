using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer___Raymarching__Computing_NEA_
{
    internal class Ray
    {
        public Vec3 position;
        public Vec3? direction;
        public Shape? previousShape;

        public Vec3 runningTotalOfReflectance = new(1, 1, 1);

        public Ray(Vec3 position, Vec3 direction)
        {
            this.position = position;
            this.direction = direction;
            this.previousShape = null;
        }
    }
}
