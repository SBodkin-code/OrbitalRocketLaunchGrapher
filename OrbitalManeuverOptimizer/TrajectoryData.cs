

namespace OrbitalManeuverOptimizer
{
    class TrajectoryData
    {
        public double[] XPosition { get; set; }
        public double[] YPosition { get; set; }
        public double[] XVelocity { get; set; }
        public double[] YVelocity { get; set; }
        public double[] Altitude { get; set; }
        public double[] Velocity { get; set; }
        public double[] XAeroForce { get; set; }
        public double[] YAeroForce { get; set; }
        public double[] AeroForce { get; set; }
        public double[] Mass { get; set; }
        public double[] TimeFrame { get; set; }

        // You could add helper methods here if needed
        public int DataLength => TimeFrame?.Length ?? 0;
    }
}
