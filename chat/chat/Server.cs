using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Windows;

namespace chat
{
    class Server
    {
        TcpClient tcpClient;
        TcpListener tcpListener;
        public void ServerObject(string ip, int port)
        {
            try
            {
                IPAddress iPAddress = IPAddress.Parse(ip);
                tcpListener = new TcpListener(iPAddress, port);
                Thread serverThread = new Thread(new ThreadStart(serverStart));
                serverThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка создания сервера");
            }
        }

        public void serverStart()
        {

            MainWindow mw = new MainWindow();
            tcpListener.Start();
            tcpClient = tcpListener.AcceptTcpClient();
            mw.addTextToRichTextBox("Клиент подключен.");
            NetworkStream networkStream = tcpClient.GetStream();
            byte[] data = new byte[256];

            while (true)
            {
                int bytes = networkStream.Read(data, 0, data.Length); // получаем количество считанных байтов
                string message = Encoding.UTF8.GetString(data, 0, bytes);
                //mw.addTextToRichTextBox(message);
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
    }
}
