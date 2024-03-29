﻿using System;
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
        public Socket clientSocket;
        public Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static MainWindow mw = new MainWindow();

        public void ServerObject(string ip, int port)
        {
            mw.addMessageTextBox.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                mw.addMessageTextBox.AppendText("Ожидаем подключение...");
            }));

            try
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
                mw.getMessageTextBox.Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    mw.getMessageTextBox.AppendText("Ожидаем подключение...");
                }));
                socket.Listen(1);

                clientSocket = socket.Accept();
                Thread serverThread = new Thread(new ThreadStart(serverStart));
                serverThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка создания сервера");
                clientSocket.Close();
                socket.Close();
            }
        }

        public void serverStart()
        {
            byte[] buffer = new byte[1024];
            mw.getMessageTextBox.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                mw.getMessageTextBox.AppendText("Клиент подключен.");
            }));



            while (true)
            {
                clientSocket.Receive(buffer);
                string message = Encoding.Unicode.GetString(buffer);

                mw.getMessageTextBox.Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    mw.getMessageTextBox.AppendText(message);
                }));
                Array.Clear(buffer, 0, 1024);
            }
        }

        public void SendMessage(string message)
        {
            try
            {
                byte[] buffer = System.Text.Encoding.Unicode.GetBytes(message);
                clientSocket.Send(buffer);

                mw.getMessageTextBox.Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    mw.getMessageTextBox.AppendText(message);
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error", ex.Message);
                clientSocket.Close();
                socket.Close();
            }
        }
    }
}
