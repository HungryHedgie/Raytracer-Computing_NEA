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
        public vec3 position;
        public Quaternion rotation;

        public camera(vec3 startPos, double[] fullRotationInfo)
        {
            position = startPos;

            double xyPlaneRot = fullRotationInfo[0];
            double yzPlaneRot = fullRotationInfo[1];
            double xzPlaneRot = fullRotationInfo[2];
            //  Generates corresponding quaternions for xy, yz and xy plane rotations, then combines them
            Quaternion xyQuat = new Quaternion(0, 0, MathF.Sin((float)(xyPlaneRot / 2) * MathF.PI / 180), MathF.Cos((float)(xyPlaneRot / 2) * MathF.PI / 180));
            Quaternion yzQuat = new Quaternion(MathF.Sin((float)(yzPlaneRot / 2) * MathF.PI / 180), 0, 0, MathF.Cos((float)(yzPlaneRot / 2) * MathF.PI / 180));
            Quaternion xzQuat = new Quaternion(0, MathF.Sin((float)(xzPlaneRot / 2) * MathF.PI / 180), 0, MathF.Cos((float)(xzPlaneRot / 2) * MathF.PI / 180));
            rotation = xzQuat * yzQuat * xyQuat;
            
        }

        public vec3 camSpaceToWorldSpace(vec3 point_relCam) {
            vec3 point_relWorld = this.position + rotatePoint(point_relCam, this.rotation);
            
            return point_relWorld;
        }


        private vec3 rotatePoint(vec3 initialVec, Quaternion rotation) {
            Quaternion initialQuat = initialVec.asQuaternion();
            Quaternion finalQuat = rotation * initialQuat * Quaternion.Conjugate(rotation);
            return new vec3(finalQuat);
        }

    }
}
