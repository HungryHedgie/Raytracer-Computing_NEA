using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer___Raymarching__Computing_NEA_
{
    public class PointLight
    {
        public Vec3 position;

        public Vec3 lightStrength = new Vec3(0, 0, 0);

        public PointLight(Vec3 position, Vec3 lightColour, double lightBrightness)
        {
            this.position = position;
            this.lightStrength = lightBrightness*lightColour;
        }

    }
}
