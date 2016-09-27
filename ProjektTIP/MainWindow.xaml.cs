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


namespace ProjektTIP {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private const int listenPort = 11000;
        
        private Friends friends;

        private Socket sending_socket;
        private Socket sending_socket_audio;
        private IPAddress send_to_address;
        private IPEndPoint sending_end_point;
        private string audioPath = "test.vaw";

        private Friend user;

        private volatile bool connected;

        UdpClient listener_audio;
        IPEndPoint groupEP;

        private bool stopCall;

        public WaveIn waveSource = null;

        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        public MainWindow() {
            InitializeComponent();
            stopCall = false;
            sending_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            send_to_address = IPAddress.Parse("127.0.0.1");
            sending_end_point = new IPEndPoint(send_to_address, 11000);

            connected = false;


            friends = new Friends();
            var friend = new Friend("Buggi");
            friend.setName("Jakub", "Bugaj");
            friend.setAvatar("Azx");
            friend.Opinion = 5;
            friends.addFriend(friend);
            friends.addFriend("Damian");
            friends.addFriend("Wojtas");

            listFriends.ItemsSource = friends;
            listFriends.DisplayMemberPath = "Nick";

            //setFriendStars(3);

            user = new Friend("Zabka");
        }



        private void setFriendStars(int value)
        {
            if (value >= 0 && value <= 6)
            {
                imgStar1.Source = new BitmapImage(new Uri(@"\star-transparent.png", UriKind.Relative));
                imgStar2.Source = new BitmapImage(new Uri(@"\star-transparent.png", UriKind.Relative));
                imgStar3.Source = new BitmapImage(new Uri(@"\star-transparent.png", UriKind.Relative));
                imgStar4.Source = new BitmapImage(new Uri(@"\star-transparent.png", UriKind.Relative));
                imgStar5.Source = new BitmapImage(new Uri(@"\star-transparent.png", UriKind.Relative));
                imgStar6.Source = new BitmapImage(new Uri(@"\star-transparent.png", UriKind.Relative));




                for (var i = 0; i < value; i++)
                {
                    if (i == 0)
                    {
                        imgStar1.Source = new BitmapImage(new Uri(@"\star.png", UriKind.Relative));
                    }
                    if (i == 1)
                    {
                        imgStar2.Source = new BitmapImage(new Uri(@"\star.png", UriKind.Relative));
                    }
                    if (i == 2)
                    {
                        imgStar3.Source = new BitmapImage(new Uri(@"\star.png", UriKind.Relative));
                    }
                    if (i == 3)
                    {
                        imgStar4.Source = new BitmapImage(new Uri(@"\star.png", UriKind.Relative));
                    }
                    if (i == 4)
                    {
                        imgStar5.Source = new BitmapImage(new Uri(@"\star.png", UriKind.Relative));
                    }
                    if (i == 5)
                    {
                        imgStar6.Source = new BitmapImage(new Uri(@"\star.png", UriKind.Relative));
                    }
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
        private void bConnection_Click(object sender, RoutedEventArgs e) {

            if (!connected)
            {

                byte[] send_buffer = Encoding.ASCII.GetBytes("Hello");

                try
                {
                    AllocConsole();
                    Console.WriteLine("Wysłanie hello");
                    sending_socket.SendTo(send_buffer, sending_end_point);


                }
                catch (Exception send_exception)
                {
                    Console.WriteLine(" Exception {0}", send_exception.Message);
                }

                sending_socket_audio = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                sending_end_point = new IPEndPoint(send_to_address, 11122);

                waveSource = new WaveIn();
                waveSource.WaveFormat = new WaveFormat(48000,1);

                waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
                waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);


               waveSource.StartRecording();


                connected = true;

                bConnection.Content = "Rozłącz";
            }
            else
            {
                waveSource.StopRecording();
                connected = false;
                bConnection.Content = "Połącz";
            }
        }

        private void recive_UDP() {
            bool done = false;
            UdpClient listener = new UdpClient(listenPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);


            string received_data = "";
            byte[] receive_byte_array;

            AllocConsole();
            Console.WriteLine("Test");

            try {

                while (!done) {
                    receive_byte_array = listener.Receive(ref groupEP);
                    AllocConsole();
                    var recivedString = Encoding.ASCII.GetString(receive_byte_array);
                    Console.WriteLine(recivedString);

                    if (recivedString.Equals("Hello")) {

                        Thread viewerThread = new Thread(delegate ()
                        {
                            var callWindow = new CallWindow();
                            callWindow.Show();
                            System.Windows.Threading.Dispatcher.Run();
                        });

                        viewerThread.SetApartmentState(ApartmentState.STA); // needs to be STA or throws exception
                        viewerThread.Start();

                        sending_socket.SendTo(Encoding.ASCII.GetBytes("Invite"), sending_end_point);
                        var waudio = new Thread(new ThreadStart(recive_audio));
                        waudio.Start();
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        private void openCallWindow()
        {
            CallWindow callWindow = new CallWindow();
            callWindow.ShowDialog();
        }

        private void recive_audio() {
            UdpClient listener_audio = new UdpClient(11122);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 11122);

            WaveOut _waveOut = new WaveOut();

            var receive_byte_array = listener_audio.Receive(ref groupEP);
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
            var wstart = new Thread(new ThreadStart(recive_UDP));
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
            var selectedItem = (Friend)listFriends.SelectedItem;

            if (!String.IsNullOrEmpty(selectedItem.Name)) {
                freindLabel.Content = selectedItem.Name + ' ' + selectedItem.Last_name;
            }
            else {
                freindLabel.Content = selectedItem.Nick;
                freindImg.Source = new BitmapImage(new Uri(@"\avatar-man.png",UriKind.Relative));
            }

            setFriendStars(selectedItem.Opinion);

        }

        private void star_1_Click(object sender, RoutedEventArgs e)
        {
            if(listFriends.SelectedItem != null)
            {
                var friend = (Friend)listFriends.SelectedItem;
                friend.Opinion = 1;
                setFriendStars(friend.Opinion);
            }
        }

        private void star_2_Click(object sender, RoutedEventArgs e)
        {
            if (listFriends.SelectedItem != null)
            {
                var friend = (Friend)listFriends.SelectedItem;
                friend.Opinion = 2;
                setFriendStars(friend.Opinion);
            }

        }

        private void star_3_Click(object sender, RoutedEventArgs e)
        {
            if (listFriends.SelectedItem != null)
            {
                var friend = (Friend)listFriends.SelectedItem;
                friend.Opinion = 3;
                setFriendStars(friend.Opinion);
            }
        }

        private void star_4_Click(object sender, RoutedEventArgs e)
        {
            if (listFriends.SelectedItem != null)
            {
                var friend = (Friend)listFriends.SelectedItem;
                friend.Opinion = 4;
                setFriendStars(friend.Opinion);
            }
        }

        private void star_5_Click(object sender, RoutedEventArgs e)
        {
            if (listFriends.SelectedItem != null)
            {
                var friend = (Friend)listFriends.SelectedItem;
                friend.Opinion = 5;
                setFriendStars(friend.Opinion);
            }
        }

        private void star_6_Click(object sender, RoutedEventArgs e)
        {
            if (listFriends.SelectedItem != null)
            {
                var friend = (Friend)listFriends.SelectedItem;
                friend.Opinion = 6;
                setFriendStars(friend.Opinion);
            }
        }

        private void bSettings_Click(object sender, RoutedEventArgs e)
        {
            user.Name = "Jan";
            var settingWindow = new SettingWindow(ref user);
            settingWindow.ShowDialog();
        }
    }
}
