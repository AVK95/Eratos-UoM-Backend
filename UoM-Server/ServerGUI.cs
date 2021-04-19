using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UoM_Server
{
    public partial class ServerGUI : Form
    {
        private bool serverRunning;
        private MultiThreadedServer server;

        private string ipAddress = "127.0.0.1";
        private int port = 8888;

        public ServerGUI()
        {
            InitializeComponent();
            serverRunning = false;
            rbOFF.Checked = true;
            rbOn.Checked = false;
        }

        private void rbOn_CheckedChanged(object sender, EventArgs e)
        {
            if (rbOn.Checked == false)
                return;
            
            if (!serverRunning)
            {
                server = new MultiThreadedServer(ipAddress, port);
                serverRunning = true;
                MessageBox.Show("Server Started on " + ipAddress + " : " + port.ToString());
            }
        }

        private void rbOFF_CheckedChanged(object sender, EventArgs e)
        {
            if (rbOFF.Checked == false)
                return;

            if(serverRunning)
            {
                server.StopServer();
                serverRunning = false;
            }
        }
    }
}
