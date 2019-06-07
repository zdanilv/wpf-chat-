using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Windows;
using System.Threading;

namespace chat
{
    class Client
    {
        public string message;
        TcpClient tcpClient = new TcpClient();
        MainWindow mw = new MainWindow();

        public void ClientObject(string ipaddress, int port)
        {
            try
            {
                tcpClient.Connect(ipaddress, port);
                Thread getMessageThread = new Thread(new ThreadStart(GetMessage));
                getMessageThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error", ex.Message);
            }
        }

        public void SendMessage(string message)
        {
            try
            {
                NetworkStream stream = tcpClient.GetStream();
                byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                stream.Dispose();
                stream.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error", ex.Message);
            }
        }

        public void GetMessage()
        {
            try
            {
                byte[] data = new byte[256];
                NetworkStream stream = tcpClient.GetStream();
                do
                {
                    int bytes = stream.Read(data, 0, data.Length); // получаем количество считанных байтов
                    string message = Encoding.UTF8.GetString(data, 0, bytes);
                    mw.addTextToRichTextBox(message);
                }
                while (stream.DataAvailable);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error", ex.Message);
            }
        }
    }
}
