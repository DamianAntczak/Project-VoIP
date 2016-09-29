using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SharedClasses;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProjektTIP {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private const int listenPort = 11000;
        private const int listenTCPPort = 11112;

        private static string RETURN_NO = "NO";
        private static string RETURN_OK = "OK";


        private Friends friends;

        private Socket sending_socket;
        private Socket sending_socket_audio;
        private IPAddress send_to_address;
        private IPEndPoint sending_end_point;
        private string audioPath = "test.vaw";

        private Friend user;
        private UserLogin myLogin;

        private Random rnd;

        private volatile bool connected;

        UdpClient listener_audio;
        IPEndPoint groupEP;
        private TcpListener tcpListener;

        private bool stopCall;
        private bool friendIsCalling = false;
        private Thread wstart;

        public WaveIn waveSource = null;

        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        public MainWindow() {
            InitializeComponent();
            stopCall = false;
            sending_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //send_to_address = IPAddress.Parse("127.0.0.1");
            //sending_end_point = new IPEndPoint(send_to_address, 11000);

            connected = false;


            //setFriendStars(3);

            user = new Friend("Zabka");

            rnd = new Random();

            var loginWindow = new LoginWindow();

            myLogin = loginWindow.ShowLoginDialog();


            if (myLogin.Id == 0) {
                Application.Current.Shutdown();
            }

            var opis = String.IsNullOrEmpty(myLogin.Description) ? "Brak opisu!" : myLogin.Description;
            lDescription.Content = myLogin.Login + " " + lDescription.Content;
            tDescription.Text = opis;

            listFriends.ItemsSource = myLogin.Friends;
            listFriends.DisplayMemberPath = "Login";
            tcpListener = new TcpListener(IPAddress.Any, listenTCPPort);
            tcpListener.Start();
            CallRequestListener();

        }

        private async void CallRequestListener() {
            bool acceptCall = false;

            using (var tcpClient = await tcpListener.AcceptTcpClientAsync()) {
                var ns = tcpClient.GetStream();
                var reader = new StreamReader(ns, Encoding.UTF8);
                var writer = new StreamWriter(ns, Encoding.UTF8);
                writer.AutoFlush = true;
                var json = await reader.ReadLineAsync();
                JsonClassResponse<UserInfo> incomingCall
                    = JsonConvert.DeserializeObject<JsonClassResponse<UserInfo>>(json);
                send_to_address = IPAddress.Parse(incomingCall.Response.ActualIP);
                sending_end_point = new IPEndPoint(send_to_address, listenPort);
                acceptCall = true;
                if (acceptCall) {
                    writer.WriteLine(JsonConvert.SerializeObject(new JsonClassResponse<string>() {
                        RID = incomingCall.RID,
                        RequestCode = (int)RequestsCodes.OK,
                        Response = RETURN_OK
                    }));
                    StartSendUDP();
                    StartListenUDP();
                }


            }
        }

        private void setFriendStars(int value) {
            if (value >= 0 && value <= 6) {
                imgStar1.Source = new BitmapImage(new Uri(@"\star-transparent.png", UriKind.Relative));
                imgStar2.Source = new BitmapImage(new Uri(@"\star-transparent.png", UriKind.Relative));
                imgStar3.Source = new BitmapImage(new Uri(@"\star-transparent.png", UriKind.Relative));
                imgStar4.Source = new BitmapImage(new Uri(@"\star-transparent.png", UriKind.Relative));
                imgStar5.Source = new BitmapImage(new Uri(@"\star-transparent.png", UriKind.Relative));
                imgStar6.Source = new BitmapImage(new Uri(@"\star-transparent.png", UriKind.Relative));




                for (var i = 0; i < value; i++) {
                    if (i == 0) {
                        imgStar1.Source = new BitmapImage(new Uri(@"\star.png", UriKind.Relative));
                    }
                    if (i == 1) {
                        imgStar2.Source = new BitmapImage(new Uri(@"\star.png", UriKind.Relative));
                    }
                    if (i == 2) {
                        imgStar3.Source = new BitmapImage(new Uri(@"\star.png", UriKind.Relative));
                    }
                    if (i == 3) {
                        imgStar4.Source = new BitmapImage(new Uri(@"\star.png", UriKind.Relative));
                    }
                    if (i == 4) {
                        imgStar5.Source = new BitmapImage(new Uri(@"\star.png", UriKind.Relative));
                    }
                    if (i == 5) {
                        imgStar6.Source = new BitmapImage(new Uri(@"\star.png", UriKind.Relative));
                    }
                }
            }
            else {
                throw new InvalidOperationException();
            }
        }

        private async void bConnection_Click(object sender, RoutedEventArgs e) {

            if (!connected) {
                var selectedFriend = (UserInfo)listFriends.SelectedItem;

                if (selectedFriend != null) {

                    JsonClassRequest request = new JsonClassRequest() {
                        RID = rnd.Next(1000, 5000),
                        RequestCode = (int)RequestsCodes.CALL,
                        Parameters = new List<string>() { myLogin.SessionID.ToString(), myLogin.Id.ToString(), selectedFriend.Id.ToString() }
                    };
                    string json = JsonConvert.SerializeObject(request);
                    var x = await ConnectToServer(json);
                    var response = JsonConvert.DeserializeObject<JsonClassResponse<UserInfo>>(x);
                    if (response.RequestCode == (int)RequestsCodes.OK) {
                        send_to_address = IPAddress.Parse(response.Response.ActualIP);
                        StartSendUDP();
                        StartListenUDP();
                    }


                }
                else {
                    MessageBox.Show("Nie wybrano użytkownika");
                }
            }
            else {
                waveSource.StopRecording();
                connected = false;
                bConnection.Content = "Połącz";
            }

        }

        private void StartSendUDP() {
            sending_socket_audio = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sending_end_point = new IPEndPoint(send_to_address, listenPort);

            waveSource = new WaveIn();
            waveSource.WaveFormat = new WaveFormat(48000, 1);

            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
            waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);


            waveSource.StartRecording();


            connected = true;

            bConnection.Content = "Rozłącz";
        }


        private async void recive_UDP() {


            byte[] receive_byte_array;

            Console.WriteLine("Test");

            try {

                AllocConsole();

                Thread viewerThread = new Thread(delegate () {
                    var callWindow = new CallWindow();
                    callWindow.Show();
                    System.Windows.Threading.Dispatcher.Run();
                });

                viewerThread.SetApartmentState(ApartmentState.STA); // needs to be STA or throws exception
                viewerThread.Start();
                var waudio = new Thread(new ThreadStart(recive_audio));
                waudio.Start();
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        private void openCallWindow() {
            CallWindow callWindow = new CallWindow();
            callWindow.ShowDialog();
        }

        private async void recive_audio() {
            UdpClient listener_audio = new UdpClient(listenPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);

            WaveOut _waveOut = new WaveOut();

            var udp = await listener_audio.ReceiveAsync();
            var receive_byte_array = udp.Buffer;
            Console.WriteLine(receive_byte_array);
            IWaveProvider provider = new RawSourceWaveStream(
                     new MemoryStream(receive_byte_array), new WaveFormat(44100, 1));

            while (!stopCall) {
                receive_byte_array = listener_audio.Receive(ref groupEP);
                Console.WriteLine(receive_byte_array);
                provider = new RawSourceWaveStream(
                         new MemoryStream(receive_byte_array), new WaveFormat(44100, 1));

                _waveOut.Init(provider);
                _waveOut.Play();
            }

        }

        private void bAvalible_Click(object sender, RoutedEventArgs e) {
            StartListenUDP();
        }

        private void StartListenUDP() {
            wstart = new Thread(new ThreadStart(recive_UDP));
            wstart.Start();
        }

        void waveSource_DataAvailable(object sender, WaveInEventArgs e) {
            Console.WriteLine("Wysłanie pakietu");
            sending_socket.SendTo(e.Buffer, sending_end_point);

        }

        void waveSource_RecordingStopped(object sender, StoppedEventArgs e) {
            if (waveSource != null) {
                waveSource.Dispose();
                waveSource = null;
            }

        }


        private void listFriends_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            var selectedItem = (UserInfo)listFriends.SelectedItem;

            if (!String.IsNullOrEmpty(selectedItem.Name)) {
                freindLabel.Content = selectedItem.Name + ' ' + selectedItem.SecondName;
            }
            else {
                freindLabel.Content = selectedItem.Login;
                freindImg.Source = new BitmapImage(new Uri(@"\avatar-man.png", UriKind.Relative));
            }

            setFriendStars(0);

        }

        private void star_1_Click(object sender, RoutedEventArgs e) {
            if (listFriends.SelectedItem != null) {
                //var friend = (Friend)listFriends.SelectedItem;
                //friend.Opinion = 1;
                //setFriendStars(friend.Opinion);
            }
        }

        private void star_2_Click(object sender, RoutedEventArgs e) {
            if (listFriends.SelectedItem != null) {
                //var friend = (Friend)listFriends.SelectedItem;
                //friend.Opinion = 2;
                //setFriendStars(friend.Opinion);
            }

        }

        private void star_3_Click(object sender, RoutedEventArgs e) {
            if (listFriends.SelectedItem != null) {
                //var friend = (Friend)listFriends.SelectedItem;
                //friend.Opinion = 3;
                //setFriendStars(friend.Opinion);
            }
        }

        private void star_4_Click(object sender, RoutedEventArgs e) {
            if (listFriends.SelectedItem != null) {
                //var friend = (Friend)listFriends.SelectedItem;
                //friend.Opinion = 4;
                //setFriendStars(friend.Opinion);
            }
        }

        private void star_5_Click(object sender, RoutedEventArgs e) {
            if (listFriends.SelectedItem != null) {
                //var friend = (Friend)listFriends.SelectedItem;
                //friend.Opinion = 5;
                //setFriendStars(friend.Opinion);
            }
        }

        private void star_6_Click(object sender, RoutedEventArgs e) {
            if (listFriends.SelectedItem != null) {
                //var friend = (Friend)listFriends.SelectedItem;
                //friend.Opinion = 6;
                //setFriendStars(friend.Opinion);
            }
        }

        private void bSettings_Click(object sender, RoutedEventArgs e) {
            var settingWindow = new SettingWindow(ref myLogin);
            settingWindow.ShowDialog();
        }


        async Task<string> ConnectToServer(string json) {
            using (var tcpClient = new TcpClient()) {
                try {

                    await tcpClient.ConnectAsync(Settings.ServerAddress, Settings.ServerPort);
                    var writer = new StreamWriter(tcpClient.GetStream(), Encoding.UTF8);
                    writer.AutoFlush = true;
                    var reader = new StreamReader(tcpClient.GetStream(), Encoding.UTF8);
                    await writer.WriteLineAsync(json);
                    var responseString = await reader.ReadLineAsync();
                    return responseString;

                }
                catch (SocketException e) {
                    return "error";
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            tcpListener.Stop();
        }
    }
}
