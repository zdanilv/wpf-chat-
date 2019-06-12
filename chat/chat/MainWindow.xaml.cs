using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Net;

namespace chat
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static int serverOrClient = 0;
        Server server = new Server();
        Client client = new Client();

        public MainWindow()
        {
            InitializeComponent();

            addMessageTextBox("Привет");
        }

        private void ButtonServer_Click(object sender, RoutedEventArgs e)
        {
            serverOrClient = 1;
            buttonConnect.IsEnabled = false;
            server.ServerObject(textboxIp.Text, Convert.ToInt32(textboxPort.Text));
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            serverOrClient = 2;
            buttonServer.IsEnabled = false;
            client.ClientObject(textboxIp.Text, Convert.ToInt32(textboxPort.Text));
        }

        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            if (serverOrClient == 1)
            {
                server.SendMessage(textboxNick.Text + ": " + textboxSend.Text);
            }
            if (serverOrClient == 2)
            {
                client.SendMessage(textboxNick.Text + ": " + textboxSend.Text);
            }
        }

        public void addMessageTextBox(string message)
        {
            getMessageTextBox.Dispatcher.BeginInvoke(new Action(delegate()
            {
                getMessageTextBox.AppendText(message);
            }));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            server.socket.Dispose();
            server.socket.Close();
            server.clientSocket.Dispose();
            server.clientSocket.Close();
            client.socket.Dispose();
            client.socket.Close();

            Environment.Exit(0);
        }
    }
}
