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
        public Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static MainWindow mw = new MainWindow();

        public void ClientObject(string ipaddress, int port)
        {
            try
            {
                socket.Connect(ipaddress, port);
                mw.addMessageTextBox("Подключение прошло успешно.");
                Thread getMessageThread = new Thread(new ThreadStart(serverStart));
                getMessageThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error", ex.Message);
                socket.Close();
            }
        }

        public void SendMessage(string message)
        {
            try
            {
                byte[] buffer = System.Text.Encoding.Unicode.GetBytes(message);
                socket.Send(buffer);
                mw.addMessageTextBox(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error", ex.Message);
                socket.Close();
            }
        }

        public void serverStart()
        {
            try
            {
                byte[] buffer = new byte[1024];
                while (true)
                {
                    socket.Receive(buffer);
                    string message = Encoding.Unicode.GetString(buffer);
                    mw.addMessageTextBox(message);
                    Array.Clear(buffer, 0, 1024);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error", ex.Message);
                socket.Close();
            }
        }
    }
}
