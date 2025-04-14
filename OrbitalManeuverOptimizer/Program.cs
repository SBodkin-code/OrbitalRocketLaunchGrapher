using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Python.Runtime;

namespace OrbitalManeuverOptimizer
{
    static class Program
    {
        private static string pythonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PythonRuntime");
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Environment.SetEnvironmentVariable("PYTHONHOME", pythonPath);
            Environment.SetEnvironmentVariable("PYTHONPATH", pythonPath);
            Runtime.PythonDLL = Path.Combine(pythonPath, "python313.dll"); ;
            PythonEngine.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainScreen());
        }
    }
}