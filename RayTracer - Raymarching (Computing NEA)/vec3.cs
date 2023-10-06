﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RayTracer___Raymarching__Computing_NEA_
{
    internal class vec3
    {
        public double x;
        public double y;
        public double z;

        public vec3(double x_in, double y_in, double z_in)
        {
            x = x_in;
            y = y_in;
            z = z_in;
        }

        public vec3(Quaternion input)
        {
            x = input.X;
            y = input.Y;
            z = input.Z;
            if(input.W != 0)    //  Debug and could be removed
            {
                MessageBox.Show("Error with quaternion to vector, real component is non zero");
            }
        }

        public Quaternion asQuaternion()
        {
            Quaternion quat = new Quaternion((float)x, (float)y, (float)z, 0);
            return quat;
        }

        public static vec3 cross(vec3 a, vec3 b)
        {
            double s_1 = a.y * b.z - a.z * b.y;
            double s_2 = a.z * b.x - a.x * b.z;
            double s_3 = a.x * b.y - a.y * b.x;
            return new vec3(s_1, s_2, s_3);

        }

        static public vec3 normalise(vec3 a)
        {
            double normFraction = 1 / Math.Sqrt(Math.Pow(a.x, 2) + Math.Pow(a.y, 2) + Math.Pow(a.z, 2));
            return a * normFraction;
        }

        static double dot3D(vec3 a, vec3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        static public vec3 operator +(vec3 a, vec3 b)
        {
            return new vec3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        static public vec3 operator *(double a, vec3 b)
        {
            return new vec3(a * b.x, a * b.y, a * b.z);
        }

        static public vec3 operator *(vec3 b, double a)
        {
            return new vec3(a * b.x, a * b.y, a * b.z);
        }

        static public vec3 operator -(vec3 a, vec3 b)
        {
            return new vec3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

    }
}
