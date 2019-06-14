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
using Open.Nat;                                                                                                                     // Библиотека которая помогает перебросить порты (NuGet: Open.Nat)

namespace WinChat
{
    public partial class Form1 : Form
    {

        Thread server = null;                                                                                                       // Объявляем поток для сервера
        Thread client = null;                                                                                                       // Объявляем поток для клиента
        TcpClient tcpClient = null;                                                                                                 // Объявляем сам клиент
        TcpListener tcpListener = null;                                                                                             // Объявляем слушатель (сервер)
        NetworkStream networkStream = null;                                                                                         // Объявляем поток данных
        int serverOrClient = 0, port;                                                                                               // Объявляем переменную для проверки(сервер или клиент) и порт
        string nick, ipaddress;                                                                                                     // Объявляем строки ника и ip-адреса

        public Form1()
        {
            InitializeComponent();
            FormClosing += Form1_FormClosing;                                                                                       // Объявляем событие закрытие формы
            buttonDisconnect.Enabled = false;                                                                                       // Выключаем кнопку Disconnect
        }

        public async void OpenPort() // Открываем(перебрасываем) порты
        {
            try
            {
                var discoverer = new NatDiscoverer();
                var cts = new CancellationTokenSource(10000);                                                                       // После 10 секунд ожидания операция завершается (если ничего не произошло, т.е. порты не перебросились)
                var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);

                await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, port, port, "WinChat"));                                  // Открываем порт по протоколу tcp, *8888 (сам порт), 
            }                                                                                                                       //*8888 (сам порт), и название (нужно для роутера)
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void EnableCont()
        {
            this.Invoke(new Action(() => { buttonDisconnect.Enabled = false; }));
            this.Invoke(new Action(() => { buttonConnect.Enabled = true; }));
            this.Invoke(new Action(() => { buttonServer.Enabled = true; }));
            this.Invoke(new Action(() => { buttonSendMessage.Enabled = true; }));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            serverOrClient = 2;
            buttonDisconnect.Enabled = true;
            buttonConnect.Enabled = false;
            buttonServer.Enabled = false;

            nick = textBoxNick.Text;
            port = Convert.ToInt32(textBoxPort.Text);
            ipaddress = textBoxIp.Text;

            clientObject(ipaddress, port);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            serverOrClient = 1;
            buttonDisconnect.Enabled = true;
            buttonConnect.Enabled = false;
            buttonServer.Enabled = false;

            nick = textBoxNick.Text;
            port = Convert.ToInt32(textBoxPort.Text);
            ipaddress = textBoxIp.Text;

            server = new Thread(serverObject);                                                                                      // Определяем поток server
            server.Start();                                                                                                         // Запускаем поток server
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBoxSend.Text == "")                                                                                             // Если textBoxSend пуст, то выдает сообщение 
            {
                MessageBox.Show("Введите текст!");
            }
            else
            {
                sendMessage(textBoxSend.Text);                                                                                      // Запускаем метод sendMEssage(наше сообщение)
                textBoxSend.Clear();                                                                                                // Очищаем поле textBoxSend
            }
        }

        public void serverObject()                                                                                                  // Метод сервера универсален, подходит и для TcpListener(сервера) и TcpClient(клиента)
        {
            try
            {
                try
                {
                    OpenPort();                                                                                                     // Перебрасываем порты

                    if (serverOrClient == 1)                                                                                        // Если выбран "Server"
                    {
                        IPAddress localAddr = IPAddress.Parse(ipaddress);                                                                  // Server: определяем ipadress как Ip-адрес
                        tcpListener = new TcpListener(localAddr, port);                                                                    // Инициализируем класс TcpListener (Слушателя/сервера)
                        tcpListener.Start();                                                                                               // Запускаем слушатель/сервер

                        this.Invoke(new Action(() => { richTextBox.Text = richTextBox.Text
                            + Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " " + "Ожидаем подключение..."; }));               // Выводим сообщение 

                        TcpClient tcpClientServer = tcpListener.AcceptTcpClient();                                                         // Когда к серверу обращается клиент, мы используем AcceptTcpClient для получения соответственно объекта TcpClient, который будет использоваться для взаимодействия с подключенным клиентом
                        networkStream = tcpClientServer.GetStream();                                                                       // Чтобы взаимодействовать с клиентом/сервером TcpClient определяет метод GetStream(), который возвращает объект NetworkStream. Через данный объект можно передавать сообщения серверу или, наоборот, получать данные с сервера

                        this.Invoke(new Action(() => { richTextBox.Text = richTextBox.Text
                            + Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " " + "Соединение установлено!"; }));              // Выводим сообщение
                    }

                    byte[] buffer = new byte[1024];                                                                                 // Объявляем массив байтов buffer размером 1024 байт

                    while (true)                                                                                                    // Дальше безразницы выбран Server или Client
                    {
                        int bytes = networkStream.Read(buffer, 0, buffer.Length);                                                   // Считываем данные с потока данных и заносим кол-во байтов в bytes 
                        if (bytes > 0)                                                                                              // Если байтов в сообщении больше 0
                        {
                            string message = Encoding.Unicode.GetString(buffer);                                                           // Получаем строку из массива байтов

                            this.Invoke(new Action(() => { richTextBox.Text = richTextBox.Text
                                + Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " " + message; }));                            // Выводим полученую строку

                            Array.Clear(buffer, 0, buffer.Length);                                                                         // Очищаем массив байтов
                            message = "";                                                                                                  // Очищаем строку 
                        }
                        else                                                                                                               // Когда кто-то (Client/Server) отключается начинают приходить байты размеров 0 байт, тут мы и ловим отключение (Client/Server)
                        {
                            this.Invoke(new Action(() => { richTextBox.Text = richTextBox.Text
                                + Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " " + "Клиент отключился!"; }));               // Выводим сообщение

                            Disconnect();                                                                                                  // Вызов метода Disconnect
                            break;                                                                                                         // Выходим из цикла
                        }
                    }
                }
                catch (System.IO.IOException ex) { }
            }
            catch(SocketException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void clientObject(string ip, int port)
        {
            tcpClient = new TcpClient();                                                                                            // Инициализируем клиента

            this.Invoke(new Action(() => { richTextBox.Text = richTextBox.Text + Environment.NewLine
                + DateTime.Now.ToString("HH:mm:ss") + " " + "подключение к " + ipaddress + ":" + port; }));

            tcpClient.Connect(ip, port);

            this.Invoke(new Action(() => { richTextBox.Text = richTextBox.Text
                + Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " " + "Соединение установлено!"; }));

            networkStream = tcpClient.GetStream();

            client = new Thread(serverObject);
            client.Start();
        }

        public void sendMessage(string message)
        {
            message = textBoxNick.Text + ": " + message;
            richTextBox.Text = richTextBox.Text + Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " " + message;

            byte[] buffer = Encoding.Unicode.GetBytes(message);
            networkStream.Write(buffer, 0, buffer.Length);

            Array.Clear(buffer, 0, buffer.Length);
            message = "";
        }

        public void Disconnect()
        {
            EnableCont();

            if(networkStream != null)
            {
                this.Invoke(new Action(() => { richTextBox.Text = richTextBox.Text
                    + Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " " + "Отключение..."; }));

                networkStream.Dispose();
                networkStream.Close();
            }

            if (tcpListener != null)
            {
                tcpListener.Stop();
            }
                
            if (tcpClient != null)
            {
                tcpClient.Close();
                this.Invoke(new Action(() => { richTextBox.Text = richTextBox.Text
                    + Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " " + "Отключено."; }));
            }
                
            tcpListener = null;
            tcpClient = null;
        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Disconnect();
            Environment.Exit(0);
        }
    }
}
