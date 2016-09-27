﻿using System;
using System.Collections.Generic;
using System.Linq;
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
using Newtonsoft.Json;
using System.IO;
using System.Net.Sockets;
using System.Net;

namespace ProjektTIP
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private const int listenPort = 11001;
        private const string serverAdrres = "127.0.0.1";

        public LoginWindow()
        {
            InitializeComponent();
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

        private void bLogin_Click(object sender, RoutedEventArgs e)
        {
            if(textNick.Text.Equals("user") && passwordBox.Password.Equals("user"))
            {
                this.DialogResult = true;
                this.Close();
                var user = new SharedClasses.User();
                user.Login = textNick.Text;
                user.PasswordHash = HashPassword(passwordBox.Password);
                string json = JsonConvert.SerializeObject(user);
                MessageBox.Show(json);


                var x = ConnectToServer(json);

            }
            else
            {
                lInfo.Content = "Podano błędne dane";
            }
        }

        async Task<bool> ConnectToServer(string json)
        {
            using (var tcpClient = new TcpClient())
            {
                await tcpClient.ConnectAsync(IPAddress.Parse(serverAdrres), listenPort);
                var writer = new StreamWriter(tcpClient.GetStream(), Encoding.UTF8);
                var reader = new StreamReader(tcpClient.GetStream(), Encoding.UTF8);
                writer.WriteLine(json);
                var responseString = await reader.ReadLineAsync();
                return true;
            }
        }

    }
}