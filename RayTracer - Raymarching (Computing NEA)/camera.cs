﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace RayTracer___Raymarching__Computing_NEA_
{
    internal class Camera
    {
        public Vec3 position;
        public Quaternion rotation;
        public Quaternion xyQuat;   //  This is a class variable as when updating newDirection we need to know the xy rotation on its own

        public Camera(Vec3 startPos, double[] fullRotationInfo)
        {
            position = startPos;

            newRotation(fullRotationInfo);
            
        }

        public void newRotation(double[] fullRotationInfo)
        {
            double xyPlaneRot = fullRotationInfo[0];
            double yzPlaneRot = fullRotationInfo[1];
            double xzPlaneRot = fullRotationInfo[2];   //  We take negative of this as for quaternions a positive xz rotation points downwards
            //  Generates corresponding quaternions for xy, yz and xy plane rotations, then combines them
            this.xyQuat = new Quaternion(0, 0, MathF.Sin((float)(xyPlaneRot / 2) * MathF.PI / 180), MathF.Cos((float)(xyPlaneRot / 2) * MathF.PI / 180));
            Quaternion yzQuat = new Quaternion(MathF.Sin((float)(yzPlaneRot / 2) * MathF.PI / 180), 0, 0, MathF.Cos((float)(yzPlaneRot / 2) * MathF.PI / 180));
            Quaternion xzQuat = new Quaternion(0, -MathF.Sin((float)(xzPlaneRot / 2) * MathF.PI / 180), 0, MathF.Cos((float)(xzPlaneRot / 2) * MathF.PI / 180));
            this.rotation =  yzQuat * xyQuat * xzQuat;
        }

        public void newDirection(Vec3 newMovement)
        {
            Vec3 deltaXY = new Vec3(newMovement.x, newMovement.y, 0);
            Vec3 deltaZ = new Vec3(0, 0, newMovement.z);
            this.position += rotatePoint(deltaXY, this.xyQuat);
            this.position += deltaZ;
        }

        public Vec3 camSpaceToWorldSpace(Vec3 point_relCam) {
            Vec3 point_relWorld = this.position + rotatePoint(point_relCam, this.rotation);
            return point_relWorld;
        }


        private Vec3 rotatePoint(Vec3 initialVec, Quaternion rotation) {
            Quaternion initialQuat = initialVec.AsQuaternion();
            Quaternion finalQuat = rotation * initialQuat * Quaternion.Conjugate(rotation);
            return new Vec3(finalQuat);
        }

    }
}
