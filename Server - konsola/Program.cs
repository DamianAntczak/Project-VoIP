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
namespace Server___konsola {
    class Program : IUserDataBaseOperations {
        private const int listenPort = 11001;
        static ClientPool clientPool;
        static TcpListener tcpListener;
        static LiteDatabase dataBase;
        static LiteCollection<User> dataBaseCollection;
        const string odebrano = "ODEBRANO";
        static void Main(string[] args) {
            clientPool = new ClientPool();
            tcpListener = new TcpListener(IPAddress.Any, listenPort);
            tcpListener.Start();
            dataBase = new LiteDatabase("DB.db");
            dataBaseCollection = dataBase.GetCollection<User>("users");
            Listener();

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
            BazaInit();
            Console.ReadKey();
            tcpListener.Stop();
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
            userList.Add(new User { Name = "N", SecondName = "SN", Login = "L", PasswordHash = "PASS", Description = "OPIS", Friends = new List<string>() { "2l" } });
            userList.Add(new User { Name = "2N", SecondName = "2SN", Login = "2l", PasswordHash = "PASS", Description = "OPISSSSSSSSSS", Friends = new List<string>() { "L" } });
            userList.Add(new User { Name = "3N", SecondName = "3SN", Login = "3L", PasswordHash = "Pass", Description = "COS" });
            userList.Add(new User { Name = "3N", SecondName = "3SN", Login = "4L", PasswordHash = "Pass", Description = "COS" });
            using (var db = new LiteDatabase("DB.db")) {
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

        public UserInfo LookForUser(string Name, string SecondName, string Login) {
            throw new NotImplementedException();
        }

        public bool TryRegisterNewUser(string Name, string SecondName, string Login, string PasswordHash, string Description) {
            Console.WriteLine("Próba zapisania nowego użytkownika do bazy");
            if (dataBase == null)
                throw new ArgumentNullException();
            var user = new User { Name = Name, SecondName = SecondName, Login = Login, PasswordHash = PasswordHash, Description = Description };
            try {
                dataBaseCollection.Insert(user);
                return true;
            }
            catch (LiteException e) {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public void AddNewFriendToList(string Login1, string Login2) {
            throw new NotImplementedException();
        }
        /// <summary>
        /// dodatkowa weryfikacja z loginem i id
        /// może dodać IP?
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="Login"></param>
        /// <param name="Name"></param>
        /// <param name="SecondName"></param>
        /// <param name="Description"></param>
        public void ChangeUserData(int UserID, string Login, string Name, string SecondName, string Description) {
            var user = dataBaseCollection.Find(x => x.Id == UserID && x.Login == Login).First();
            if (user != null) {
                if (Name != null && user.Name != Name)
                    user.Name = Name;
                if (SecondName != null && user.SecondName != SecondName)
                    user.SecondName = SecondName;
                if (Description != null && user.Description != Description)
                    user.Description = Description;
                dataBaseCollection.Update(user);
            }
        }
        public void ChangeUserPassword(int UserID, string Login, string OldPasswordHash, string NewPasswordHash) {
            var user = dataBaseCollection.FindOne(x => x.Id == UserID && x.Login == Login);
            var oldPassword = HashPassword(OldPasswordHash);
            var newPassword = HashPassword(NewPasswordHash);
            if (oldPassword != null && oldPassword == user.PasswordHash && newPassword != null)
                user.PasswordHash = newPassword;
        }

        public bool TryToLoginUser(string Login, string Password) {
            throw new NotImplementedException();
        }

        public string HashPassword(string ClientHashedPassword) {
            //haslo >8 liter
            throw new NotImplementedException();
        }
        #region Wersja z gniazdkiem
        //static public async Task ListenerSocket() {
        //    Console.WriteLine("Czekam");
        //    using (var socket = await tcpListener.AcceptSocketAsync()) {
        //        Console.WriteLine("POŁĄCZONO");
        //        NetworkStream ns = new NetworkStream(socket);
        //        int data;
        //        do {
        //            var d = ns.ReadByte();
        //            data = d;
        //            Console.WriteLine(data);
        //        } while (data != null);
        //    }
        //}
        #endregion
    }
}
