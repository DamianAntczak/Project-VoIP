using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProjektTIP
{
    /// <summary>
    /// Interaction logic for RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        private SharedClasses.User newUser;
        private bool validation;

        public RegistrationWindow()
        {
            InitializeComponent();
            newUser = new SharedClasses.User();
            this.DataContext = this.newUser;
            validation = true;
        }

        private void bRegister_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(tNazwisko.Text))
                validation = false;
            if (String.IsNullOrEmpty(tImie.Text))
                validation = false;
            if (String.IsNullOrEmpty(tLogin.Text))
                validation = false;
            if (!password.Password.Equals(passwordConfirm.Password))
                validation = false;


            if (validation == true)
            {

                newUser.PasswordHash = HashPassword(password.Password);

                string json = JsonConvert.SerializeObject(newUser);
                MessageBox.Show(json);

                var x = ConnectToServer(json);

            }
            else
            {
                MessageBox.Show("Niepoprawne dane");
            }


        }

        public string HashPassword(string ClientHashedPassword)
        {
            var byteArrayClientPassword = Encoding.UTF8.GetBytes(ClientHashedPassword);
            SHA256 sha = new SHA256Managed();
            sha.Initialize();
            var hashedPasswordBytes = sha.ComputeHash(byteArrayClientPassword);
            //var hashedPasswordString = Encoding.UTF8.GetString(hashedPasswordBytes);
            string result = "";
            foreach (var h in hashedPasswordBytes)
            {
                result += string.Format("{0:x2}", h);
            }

            sha.Clear();
            //Console.WriteLine(hashedPasss);
            return result;
        }

        async Task<bool> ConnectToServer(string json)
        {
            using (var tcpClient = new TcpClient())
            {
                await tcpClient.ConnectAsync(Settings.ServerAddress, Settings.ServerPort);
                var writer = new StreamWriter(tcpClient.GetStream(), Encoding.UTF8);
                var reader = new StreamReader(tcpClient.GetStream(), Encoding.UTF8);
                writer.WriteLine(json);
                var responseString = await reader.ReadLineAsync();
                return true;
            }
        }
    }
}
