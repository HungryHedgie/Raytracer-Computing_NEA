namespace TestingWithConsole_Raytracer
{
    internal class Program
    {
        static Random rnd = new Random();
        static int pointCount = 150;
        static void Main(string[] args)
        {
            Vec3 normal = new(0, 0, 1);
            string allPoints = "";
            for (int i = 0; i < pointCount; i++)
            {
                Vec3 currPoint = FindingNewRayDirection(normal);
                string vecInfo = currPoint.x.ToString() + "," + currPoint.y.ToString() + "," + currPoint.z.ToString();
                allPoints += vecInfo + "\n";
            }
            Console.WriteLine(allPoints);
        }
        public static Vec3 FindingNewRayDirection(Vec3 normal)
        {
            double theta = rnd.NextDouble() * 2 * Math.PI;  //   Get a random number for the trig functions
            double phi = rnd.NextDouble() * 2 * Math.PI;    //  Could decrease precision, 2 or 3 dp is likely to be enough

            double cosTheta = Math.Cos(theta);
            double sinTheta = Math.Sin(theta);

            double cosPhi = Math.Cos(phi);
            double sinPhi = Math.Sin(phi);

            //  This method will give us an already normalised value
            //  newDir will be a random point lying on a unit sphere
            Vec3 newDir = new(cosPhi * cosTheta, cosPhi * sinTheta, sinPhi);
            //  If newDir faces back in towards the centre of the object we flip it so it points out
            if (newDir * normal < 0)
            {
                newDir *= -1;
            }
            return newDir;
        }

    }

}