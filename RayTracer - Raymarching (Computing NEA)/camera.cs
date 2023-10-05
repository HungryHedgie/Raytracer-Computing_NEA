using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace RayTracer___Raymarching__Computing_NEA_
{
    internal class camera
    {
        vec3 position;
        Quaternion rotation;

        public camera(vec3 startPos, Quaternion startRotation)
        {
            position = startPos;
            rotation = startRotation;
            
        }

        public vec3 camSpaceToWorldSpace(vec3 point_relCam) {
            vec3 point_relWorld = this.position + rotatePoint(point_relCam, this.rotation);
            
            return point_relWorld;
        }

        private vec3 rotatePoint(vec3 initialVec, Quaternion rotation) { 
            //  NOT IMPLEMENTED
        
        }

    }
}
