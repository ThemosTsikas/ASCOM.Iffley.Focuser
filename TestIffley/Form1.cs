using System;
using System.Windows.Forms;

namespace ASCOM.Iffley
{
    public partial class Form1 : Form
    {

        private ASCOM.DriverAccess.Focuser driver;

        public Form1()
        {
            InitializeComponent();
            SetUIState();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsConnected)
                driver.Connected = false;

            Properties.Settings.Default.Save();
        }

        private void buttonChoose_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.DriverId = ASCOM.DriverAccess.Focuser.Choose(Properties.Settings.Default.DriverId);
            SetUIState();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.Connected = false;
            }
            else
            {
                driver = new ASCOM.DriverAccess.Focuser(Properties.Settings.Default.DriverId);
                driver.Connected = true;
            }
            SetUIState();
        }

        private void SetUIState()
        {
            buttonConnect.Enabled = !string.IsNullOrEmpty(Properties.Settings.Default.DriverId);
            buttonChoose.Enabled = !IsConnected;
            buttonConnect.Text = IsConnected ? "Disconnect" : "Connect";
            if (IsConnected) Position.Text = driver.Position.ToString();
        }

        private bool IsConnected
        {
            get
            {
                return ((driver != null) && (driver.Connected == true));
            }
        }

        private void buttonAction_Click(object sender, EventArgs e)
        {
            driver.Action("ResetToZero", "");
            SetUIState();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            driver.Move(2000);
            SetUIState();

        }

        private void label1_Click(object sender, EventArgs e)
        {
            Position.Text = driver.Position.ToString();
            SetUIState();
        }

        private void Move0_Click(object sender, EventArgs e)
        {
            driver.Move(0);
            SetUIState();
        }

        private void Move100_Click(object sender, EventArgs e)
        {
            driver.Move(100);
            SetUIState();
        }
    }
}
