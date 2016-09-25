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
using System.Windows.Media.Imaging;

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

        private volatile bool connected;

        UdpClient listener_audio;
        IPEndPoint groupEP;

        private bool stopCall;

        public WaveIn waveSource = null;
        public WaveFileWriter waveFile = null;

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
            friends.addFriend(friend);
            friends.addFriend("Damian");
            friends.addFriend("Wojtas");

            listFriends.ItemsSource = friends;
            listFriends.DisplayMemberPath = "Nick";

            setFriendStars(3);
        }

        private void setFriendStars(int value)
        {
            if (value > 0 && value < 6)
            {
                star_1.Visibility = Visibility.Hidden;
                star_2.Visibility = Visibility.Hidden;
                star_3.Visibility = Visibility.Hidden;
                star_4.Visibility = Visibility.Hidden;
                star_5.Visibility = Visibility.Hidden;
                star_6.Visibility = Visibility.Hidden;

                for (var i = 0; i < value; i++)
                {
                    if (i == 0)
                        star_1.Visibility = Visibility.Visible;
                    if (i == 1)
                        star_2.Visibility = Visibility.Visible;
                    if (i == 2)
                        star_3.Visibility = Visibility.Visible;
                    if (i == 3)
                        star_4.Visibility = Visibility.Visible;
                    if (i == 4)
                        star_5.Visibility = Visibility.Visible;
                    if (i == 5)
                        star_6.Visibility = Visibility.Visible;
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
                waveSource.WaveFormat = new WaveFormat(44100, 1);

                waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
                waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);

                waveFile = new WaveFileWriter(audioPath, waveSource.WaveFormat);

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
            if (waveFile != null) {
                waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                waveFile.Flush();
                Console.WriteLine("Wysłanie pakietu");
                sending_socket.SendTo(e.Buffer, sending_end_point);
            }
        }

        void waveSource_RecordingStopped(object sender, StoppedEventArgs e) {
            if (waveSource != null) {
                waveSource.Dispose();
                waveSource = null;
            }

            if (waveFile != null) {
                waveFile.Dispose();
                waveFile = null;
            }

        }

        private void bBye_Click(object sender, RoutedEventArgs e) {
            waveSource.StopRecording();
        }

        private void listFriends_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            var selectedItem = (Friend)listFriends.SelectedItem;

            if (!String.IsNullOrEmpty(selectedItem.Name)) {
                freindLabel.Content = selectedItem.Name + ' ' + selectedItem.Last_name;
            }
            else {
                freindLabel.Content = selectedItem.Nick;
                freindImg.Source = new BitmapImage(new Uri("C:\\avatar-man.png"));


            }

        }
    }
}
