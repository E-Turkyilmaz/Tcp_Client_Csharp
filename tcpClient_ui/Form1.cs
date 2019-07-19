using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace tcpClient_ui
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public System.Net.Sockets.NetworkStream stream;
        public StreamReader STR;
        public StreamWriter STW;
        public System.Net.Sockets.TcpClient client;
        public String data = null;
        public string message;

 
        private void conBtn_Click(object sender, EventArgs e)
        {
           
            ConnectToServer();
            conBtn.Enabled = false;
            dcBtn.Enabled = true;
        }
        
        private void ConnectToServer()
        {
            try
            {
                Int32 port = Convert.ToInt32(portTextBox.Text);
                IPAddress ipAddress = IPAddress.Parse(ipTextBox.Text);

                if ((portTextBox.Text != "") && (ipTextBox.Text != ""))
                {
                    client = new TcpClient(ipAddress.ToString(), port);
                }
                else if(ipAddress == null)
                {
                    MessageBox.Show("Plesase Enter Connection Info!", "IP and Port missing!!!", 
                        MessageBoxButtons.OK ,MessageBoxIcon.Exclamation);
                }

                STW = new StreamWriter(client.GetStream());
                STR = new StreamReader(client.GetStream());
                STW.AutoFlush = true;
                receivingWorker.RunWorkerAsync();
                sendingWorker.WorkerSupportsCancellation = true;
                receivingWorker.WorkerSupportsCancellation = true;
            }

            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void sendBtn_Click(object sender, EventArgs e)
        {
            if (txtSend.Text != "")
            {
                message = txtSend.Text;
                sendingWorker.RunWorkerAsync();
            }

            txtSend.Text = "";
        }

        private void sendingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (InvokeRequired)
            {
                STW.WriteLine(message);

                this.txtReceive.Invoke(new MethodInvoker(delegate()
                    {
                        txtReceive.AppendText("\nSENT: " + message   + "\n");
                    }));
            }
            else
            {
                MessageBox.Show("Failed to send the message!");
            }

            sendingWorker.CancelAsync();
        }

        private void receivingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (client.Connected)
            {
                try
                {
                    data = STR.ReadLine();

                    this.txtSend.Invoke(new MethodInvoker(delegate()
                        {
                            if (data != "['][CLOSE][']\r\n")
                            {
                                txtReceive.AppendText("\nRECEIVED: " + data + "\n");
                            }
                        }));

                    if (data == "['][CLOSE][']\r\n")
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            STW.Close();
            STR.Close();
            receivingWorker.CancelAsync();
            client.Close();

            if (InvokeRequired)
            {
                this.dcBtn.Invoke(new MethodInvoker(delegate()
                    {
                        dcBtn.Enabled = false;
                    }));
                this.conBtn.Invoke(new MethodInvoker(delegate()
                    {
                        conBtn.Enabled = true;
                    }));
            }
        }

        private void dcBtn_Click(object sender, EventArgs e)
        {
            message = "['][CLOSE][']";
            STW.WriteLine(message);
            STW.Close();
            STR.Close();
            receivingWorker.CancelAsync();
            client.Close();            
            dcBtn.Enabled = false;
            conBtn.Enabled = true;
        }
    }
}
