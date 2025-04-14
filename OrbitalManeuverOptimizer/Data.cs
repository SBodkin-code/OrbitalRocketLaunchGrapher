
using System.Collections.Generic;

namespace OrbitalManeuverOptimizer
{
    class Data
    {
        public List<Point2D> Points { get; private set; } 

        public Data()
        {
            Points = new List<Point2D>();
        }
        public void AddPoint(double x, double y)
        {
            Points.Add(new Point2D(x, y));
        }

        public void ConvertArrays(double[] arrayA, double[] arrayB)
        {
            for(int x = 0; x < arrayA.Length; x++)
            {
                AddPoint(arrayA[x], arrayB[x]);
            }
        }
    }

    public struct Point2D
    {
        public double X { get; }
        public double Y { get; }

        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
