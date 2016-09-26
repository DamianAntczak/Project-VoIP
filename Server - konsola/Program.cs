using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Net.Sockets;
using System.Net;
using LiteDB;
using System.Security;
using System.Security.Cryptography;
namespace Server___konsola {
    class Program {
        private const int listenPort = 11001;
        static ClientPool clientPool;
        static TcpListener tcpListener;
        static string dbName = "UserDB";
        static LiteDatabase dataBase;
        static LiteCollection<User> dataBaseCollection;
        static UserDatabaseOperations userDatabaseOperations;
        const string odebrano = "ODEBRANO";
        static void Main(string[] args) {
            clientPool = new ClientPool();
            tcpListener = new TcpListener(IPAddress.Any, listenPort);
            dataBase = new LiteDatabase(dbName);
            dataBaseCollection = dataBase.GetCollection<User>("users");
            //BazaInit();
            userDatabaseOperations = new UserDatabaseOperations(dataBase, dataBaseCollection);
            var s = userDatabaseOperations.HashPassword("asdfasddlfkassjdflaskdfjassldkfjasdieuvj32kj3 adfsdfasdf j930234jfladsfkja");
            File.WriteAllText("elo.txt", s);
            Console.WriteLine(s);

            var g = Guid.NewGuid();
            var g2 = new Guid();
            Console.WriteLine("{0}\n{1}", g.ToString(), g2.ToString());

            List<int> list = new List<int> { 0, 1, 2, 3, 4 };
            var i = list.IndexOf(3);
            var i2 = list.IndexOf(6);
            var i33 = list.Where(x => x < 4);

            //tcpListener.Start();
            //dataBase = new LiteDatabase("DB.db");
            //dataBaseCollection = dataBase.GetCollection<User>("users");
            //Listener();

            //users.Clear();
            //using (var db = new LiteDatabase("DB.db")) {
            //    db.GetDatabaseInfo();
            //    var b = db.CollectionExists("users");
            //    var col = db.GetCollection("users");
            //    var i = col.Count();
            //    var w = col.FindAll();
            //    foreach (var u in w) {
            //        Console.WriteLine(u.ToString());
            //    }
            //}
            // BazaInit();
            Console.ReadKey();
            //tcpListener.Stop();
        }

        static public async Task<string> Listener() {
            Console.WriteLine("Czekam");
            SecureString ss = new SecureString();
            using (var tcpClient = await tcpListener.AcceptTcpClientAsync()) {
                if (tcpClient.Connected) {
                    while (true) {
                        var reader = new StreamReader(tcpClient.GetStream(), Encoding.UTF8);
                        var line = await reader.ReadLineAsync();
                        var commands = line.Split(" ".ToCharArray());
                        foreach (var item in commands) {
                            Console.WriteLine(item);
                        }
                        if (line != null)
                            Console.WriteLine(line);
                        else {//pusta linia = koniec?
                            var writer = new StreamWriter(tcpClient.GetStream(), Encoding.UTF8);
                            await writer.WriteLineAsync(odebrano);
                            break;
                        }
                    }
                }
            }
            Console.WriteLine("Połączono!");
            return "1";
        }
        static public void BazaInit() {
            // BAZA DANYCH
            Console.WriteLine("Inicjalizacja bazy");
            List<User> userList = new List<User>();
            userList.Add(new User { Name = "N", SecondName = "SN", Login = "L", PasswordHash = HashPassword("PASS"), Description = "OPIS", ActualIP = string.Empty, SessionID = new Guid() });
            userList.Add(new User { Name = "2N", SecondName = "2SN", Login = "2l", PasswordHash = HashPassword("PASS"), Description = "OPISSSSSSSSSS", Friends = new List<int>() { 1 }, ActualIP = string.Empty, SessionID = new Guid() });
            userList.Add(new User { Name = "3N", SecondName = "3SN", Login = "3L", PasswordHash = HashPassword("PASS"), Description = "COS", ActualIP = string.Empty, SessionID = new Guid() });
            userList.Add(new User { Name = "3N", SecondName = "3SN", Login = "4L", PasswordHash = HashPassword("PASS"), Description = "COS", ActualIP = string.Empty, SessionID = new Guid() });
            using (var db = new LiteDatabase(dbName)) {
                var users = db.GetCollection<User>("users");
                /// ustawienie unikatowej wartości. 
                users.EnsureIndex(x => x.Login, true);
                foreach (var u in userList) {

                    try {
                        users.Insert(u);
                    }
                    catch (LiteException le) {
                        Console.WriteLine(le.Message);
                    }
                }
                Console.WriteLine("Zakończono inicjalizację");
            }
        }

        /// <summary>
        /// Przetważa żądanie klietnta wywołując odpowiednią metodę z UserDatabaseOperations.
        /// </summary>
        /// <param name="IpAddres">Adres IP klienta</param>
        /// <param name="data">dane</param>
        public void TakeClientRequest(string IpAddres, string data) {

        }

        public static string HashPassword(string ClientHashedPassword) {
            //haslo >8 liter sprawdzane po stronie kilenta
            var byteArrayClientPassword = Encoding.UTF8.GetBytes(ClientHashedPassword);
            SHA256 sha = new SHA256Managed();
            sha.Initialize();
            var hashedPasswordBytes = sha.ComputeHash(byteArrayClientPassword);
            string result = "";
            foreach (var h in hashedPasswordBytes) {
                result += string.Format("{0:x2}", h);
            }
            sha.Clear();
            return result;
        }
    }
}
