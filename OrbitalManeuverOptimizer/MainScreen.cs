using OrbitalManeuverOptimizer.Properties;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace OrbitalManeuverOptimizer
{
    class MainScreen : Form
    {
        // For graph animation
        private List<DataSeries> allSeries = new List<DataSeries>();
        private System.Timers.Timer animationTimer;
        private int currentDataIndex;
        private int indexChange;

        private Chart altitudeChart;
        private Chart velocityChart;
        private Chart earthPositionChart;
        private Chart velocityXYChart;
        private Chart satelliteMassChart;
        private Chart satelliteAeroForceChart;
        private Panel controlsPanel;
        private Panel formsPanel;

        private Button startSimulation;
        private Button rocketView;
        private Button startAnimation;
        private Button stopAnimation;

        private TextBox durationTextBox;
        private TextBox initialXVelocityTextBox;
        private TextBox initialYVelocityTextBox;
        private TextBox initialXTextBox;
        private TextBox initialYTextBox;

        // Rocket Properties
        private TextBox rocketMassTextBox;
        private Label rocketFinalMassLable;
        // StageProperties
        private TextBox startTime1TextBox;
        private TextBox endTime1TextBox;
        private TextBox sepTime1TextBox;
        private TextBox stageMass1TextBox;
        private TextBox stageThrust1TextBox;
        private TextBox thrustAngle1TextBox;
        private TextBox isp1TextBox;
        private TextBox startTime2TextBox;
        private TextBox endTime2TextBox;
        private TextBox sepTime2TextBox;
        private TextBox stageMass2TextBox;
        private TextBox stageThrust2TextBox;
        private TextBox thrustAngle2TextBox;
        private TextBox isp2TextBox;

        // Top Panel
        private Button screen1Button;
        private Button screen2Button;

        private class DataSeries
        {
            public string Name { get; set; }
            public List<Point2D> SeriesData { get; set; }
            public Color SeriesColor { get; set; }
            public Chart TargetChart { get; set; }
            public Data DisplayedData { get; set; } = new Data();
        }

        private static dynamic orbitalOptimizer;

        private readonly RocketCalculations rocketCalculations;
        public MainScreen()
        {
            //InitializeComponent();
            LoadPythonRuntime();
            SetupUI();
            SetupTimer();
            rocketCalculations = new RocketCalculations();
        }
        private void SetupUI()
        {

            ComponentResourceManager resources = new ComponentResourceManager(typeof(MainScreen));

            this.Text = "Rocket Flight Tracking System";
            this.Size = new Size(1900, 1080);
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Load += new System.EventHandler(this.MainScreen_Load);

            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                Padding = new Padding(5),
            };
            this.Controls.Add(mainLayout);

            altitudeChart = HelperFunctions.CreateChart("altitudeChart", "Altitude (m)", Color.Green, "Time (s)", "Altitude (m)");
            velocityChart = HelperFunctions.CreateChart("velocityChart", "Velocity (m/s)", Color.Red, "Time (s)", "Velocity (m)");
            velocityXYChart = HelperFunctions.CreateChart("velocityXYChart", "VelocityXY (m/s)", Color.Aquamarine, "VelocityX (m/s)", "VelocityY (m/s)");
            earthPositionChart = HelperFunctions.CreateChart("earthPositionChart", "Earth Perspective", Color.Blue, "XPosition (m)", "YPosition (m)");
            satelliteMassChart = HelperFunctions.CreateChart("satelliteMassChart", "Satellite Mass", Color.Black, "Time (s)", "Mass (kg)");
            satelliteAeroForceChart = HelperFunctions.CreateChart("satelliteAeroForceChart", "AeroForce (N)", Color.Aquamarine, "AeroForceX (N)", "AeroForceY (N)");
            HelperFunctions.PopulateChartFromPython(orbitalOptimizer.PlotPlanet(), earthPositionChart, "EarthCoords");
            // (chart, column, row)
            mainLayout.Controls.Add(earthPositionChart, 0, 1);
            mainLayout.Controls.Add(altitudeChart, 0, 2);
            mainLayout.Controls.Add(velocityChart, 1, 1);
            mainLayout.Controls.Add(velocityXYChart, 1, 2);
            mainLayout.Controls.Add(satelliteMassChart, 2, 1);
            mainLayout.Controls.Add(satelliteAeroForceChart, 2, 2);

            // Controls Column (Column 0)
            controlsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };
            //  Panel Row 
            formsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
            };

            CreateControlsColumn();
            CreateTopPanel();
            SetupControlEvents();
            SetupPanelEvents();
            mainLayout.Controls.Add(controlsPanel, 0, 0);
            mainLayout.SetRowSpan(controlsPanel, 3);

            mainLayout.Controls.Add(formsPanel, 1, 0);
            mainLayout.SetColumnSpan(formsPanel, 3);

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 5F)); // Top Panel
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 47.5F)); // Top row
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 47.5F)); // Bottom row
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F)); // Control Panel
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F)); // 1 column
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F)); // 2 column
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F)); // 3 column

        }

        private void SetupTimer()
        {
            animationTimer = new System.Timers.Timer(50);
            animationTimer.Elapsed += OnTimerElapsed;
            animationTimer.AutoReset = true;
            animationTimer.Enabled = false;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // This method will be called from a different thread
            // Use Invoke to update UI elements from the UI thread
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateChart()));
            }
            else
            {
                UpdateChart();
            }
        }

        private void UpdateChart()
        {
            bool seriesPlotted = true;
            
            if (currentDataIndex < 300)
            {
                indexChange = 1;
            }
            else
            {
                indexChange = allSeries[0].SeriesData.Count / 250;
            }
                try
                {
                    foreach (var series in allSeries)
                    {
                        if (currentDataIndex < series.SeriesData.Count)
                        {
                            seriesPlotted = false;

                            series.DisplayedData.AddPoint(series.SeriesData[currentDataIndex].X, series.SeriesData[currentDataIndex].Y);

                            if (series.TargetChart != null)
                            {
                                Series chartSeries = series.TargetChart.Series.FirstOrDefault(x => x.Name == series.Name);
                                for (int i = 0; i < series.DisplayedData.Points.Count; i++)
                                {
                                    chartSeries.Points.AddXY(series.DisplayedData.Points[i].X, series.DisplayedData.Points[i].Y);
                                }

                                if (series.Name == "earthPositionChart")
                                {
                                    series.TargetChart.ChartAreas[0].RecalculateAxesScale();
                                }
                            }
                        }
                    }
                    currentDataIndex += indexChange;

                    if (seriesPlotted)
                    {
                        HelperFunctions.RealignSeries(earthPositionChart);
                        Console.WriteLine("Current Point: ", currentDataIndex);
                        animationTimer.Enabled = false;

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
        }

        private void SetupControlEvents()
        {
            this.startSimulation.Click += new System.EventHandler(this.StartSimulation_Click);
            this.rocketView.Click += new System.EventHandler(this.ViewRocketTrajectory_Click);
            this.startAnimation.Click += new System.EventHandler(this.StartAnimation_Click);
            this.stopAnimation.Click += new System.EventHandler(this.StopAnimation_Click);

            this.startTime1TextBox.TextChanged += new System.EventHandler(this.StartSimulation_Click);
            this.endTime1TextBox.TextChanged += new System.EventHandler(this.StartSimulation_Click);
            this.sepTime1TextBox.TextChanged += new System.EventHandler(this.StartSimulation_Click);
            this.stageMass1TextBox.TextChanged += new System.EventHandler(this.StartSimulation_Click);
            this.stageThrust1TextBox.TextChanged += new System.EventHandler(this.StartSimulation_Click);
            this.thrustAngle1TextBox.TextChanged += new System.EventHandler(this.StartSimulation_Click);
            this.isp1TextBox.TextChanged += new System.EventHandler(this.StartSimulation_Click);

            this.startTime2TextBox.TextChanged += new System.EventHandler(this.StartSimulation_Click);
            this.endTime2TextBox.TextChanged += new System.EventHandler(this.StartSimulation_Click);
            this.sepTime2TextBox.TextChanged += new System.EventHandler(this.StartSimulation_Click);
            this.stageMass2TextBox.TextChanged += new System.EventHandler(this.StartSimulation_Click);
            this.stageThrust2TextBox.TextChanged += new System.EventHandler(this.StartSimulation_Click);
            this.thrustAngle2TextBox.TextChanged += new System.EventHandler(this.StartSimulation_Click);
            this.isp2TextBox.TextChanged += new System.EventHandler(this.StartSimulation_Click);
        }
        private void SetupPanelEvents()
        {
            this.screen1Button.Click += new EventHandler(this.Screen1Button_Click);
            this.screen2Button.Click += new EventHandler(this.Screen2Button_Click);
        }

        private void Screen1Button_Click(object sender, EventArgs e)
        {
            UserControl userControl = new UserControl();
            userControl.Show();
        }

        private void Screen2Button_Click(object sender, EventArgs e)
        {
            Form View3DForm = new View3D();
            View3DForm.Show();
            Console.WriteLine("New Form");
        }

        private void CreateControlsColumn()
        {
            controlsPanel.SuspendLayout();

            startAnimation = new Button
            {
                Text = "Start Animation",
                Location = new Point(10, 180),
                Height = 50,
                BackColor = Color.Purple,
                ForeColor = Color.White,
                AutoSize = true,
                Dock = DockStyle.Bottom,
            };
            stopAnimation = new Button
            {
                Text = "Stop Animation",
                Location = new Point(10, 180),
                Height = 50,
                BackColor = Color.DarkBlue,
                ForeColor = Color.White,
                AutoSize = true,
                Dock = DockStyle.Bottom,
                Visible = false,
            };

            startSimulation = new Button
            {
                Text = "Start Simulation",
                Location = new Point(10, 180),
                Width = 150,
                BackColor = Color.Green,
                ForeColor = Color.White,
                AutoSize = true,
                Dock = DockStyle.Bottom,
                Padding = new Padding(20),
            };
            rocketView = new Button
            {
                Text = "Rocket View",
                Location = new Point(10, 180),
                Height = 50,
                BackColor = Color.Blue,
                ForeColor = Color.White,
                Dock = DockStyle.Bottom,
            };

            Label duration = new Label()
            {
                Text = "Duration (s)",
                AutoSize = true,
                Dock = DockStyle.Bottom,
                Location = new Point(10, 180)

            };
            durationTextBox = new TextBox()
            {
                Dock = DockStyle.Bottom,
                Text = "500"
            };

            Label initialX = new Label()
            {
                Text = "InitialX",
                AutoSize = true,
                Dock = DockStyle.Bottom,
                Location = new Point(10, 180)

            };
            initialXTextBox = new TextBox()
            {
                Dock = DockStyle.Bottom,
                // For orbit testing
                //Text = "500000"
                // for rocket launch testing
                Text = "0"
            };

            Label initialY = new Label()
            {
                Text = "InitialY",
                AutoSize = true,
                Dock = DockStyle.Bottom,
                Location = new Point(10, 180)

            };
            initialYTextBox = new TextBox()
            {
                Dock = DockStyle.Bottom,
                Text = "0"
            };

            Label initialXVelocity = new Label()
            {
                Text = "InitialXVelocity",
                AutoSize = true,
                Dock = DockStyle.Bottom,
                Location = new Point(10, 180)

            };
            initialXVelocityTextBox = new TextBox()
            {
                Dock = DockStyle.Bottom,
                Text = "0"
            };

            Label initialYVelocity = new Label()
            {
                Text = "InitialYVelocity",
                AutoSize = true,
                Dock = DockStyle.Bottom,
                Location = new Point(10, 180)

            };
            initialYVelocityTextBox = new TextBox()
            {
                Dock = DockStyle.Bottom,
                // for orbit testing 
                //Text = "8376",
                // for rocket launch testing
                Text = "0"
            };


            controlsPanel.Controls.AddRange(new Control[]
            {
                duration,
                durationTextBox,
                initialX,
                initialXTextBox,
                initialY,
                initialYTextBox,
                initialXVelocity,
                initialXVelocityTextBox,
                initialYVelocity,
                initialYVelocityTextBox,
                startSimulation,
                startAnimation,
                stopAnimation,
                rocketView
            });

            AddRocketPropertiesToControls(controlsPanel);
            controlsPanel.ResumeLayout();
        }

        private void CreateTopPanel()
        {
            // Button Height 
            int buttonHeight = (int)(40);
            // Button Width
            int buttonWidth = 50;
            int padding = 10;
            screen1Button = new Button
            {
                Text = "Screen 1",
                Location = new Point(0, 0),
                Height = buttonHeight,
                BackColor = Color.Gray,
                ForeColor = Color.White,
            };
            screen2Button = new Button
            {
                Text = "Screen 2",
                Location = new Point(padding*3 + buttonWidth, 0),
                Height = buttonHeight,
                BackColor = Color.Gray,
                ForeColor = Color.White,
            };

            formsPanel.Controls.AddRange(new Control[]
           {
                screen1Button,
                screen2Button,
           });
        }
        private static void LoadPythonRuntime()
        {
            try
            {
                string pythonFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PythonRuntime");
                PythonEngine.PythonPath = pythonFolder;
                string pythonScriptsPath = Path.Combine(pythonFolder, "PythonScripts");
                using (Py.GIL())
                {
                    // importing modules
                    dynamic np = Py.Import("numpy");
                    dynamic sci = Py.Import("scipy");
                    dynamic sys = Py.Import("sys");
                    // Adding pythons Scripts to Pythons search path
                    sys.path.append(pythonScriptsPath);
                    // importing python files
                    dynamic optimizerScript = Py.Import("OrbitalOptimizer");
                    Console.WriteLine("Successfully imported OrbitalOptimizer.py");
                    orbitalOptimizer = optimizerScript.OrbitOptimizer();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Python Initialization Error {ex}", "InitializationError", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void AddRocketPropertiesToControls(Panel controlPanel)
        {
            int textBoxPosition = 95;
            int labelSeperation = 30;
            int paddingTop = 20;
            int paddingLeft = 5;
            int textBoxWidth = 60;

            Label rocketMassLabel = new Label()
            {
                Text = "RocketMass",
                AutoSize = true,
                Location = new Point(15, paddingTop)
            };

            rocketMassTextBox = new TextBox()
            {
                Width = textBoxWidth,
                Location = new Point(textBoxPosition, paddingTop),
                Text = "556000.0"
            };

            rocketFinalMassLable = new Label()
            {
                Text = "RocketFinalMass: 0.0 kg",
                AutoSize = true,
                Location = new Point(15, 560)
            };

            // Create group boxes for each stage
            GroupBox stage1GroupBox = new GroupBox()
            {
                Text = "Stage 1",
                Location = new Point(10, 50),
                Size = new Size(160, 230),
            };

            GroupBox stage2GroupBox = new GroupBox()
            {
                Text = "Stage 2",
                Location = new Point(10, 300),
                Size = new Size(160, 230),
            };

            // Stage 1 Controls
            Label startTime1Label = new Label()
            {
                Text = "StartTime",
                AutoSize = true,
                Location = new Point(paddingLeft, paddingTop)
            };
            startTime1TextBox = new TextBox()
            {
                Width = textBoxWidth,
                Location = new Point(textBoxPosition, paddingTop),
                Text = "0.0"
            };

            Label endTime1Label = new Label()
            {
                Text = "EndTime",
                AutoSize = true,
                Location = new Point(paddingLeft, paddingTop + labelSeperation*1)
            };
            endTime1TextBox = new TextBox()
            {
                Width = textBoxWidth,
                Location = new Point(textBoxPosition, paddingTop + labelSeperation * 1),
                Text = "162.0"
            };

            Label sepTime1Label = new Label()
            {
                Text = "SeparationTime",
                AutoSize = true,
                Location = new Point(paddingLeft, paddingTop + labelSeperation * 2)
            };
            sepTime1TextBox = new TextBox()
            {
                Width = textBoxWidth,
                Location = new Point(textBoxPosition, paddingTop + labelSeperation * 2),
                Text = "5.0"
            };

            Label stageMass1Label = new Label()
            {
                Text = "StageMass",
                AutoSize = true,
                Location = new Point(paddingLeft, paddingTop + labelSeperation * 3)
            };
            stageMass1TextBox = new TextBox()
            {
                Width = textBoxWidth,
                Location = new Point(textBoxPosition, paddingTop + labelSeperation * 3),
                Text = "27200.0"
            };

            Label stageThrust1Label = new Label()
            {
                Text = "StageThrust",
                AutoSize = true,
                Location = new Point(paddingLeft, paddingTop + labelSeperation * 4)
            };
            stageThrust1TextBox = new TextBox()
            {
                Width = textBoxWidth,
                Location = new Point(textBoxPosition, paddingTop + labelSeperation * 4),
                Text = "6955815.0"
            };

            Label thrustAngle1Label = new Label()
            {
                Text = "ThrustAngle",
                AutoSize = true,
                Location = new Point(paddingLeft, paddingTop + labelSeperation * 5)
            };
            thrustAngle1TextBox = new TextBox()
            {
                Width = textBoxWidth,
                Location = new Point(textBoxPosition, paddingTop + labelSeperation * 5),
                Text = "3.0"
            };

            Label isp1Label = new Label()
            {
                Text = "ISP",
                AutoSize = true,
                Location = new Point(paddingLeft, paddingTop + labelSeperation * 6)
            };
            isp1TextBox = new TextBox()
            {
                Width = textBoxWidth,
                Location = new Point(textBoxPosition, paddingTop + labelSeperation * 6),
                Text = "283.0"
            };

            // Stage 2 Controls
            Label startTime2Label = new Label()
            {
                Text = "StartTime",
                AutoSize = true,
                Location = new Point(paddingLeft, paddingTop )
            };
            startTime2TextBox = new TextBox()
            {
                Width = textBoxWidth,
                Location = new Point(textBoxPosition, paddingTop),
                Text = "170.0"
            };

            Label endTime2Label = new Label()
            {
                Text = "EndTime",
                AutoSize = true,
                Location = new Point(paddingLeft, paddingTop + labelSeperation * 1)
            };
            endTime2TextBox = new TextBox()
            {
                Width = textBoxWidth,
                Location = new Point(textBoxPosition, paddingTop + labelSeperation * 1),
                Text = "575.0"
            };

            Label sepTime2Label = new Label()
            {
                Text = "SeparationTime",
                AutoSize = true,
                Location = new Point(paddingLeft, paddingTop + labelSeperation * 2)
            };
            sepTime2TextBox = new TextBox()
            {
                Width = textBoxWidth,
                Location = new Point(textBoxPosition, paddingTop + labelSeperation * 2),
                Text = "5.0"
            };

            Label stageMass2Label = new Label()
            {
                Text = "StageMass",
                AutoSize = true,
                Location = new Point(paddingLeft, paddingTop + labelSeperation * 3)
            };
            stageMass2TextBox = new TextBox()
            {
                Width = textBoxWidth,
                Location = new Point(textBoxPosition, paddingTop + labelSeperation * 3),
                Text = "4500.0"
            };

            Label stageThrust2Label = new Label()
            {
                Text = "StageThrust",
                AutoSize = true,
                Location = new Point(paddingLeft, paddingTop + labelSeperation * 4)
            };
            stageThrust2TextBox = new TextBox()
            {
                Width = textBoxWidth,
                Location = new Point(textBoxPosition, paddingTop + labelSeperation * 4),
                Text = "934132.0"
            };

            Label thrustAngle2Label = new Label()
            {
                Text = "ThrustAngle",
                AutoSize = true,
                Location = new Point(paddingLeft, paddingTop + labelSeperation * 5)
            };
            thrustAngle2TextBox = new TextBox()
            {
                Width = textBoxWidth,
                Location = new Point(textBoxPosition, paddingTop + labelSeperation * 5),
                Text = "82.7"
            };

            Label isp2Label = new Label()
            {
                Text = "ISP",
                AutoSize = true,
                Location = new Point(paddingLeft, paddingTop + labelSeperation * 6)
            };
            isp2TextBox = new TextBox()
            {
                Width = textBoxWidth,
                Location = new Point(textBoxPosition, paddingTop + labelSeperation * 6),
                Text = "348.0"
            };

            // Add controls to their respective group boxes
            stage1GroupBox.Controls.AddRange(new Control[]
            {
                startTime1Label, startTime1TextBox,
                endTime1Label, endTime1TextBox,
                sepTime1Label, sepTime1TextBox,
                stageMass1Label, stageMass1TextBox,
                stageThrust1Label, stageThrust1TextBox,
                thrustAngle1Label, thrustAngle1TextBox,
                isp1Label, isp1TextBox
            });

            stage2GroupBox.Controls.AddRange(new Control[]
            {
                startTime2Label, startTime2TextBox,
                endTime2Label, endTime2TextBox,
                sepTime2Label, sepTime2TextBox,
                stageMass2Label, stageMass2TextBox,
                stageThrust2Label, stageThrust2TextBox,
                thrustAngle2Label, thrustAngle2TextBox,
                isp2Label, isp2TextBox
            });

            controlPanel.Controls.AddRange(new Control[]
            {
                rocketMassLabel, rocketMassTextBox,
                stage1GroupBox,
                stage2GroupBox,
                rocketFinalMassLable,
            });
        }


        private void RocketProperties(List<double> startTime, List<double> endTime, List<double> seperationTime, List<double> stageMass, List<double> stageThrust, List<double> thrustAngle, List<double> ISP, double rocketMass)
        {
            int Num = 2;
            
            rocketCalculations.SetRocketProperties(orbitalOptimizer, Num, startTime, endTime, seperationTime,
                stageMass, stageThrust, thrustAngle, ISP, rocketMass);

            double stage1MassUse = (endTime[0] - startTime[0]) * (stageThrust[0] / (ISP[0] * 9.81)) + stageMass[0];
            double stage2MassUse = (endTime[1] - startTime[1]) * (stageThrust[1] / (ISP[1] * 9.81)) + stageMass[1];
            double totalMassExpended = stage1MassUse + stage2MassUse;
            Console.WriteLine("#############################");
            Console.WriteLine("stage1 Thrust: " + stageThrust[0] + " ISP: " + ISP[0]);
            Console.WriteLine("stage2 Thrust: " + stageThrust[1] + " ISP: " + ISP[1]);
            Console.WriteLine("stage1 Mass: " + stageMass[0]);
            Console.WriteLine("stage2 Mass: " + stageMass[1]);
            Console.WriteLine("#############################");
            Console.WriteLine("Stage1MassUse: " + stage1MassUse + " kg");
            Console.WriteLine("Stage2MassUse: " + stage2MassUse + " kg");
            Console.WriteLine("RocketMassUse: " + totalMassExpended);
            Console.WriteLine("RocketMass: " + rocketMass);
            Console.WriteLine("RocketMassEnd: " + (rocketMass - totalMassExpended));
            Console.WriteLine("#############################");
            rocketFinalMassLable.Text = $"RocketFinalMass: {Math.Round((rocketMass - totalMassExpended), 2)} Kg";
        }
        private void TrajectoryCalculation(int Duration, double InitialX, double InitialY, double InitialXVelocity, double InitialYVelocity, bool animate=false)
        {
            TrajectoryData satellite = rocketCalculations.CalculateTrajectory(orbitalOptimizer, Duration, InitialX, InitialY, InitialXVelocity, InitialYVelocity);

            var chartConfigs = new[]
            {
                (Name: "earthPositionChart", XData: satellite.XPosition, YData: satellite.YPosition, Chart: earthPositionChart),
                (Name: "velocityChart", XData: satellite.TimeFrame, YData: satellite.Velocity, Chart: velocityChart),
                (Name: "altitudeChart", XData: satellite.TimeFrame, YData: satellite.Altitude, Chart: altitudeChart),
                (Name: "velocityXYChart", XData: satellite.XVelocity, YData: satellite.YVelocity, Chart: velocityXYChart),
                (Name: "satelliteMassChart", XData: satellite.TimeFrame, YData: satellite.Mass, Chart: satelliteMassChart),
                (Name: "satelliteAeroForceChart", XData: satellite.TimeFrame, YData: satellite.AeroForce, Chart: satelliteAeroForceChart)
            };

            allSeries.Clear();
            // Process each configuration
            foreach (var config in chartConfigs)
            {
                Data chartData = new Data();
                chartData.ConvertArrays(config.XData, config.YData);

                allSeries.Add(new DataSeries()
                {
                    Name = config.Name,
                    SeriesData = chartData.Points,
                    TargetChart = config.Chart
                });
                if (!animate)
                {
                    HelperFunctions.PopulateChart(config.Chart, config.Name, config.XData, config.YData);
                }
              
            }
        }
        
        private void MainScreen_Load(object sender, EventArgs e)
        {

        }
        private void StartSimulation_Click(object sender, EventArgs e)
        {
            // Clear all graphs 
            animationTimer.Enabled = false;
            startAnimation.Visible = true;
            stopAnimation.Visible = false;
            SetUserRocketProperties();
            TrajectoryCalculation(int.Parse(durationTextBox.Text), int.Parse(initialXTextBox.Text), int.Parse(initialYTextBox.Text), int.Parse(initialXVelocityTextBox.Text), int.Parse(initialYVelocityTextBox.Text));

            //Console.WriteLine("CalculateTrajectory clicked");
        }
        private void ViewRocketTrajectory_Click(object sender, EventArgs e)
        {
            HelperFunctions.RealignSeries(earthPositionChart);
        }

        private void StartAnimation_Click(object sender, EventArgs e)
        {
            
            altitudeChart.Series.Clear();
            velocityChart.Series.Clear();
            velocityXYChart.Series.Clear();
            earthPositionChart.Series.Clear();
            satelliteMassChart.Series.Clear();
            satelliteAeroForceChart.Series.Clear();
            HelperFunctions.PopulateChartFromPython(orbitalOptimizer.PlotPlanet(), earthPositionChart, "EarthCoords");
            
            // Clear all graphs 
            animationTimer.Enabled = true;
            stopAnimation.Visible = true;
            startAnimation.Visible = false;
            TrajectoryCalculation(int.Parse(durationTextBox.Text), int.Parse(initialXTextBox.Text), int.Parse(initialYTextBox.Text), int.Parse(initialXVelocityTextBox.Text), int.Parse(initialYVelocityTextBox.Text), animate:true);
            //Console.WriteLine("StartAnimation clicked");
        }

        private void StopAnimation_Click(object sender, EventArgs e)
        {
            animationTimer.Enabled = false;
            stopAnimation.Visible = false;
            startAnimation.Visible = true;
            Console.WriteLine("FalseAnimation clicked");
        }

        private void SetUserRocketProperties()
        {
            List<double> userStartTime = new List<double>() { double.Parse(startTime1TextBox.Text), double.Parse(startTime2TextBox.Text) };
            List<double> userEndTime = new List<double>() { double.Parse(endTime1TextBox.Text), double.Parse(endTime2TextBox.Text) };
            List<double> userSeperationTime = new List<double>() { double.Parse(sepTime1TextBox.Text), double.Parse(sepTime2TextBox.Text) };
            List<double> userStageMass = new List<double>() { double.Parse(stageMass1TextBox.Text), double.Parse(stageMass2TextBox.Text) };
            List<double> userStageThrust = new List<double>() { double.Parse(stageThrust1TextBox.Text), double.Parse(stageThrust2TextBox.Text) };
            List<double> userThrustAngle = new List<double>() { double.Parse(thrustAngle1TextBox.Text), double.Parse(thrustAngle2TextBox.Text) };
            List<double> userISP = new List<double>() { double.Parse(isp1TextBox.Text), double.Parse(isp2TextBox.Text) };
            double userRocketMass = double.Parse(rocketMassTextBox.Text);
            /*
            Console.WriteLine("userStartTime:" + userStartTime[0] + " | " + userStartTime[1]);
            Console.WriteLine("userEndTime:" + userEndTime[0] + " | " + userEndTime[1]);
            Console.WriteLine("userSeperationTime:" + userSeperationTime[0] + " | " + userSeperationTime[1]);
            Console.WriteLine("userStageMass:" + userStageMass[0] + " | " + userStageMass[1]);
            Console.WriteLine("userStageThrust:" + userStageThrust[0] + " | " + userStageThrust[1]);
            Console.WriteLine("userThrustAngle:" + userThrustAngle[0] + " | " + userThrustAngle[1]);
            Console.WriteLine("userISP:" + userISP[0] + " | " + userISP[1]);
            */

            RocketProperties(userStartTime, userEndTime, userSeperationTime, userStageMass, userStageThrust, userThrustAngle, userISP, userRocketMass);
           
        }


        private void InitializeComponent()
        {
        }
    }
}