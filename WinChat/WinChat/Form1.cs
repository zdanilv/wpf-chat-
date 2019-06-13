using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using Open.Nat;

namespace WinChat
{
    public partial class Form1 : Form
    {

        Thread server = null;
        Thread client = null;
        TcpClient tcpClient = null;
        TcpListener tcpListener = null;
        NetworkStream networkStream = null;
        int serverOrClient = 0, port;
        string nick, ipaddress;

        public Form1()
        {
            InitializeComponent();
            FormClosing += Form1_FormClosing;
        }

        public async void OpenPort()
        {
            var discoverer = new NatDiscoverer();
            var cts = new CancellationTokenSource(10000);
            var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);

            await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, port, port, "WinChat"));
        }

        public void EnableCont()
        {
            button4.Enabled = true;
            button3.Enabled = true;
            button2.Enabled = true;
            button1.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            serverOrClient = 2;
            button2.Enabled = false;
            button3.Enabled = false;
           //textBoxNick.Enabled = false;
            //textBoxIp.Enabled = false;
           // textBoxPort.Enabled = false;

            nick = textBoxNick.Text;
            port = Convert.ToInt32(textBoxPort.Text);
            ipaddress = textBoxIp.Text;

            clientObject(ipaddress, port);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            serverOrClient = 1;
            button3.Enabled = false;
            button2.Enabled = false;
            //textBoxNick.Enabled = false;
            //textBoxIp.Enabled = false;
            //textBoxPort.Enabled = false;

            nick = textBoxNick.Text;
            port = Convert.ToInt32(textBoxPort.Text);
            ipaddress = textBoxIp.Text;

            server = new Thread(serverObject);
            server.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBoxSend.Text == "")
                MessageBox.Show("Введите текст!");
            else
            {
                sendMessage(textBoxSend.Text);
                textBoxSend.Clear();
            }
        }

        public void serverObject()
        {
            try
            {
                OpenPort();

                if (serverOrClient == 1)
                {
                    IPAddress localAddr = IPAddress.Parse(ipaddress);
                    tcpListener = new TcpListener(localAddr, port);
                    tcpListener.Start();

                    TcpClient tcpClientServer = tcpListener.AcceptTcpClient();
                    networkStream = tcpClientServer.GetStream();
                    this.Invoke(new Action(() => { richTextBox.Text = richTextBox.Text + Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " " + "Соединение установлено!"; }));
                }

                byte[] buffer = new byte[1024];

                while (true)
                {
                    int bytes = networkStream.Read(buffer, 0, buffer.Length);
                    if(bytes > 0)
                    {
                        string message = Encoding.Unicode.GetString(buffer);
                        this.Invoke(new Action(() => { richTextBox.Text = richTextBox.Text + Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " " + message; }));
                        Array.Clear(buffer, 0, buffer.Length);
                        message = "";
                    }
                    else
                    {
                        this.Invoke(new Action(() => { richTextBox.Text = richTextBox.Text + Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " " + "Клиент отключился!"; }));
                        Disconnect();
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void clientObject(string ip, int port)
        {
            try
            {
                tcpClient = new TcpClient();
                this.Invoke(new Action(() => { richTextBox.Text = richTextBox.Text + Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " " + "подключение к " + ipaddress + ":" + port; }));
                tcpClient.Connect(ip, port);
                this.Invoke(new Action(() => { richTextBox.Text = richTextBox.Text + Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " " + "Соединение установлено!"; }));

                networkStream = tcpClient.GetStream();
                client = new Thread(serverObject);
                client.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void sendMessage(string message)
        {
            try
            {
                message = textBoxNick.Text + ": " + message;
                richTextBox.Text = richTextBox.Text + Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " " + message;
                byte[] buffer = Encoding.Unicode.GetBytes(message);
                networkStream.Write(buffer, 0, buffer.Length);
                Array.Clear(buffer, 0, buffer.Length);
                message = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void Disconnect()
        {
            try
            {
                EnableCont();
                this.Invoke(new Action(() => { richTextBox.Text = richTextBox.Text + Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " " + "Отключение..."; }));

                networkStream.Dispose();
                networkStream.Close();

                if (tcpListener != null)
                    tcpListener.Stop();
                if (tcpClient != null)
                    tcpClient.Close();

                this.Invoke(new Action(() => { richTextBox.Text = richTextBox.Text + Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " " + "Отключено."; }));
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Disconnect();
            Environment.Exit(0);
        }
    }
}
