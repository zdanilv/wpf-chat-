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

namespace WinChat
{
    public partial class Form1 : Form
    {
        Thread server = null;
        Thread client = null;
        TcpClient tcpClient = null;
        TcpListener tcpListener = null;
        NetworkStream networkStream = null;
        static int serverOrClient = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            serverOrClient = 2;
            button2.Enabled = false;
            textBoxNick.Enabled = false;
            clientObject(textBoxIp.Text, Convert.ToInt32(textBoxPort.Text));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            serverOrClient = 1;
            button3.Enabled = false;
            textBoxNick.Enabled = false;
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
                if (serverOrClient == 1)
                {
                    IPAddress localAddr = IPAddress.Parse(textBoxIp.Text);
                    tcpListener = new TcpListener(localAddr, Convert.ToInt32(textBoxPort.Text));
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
            // if (server != null)
            //     server.Abort();
            // if (client != null)
            //     client.Abort();
        }
    }
}
