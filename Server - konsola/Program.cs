using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Net;
using LiteDB;
namespace Server___konsola {
    class Program {
        private const int listenPort = 11001;
        static ClientPool clientPool;
        static TcpListener tcpListener;
        const string odebrano = "ODEBRANO";
        static void Main(string[] args) {
            clientPool = new ClientPool();
            tcpListener = new TcpListener(IPAddress.Any, listenPort);
            tcpListener.Start();

            Listener().Wait();

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
            Console.ReadKey();
            tcpListener.Stop();
        }
        static public async Task<string> Listener() {
            Console.WriteLine("Czekam");
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
            List<User> users = new List<User>();
            users.Add(new User { Name = "N", SecondName = "SN", Login = "L", Password = "PASS", Description = "OPIS", Friends = new List<string>() { "2l" } });
            users.Add(new User { Name = "2N", SecondName = "2SN", Login = "2l", Password = "PASS", Description = "OPISSSSSSSSSS", Friends = new List<string>() { "L" } });
            users.Add(new User { Name = "3N", SecondName = "3SN", Login = "3L", Password = "Pass", Description = "COS" });
            users.Add(new User { Name = "3N", SecondName = "3SN", Login = "4L", Password = "Pass", Description = "COS" });
            using (var db = new LiteDatabase("DB.db")) {
                var col = db.GetCollection<User>("users");
                /// ustawienie unikatowej wartości. 
                col.EnsureIndex(x => x.Login, true);
                db.BeginTrans();
                foreach (var u in users) {

                    try {
                        col.Insert(u);
                    }
                    catch (LiteException le) {
                        Console.WriteLine(le.Message);
                    }
                }
                db.Commit();
            }
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
