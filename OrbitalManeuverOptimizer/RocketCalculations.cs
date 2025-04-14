using System;
using System.Collections.Generic;

namespace OrbitalManeuverOptimizer
{
    class RocketCalculations
    {
        const string pythonPathLaptop = @"C:\Python312";
        const string pythonPathDesktop = @"C:\Users\sambo\AppData\Local\Programs\Python\Python313";

        public RocketCalculations()
        {
            //LoadPythonRuntime();
        }

        public TrajectoryData CalculateTrajectory(dynamic pythonClass, int Duration, double InitialX, double InitialY, double InitialXVelocity, double InitialYVelocity)
        {
            var trajectoryArray = pythonClass.TrajectoryCalculation(Duration, InitialX, InitialY, InitialXVelocity, InitialYVelocity);

            // Seperate Trajectory Array into Arrays
            double[] satelliteX = HelperFunctions.ProcessPythonArray(trajectoryArray["xArray"]);
            double[] satelliteY = HelperFunctions.ProcessPythonArray(trajectoryArray["yArray"]);
            double[] satelliteVelocityX = HelperFunctions.ProcessPythonArray(trajectoryArray["xVelArray"]);
            double[] satelliteVelocityY = HelperFunctions.ProcessPythonArray(trajectoryArray["yVelArray"]);
            double[] satelliteAltitude = HelperFunctions.ProcessPythonArray(trajectoryArray["altitude"]);
            double[] satelliteVelocity = HelperFunctions.ProcessPythonArray(trajectoryArray["normalisedVelocity"]);
            double[] satelliteMass = HelperFunctions.ProcessPythonArray(trajectoryArray["satelliteMass"]);
            double[] satelliteAeroForceX = HelperFunctions.ProcessPythonArray(trajectoryArray["xAeroForce"]);
            double[] satelliteAeroForceY = HelperFunctions.ProcessPythonArray(trajectoryArray["yAeroForce"]);
            double[] satelliteAeroForce = HelperFunctions.ProcessPythonArray(trajectoryArray["AeroForce"]);
            double[] timeFrame = HelperFunctions.ProcessPythonArray(trajectoryArray["timeFrame"]);

            return new TrajectoryData()
            {
                XPosition = satelliteX,
                YPosition = satelliteY,
                XVelocity = satelliteVelocityX,
                YVelocity = satelliteVelocityY,
                Altitude = satelliteAltitude,
                Mass = satelliteMass,
                Velocity = satelliteVelocity,
                XAeroForce = satelliteAeroForceX,
                YAeroForce = satelliteAeroForceY,
                AeroForce = satelliteAeroForce,
                TimeFrame = timeFrame
            };
            
        }



        public void SetRocketProperties(dynamic pythonClass, int NumberStages, List<double> StageStartTime, List<double> StageEndTime, List<double> StageSeperationTime,
            List<double> StageMass, List<double> StageThrust, List<double> StageThrustAngle, List<double> StageISP, double RocketMass)
        {
            try
            {
                var stageData = pythonClass.DefineRocketStages(NumberStages, StageStartTime, StageEndTime, StageSeperationTime, StageMass, StageThrust, StageThrustAngle, StageISP, RocketMass);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }
    }
}
