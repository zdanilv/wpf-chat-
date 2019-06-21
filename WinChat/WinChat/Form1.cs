using System;
using System.Text;
using System.Threading;
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

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
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

        private void buttonServer_Click(object sender, EventArgs e)
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

        private void buttonSendMessage_Click(object sender, EventArgs e)
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

        public void serverObject() // Метод сервера универсален, подходит и для TcpListener(сервера) и TcpClient(клиента)
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

        public void clientObject(string ip, int port) // Метод подключения клиента к серверу
        {
            tcpClient = new TcpClient();                                                                                            // Инициализируем клиента

            this.Invoke(new Action(() => { richTextBox.Text = richTextBox.Text + Environment.NewLine
                + DateTime.Now.ToString("HH:mm:ss") + " " + "подключение к " + ipaddress + ":" + port; }));                         // Выводим сообщение

            tcpClient.Connect(ip, port);                                                                                            // Подключаемся к серверу

            this.Invoke(new Action(() => { richTextBox.Text = richTextBox.Text
                + Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " " + "Соединение установлено!"; }));                   // Выводим сообщение

            networkStream = tcpClient.GetStream();                                                                                  // Чтобы взаимодействовать с клиентом/сервером TcpClient определяет метод GetStream(), который возвращает объект NetworkStream. Через данный объект можно передавать сообщения серверу или, наоборот, получать данные с сервера

            client = new Thread(serverObject);                                                                                      // Инициализируем поток client связываем его с serverObject для получения данных 
            client.Start();                                                                                                         // Запускаем поток
        }

        public void sendMessage(string message) // Метод отправки сообщений
        {
            message = textBoxNick.Text + ": " + message;                                                                            // Добавляем к сообщению никнейм
            richTextBox.Text = richTextBox.Text + Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " " + message;          // Выводим сообщение в richTextBox

            byte[] buffer = Encoding.Unicode.GetBytes(message);                                                                     // Преобразуем строку сообщения в массив байтов
            networkStream.Write(buffer, 0, buffer.Length);                                                                          // Записываем массив байтов в поток данных

            Array.Clear(buffer, 0, buffer.Length);                                                                                  // Очищаем массив байтов
            message = "";                                                                                                           // Очищаем строку
        }

        public void Disconnect()
        {
            EnableCont();                                                                                                           // Включаем кнопки

            if (networkStream != null)                                                                                              // Проверяем инициализирован ли поток данных
            {
                this.Invoke(new Action(() => { richTextBox.Text = richTextBox.Text
                    + Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " " + "Отключение..."; }));                                // Выводим сообщение

                networkStream.Dispose();                                                                                                   // Освобождаем ресурсы занятые потоком данных
                networkStream.Close();                                                                                                     // Закрываем поток данных
            }

            if (tcpListener != null)                                                                                                // Проверяем инициализирован ли TcpListener (Server)
            {
                tcpListener.Stop();                                                                                                        // Закрываем TcpListener (Server)
            }
                
            if (tcpClient != null)                                                                                                  // Проверяем инициализирован ли TcpClient (Client)
            {
                tcpClient.Close();                                                                                                         // Закрываем TcpClient (Client)
                this.Invoke(new Action(() => { richTextBox.Text = richTextBox.Text
                    + Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " " + "Отключено."; }));                                   // Выводим сообщение
            }
                
            tcpListener = null;                                                                                                     // Приравниваем TcpListener (Server) к null (Чтобы можно было повторно создать подключение)
            tcpClient = null;                                                                                                       // Приравниваем TcpClient (Client) к null (Чтобы можно было повторно создать подключение)
        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e) // Событие закрытия окна
        {
            Disconnect();
            Environment.Exit(0);                                                                                                    // Выход из программы
        }
    }
}
