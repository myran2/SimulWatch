using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace SimulWatch
{
    public partial class Main : Form
    {

        private const int serverPort = 22000;
        private Connection client = null;

        public Main()
        {
            InitializeComponent();
            RefreshProcessDropdown();
        }

        private void ConnectButton_click(object sender, EventArgs e)
        {
            // If already connected, shut down the socket
            if (client != null && client.Active())
            {
                client.Close();
                client = null;
                ConnectButton.Text = "Connect";
            }
            
            // not connected - start up the socket
            else
            {
                // IpAddr string needs to be at least as long as 1.1.1.1 or it can't be a valid IP
                if (IpAddrText.Text.Length < 8)
                    return;

                try
                {
                    client = new Connection(IpAddrText.Text, serverPort);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                ConnectButton.Text = "Disconnect";
            }
        }

        private Int32 GetCommandExecuteTime()
        {
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            // tell other clients to wait 3 seconds from the current time
            return unixTimestamp;
        }

        private void PauseButton_click(object sender, EventArgs e)
        {
            client.Send(String.Format("{0} {1}\n", "Pause", GetCommandExecuteTime()));
        }

        private void PrevButton_click(object sender, EventArgs e)
        {
            client.Send(String.Format("{0} {1}\n", "Prev", GetCommandExecuteTime()));
        }

        private void NextButton_click(object sender, EventArgs e)
        {
            client.Send(String.Format("{0} {1}\n", "Next", GetCommandExecuteTime()));
        }

        private void RefreshProcessesButton_click(object sender, EventArgs e)
        {
            RefreshProcessDropdown();

            CommandHandler.videoPlayer = null;
            this.Text = this.Name;
            this.ProcessList.ResetText();
        }

        private void ProcessList_Changed(object sender, EventArgs e)
        {
            ComboBox combobox = (ComboBox)sender;
            string selectedProcess = (string)combobox.SelectedItem;

            CommandHandler.videoPlayer = Process.GetProcessesByName(selectedProcess)[0];
            this.Text = string.Format("{0} - \"{1}\"", this.Name, selectedProcess);
        }

        private void RefreshProcessDropdown()
        {
            // clear before adding to account for cases where there are already entries in the dropdown
            this.ProcessList.Items.Clear();

            Process[] processList = Process.GetProcesses();
            foreach (Process p in processList)
            {
                // TODO: add player process names. also confirm that the vlc process name is vlc
                if (p.ProcessName == "mpv" || p.ProcessName == "vlc")
                    this.ProcessList.Items.Add(p.ProcessName);
            }
        }
    }
}
