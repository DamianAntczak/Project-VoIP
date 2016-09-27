using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Net.Sockets;
using System.Net;
using LiteDB;
using Newtonsoft.Json;
using System.Security;
using System.Security.Cryptography;
using SharedClasses;


namespace Server___konsola {
    class Program {
        static bool theEnd;

        private const int listenPort = 11001;
        static ClientPool clientPool;
        static TcpListener tcpListener;
        static string dbName = "UserDB.db";
        static LiteDatabase dataBase;
        static LiteCollection<User> dataBaseCollection;
        static UserDatabaseOperations userDatabaseOperations;
        static RequestsCodes requestCode;

        //--------------
        static JsonClassResponse<UserInfo> ji;

        const string odebrano = "ODEBRANO";
        static void Main(string[] args) {
            theEnd = false;
            clientPool = new ClientPool();
            tcpListener = new TcpListener(IPAddress.Any, listenPort);
            dataBase = new LiteDatabase(dbName);
            dataBaseCollection = dataBase.GetCollection<User>("users");
            JsonClassRequest jcr = new JsonClassRequest();


            // BazaInit();
                userDatabaseOperations = new UserDatabaseOperations(dataBase, dataBaseCollection);
            tcpListener.Start();

            while (true) {
                Listener();
                var user = dataBaseCollection.FindOne(x => x.Login == "2l");
                string json = JsonConvert.SerializeObject(UserLogin.Convert(user));

                Console.WriteLine(json);
                if (theEnd == true)
                    break;
            }


            Console.ReadKey();
            tcpListener.Stop();
        }

        public static void Listener() {
            Console.WriteLine("Połączono");
            using (var tcpClient = tcpListener.AcceptTcpClient()) {
                if (tcpClient.Connected) {
                    while (true) {
                        var reader = new StreamReader(tcpClient.GetStream(), Encoding.UTF8);
                        string json = "";

                        
                        if (reader != null && (json = reader.ReadLine()) != null) {
                            JsonClassRequest jsonRequest = JsonConvert.DeserializeObject<JsonClassRequest>(json);
                            var responseString = TakeClientRequest(jsonRequest);
                            Console.WriteLine(responseString);
                            var writer = new StreamWriter(tcpClient.GetStream(), Encoding.UTF8);
                            writer.WriteLine(responseString);
                            break;
                        }
                        else
                            break;
                    }
                }
            }
            Console.WriteLine("OK");
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
        public static string TakeClientRequest(JsonClassRequest jsonRequest) {
            requestCode = (RequestsCodes)jsonRequest.RequestCode;
            switch (requestCode) {
                case RequestsCodes.REGISTER:
                    break;
                case RequestsCodes.HELLO: {
                        string login = jsonRequest.Parameters[0];
                        string password = jsonRequest.Parameters[1];
                        var userLogin = userDatabaseOperations.TryToLoginUser(login, password);
                        JsonClassResponse<UserLogin> response = new JsonClassResponse<UserLogin> {
                            RID = jsonRequest.RID,
                            RequestCode = jsonRequest.RequestCode,
                            Response = userLogin
                        };
                        return JsonConvert.SerializeObject(response);

                    }
                case RequestsCodes.ADD_FRIEND_TO_LIST:
                    break;
                case RequestsCodes.CHANE_USER_DATA:
                    break;
                case RequestsCodes.CHANGE_USER_PASSWORD:
                    break;
                case RequestsCodes.LOOK_FOR_USER_BY_NAME:
                    break;
                case RequestsCodes.LOOK_FOR_USER_BY_LOGIN:
                    break;
                case RequestsCodes.LOGOUT:
                    break;
                case RequestsCodes.WELCOME:
                    break;
                case RequestsCodes.CALL:
                    break;
                case RequestsCodes.RINGING:
                    break;
                case RequestsCodes.OK:
                    break;
                case RequestsCodes.BYE:
                    break;
                case RequestsCodes.NO:
                    break;
                default:
                    break;
            }
            return "dd";
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
