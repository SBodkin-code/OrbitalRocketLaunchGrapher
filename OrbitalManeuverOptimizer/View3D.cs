using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OrbitalManeuverOptimizer
{
    public partial class View3D: Form
    {
        Timer myTimer;
        Button test;
        private List<Form> forms = new List<Form>();

        public View3D()
        {
            InitializeComponent();
            SetupUI();
            SetupTimer();
        }

        public void SetupUI()
        {
            this.Text = "Rocket Flight Tracking System";
            this.Size = new Size(640, 640);
            this.BackColor = Color.FromArgb(240, 240, 240);

            test = new Button()
            {
                Text = "TEST"
            };
            this.Controls.Add(test);
            SetupEvents();
        }

        private void SetupTimer()
        {
            myTimer = new Timer
            {
                Interval = 100 // 1 second
            };
            myTimer.Tick += new EventHandler(DisplayWindowPosition);
            myTimer.Start();
        }

        private void DisplayWindowPosition(object sender, EventArgs e)
        {
            //Console.WriteLine(this.DesktopLocation);
        }
        private void WindowStateChange(object sender, EventArgs e)
        {
            Console.WriteLine("Window State changed");
        }

        private void WindowSizeChange(object sender, EventArgs e)
        {
            Console.WriteLine("Window Size Changed");
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            Console.WriteLine("Window Closed");
        }

        private void SetupEvents()
        {
            this.SizeChanged += new EventHandler(this.WindowSizeChange);
            this.FormClosing += this.FormClose;
            this.test.Click += new EventHandler(this.TestButtonClick);
        }

        private void FormClose(object sender, FormClosingEventArgs e)
        {
            Console.Write("Form Closed");

            //Do Some Extra Work Here
        }

        private void TestButtonClick(object sender, EventArgs e)
        {
            Form newForm = new Form();
            forms.Append(newForm);
            newForm.Show();
            newForm.GotFocus += NewForm_GotFocus;
            newForm.SizeChanged += NewForm_DragDrop;
        }

        private void NewForm_DragDrop(object sender,  EventArgs e)
        {
            Console.Write("Dropped");
            if (forms[0].DesktopLocation.X < -100)
            {
                forms[0].Hide();
            }
        }

        private void NewForm_GotFocus(object sender, EventArgs e)
        {
            Console.WriteLine("Got focus");
        }
    }
}
