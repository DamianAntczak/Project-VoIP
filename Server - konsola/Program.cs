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
        private const int ClientPort = 11112;
        static ClientPool clientPool;
        static TcpListener tcpListener;
        static string dbName = "UserDB.db";
        static LiteDatabase dataBase;
        static LiteCollection<User> dataBaseCollection;
        static UserDatabaseOperations userDatabaseOperations;
        static RequestsCodes requestCode;

        //--------------

        const string odebrano = "ODEBRANO";
        static void Main(string[] args) {
            theEnd = false;
            clientPool = new ClientPool();
            tcpListener = new TcpListener(IPAddress.Any, listenPort);
            dataBase = new LiteDatabase(dbName);
            dataBaseCollection = dataBase.GetCollection<User>("users");
            JsonClassRequest jcr = new JsonClassRequest();


            //BazaInit();
            userDatabaseOperations = new UserDatabaseOperations(dataBase, dataBaseCollection);
            tcpListener.Start();

            while (true) {
                Listener();
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
                        var stream = tcpClient.GetStream();
                        var reader = new StreamReader(tcpClient.GetStream(), Encoding.UTF8);
                        var writer = new StreamWriter(tcpClient.GetStream(), Encoding.UTF8);
                        writer.AutoFlush = true;
                        string json = "";


                        if (reader != null && (json = reader.ReadLine()) != null) {
                            JsonClassRequest jsonRequest = JsonConvert.DeserializeObject<JsonClassRequest>(json);
                            string responseString = "";
                            if (jsonRequest.RequestCode == (int)RequestsCodes.HELLO) {
                                IPEndPoint ipep = (IPEndPoint)tcpClient.Client.RemoteEndPoint;
                                IPAddress ipa = ipep.Address;
                                responseString = TakeClientRequest(jsonRequest,ipa);
                            }
                            else
                                responseString = TakeClientRequest(jsonRequest);
                            Console.WriteLine(responseString);
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

        public static int Ring(string hostname, int port, string request) {
            using (var tcpClient = new TcpClient()) {
                tcpClient.Connect(hostname, port);
                var writer = new StreamWriter(tcpClient.GetStream(), Encoding.UTF8);
                var reader = new StreamReader(tcpClient.GetStream(), Encoding.UTF8);
                writer.AutoFlush = true;
                writer.WriteLine(request);
                var response = reader.ReadLine();
                var responseJSON = JsonConvert.DeserializeObject<JsonClassResponse<string>>(response);
                if (responseJSON.RequestCode == (int)RequestsCodes.OK)
                    return (int)RequestsCodes.OK;
                else
                    return (int)RequestsCodes.NO;
            }
        }


        static public void BazaInit() {
            // BAZA DANYCH
            Console.WriteLine("Inicjalizacja bazy");
            List<User> userList = new List<User>();
            userList.Add(new User { Name = "N", SecondName = "SN", Login = "L", PasswordHash = HashPassword("PASS"), Description = "OPIS", ActualIP = string.Empty, SessionID = new Guid() });
            userList.Add(new User { Name = "2N", SecondName = "2SN", Login = "2l", PasswordHash = HashPassword("PASS"), Description = "OPISSSSSSSSSS", Friends = new List<int>() { 1 }, ActualIP = string.Empty, SessionID = new Guid() });
            userList.Add(new User { Name = "3N", SecondName = "3SN", Login = "3L", PasswordHash = HashPassword("PASS"), Description = "COS", ActualIP = string.Empty, SessionID = new Guid() });
            userList.Add(new User { Name = "3N", SecondName = "3SN", Login = "4L", PasswordHash = HashPassword("PASS"), Description = "COS", ActualIP = string.Empty, SessionID = new Guid() });
            userList.Add(new User {
                Name = "Damian",
                SecondName = "Damian",
                Login = "Damian",
                PasswordHash = "955992b0608a67000c4825ac7ca7f047bdccb70a69024061374499fed58fb534",
                Description = "COS",
                ActualIP = string.Empty,
                SessionID = new Guid(),
                Friends = new List<int>() { 6 }
            });
            userList.Add(new User {
                Name = "Szymon",
                SecondName = "Szymon",
                Login = "Szymon",
                PasswordHash = "955992b0608a67000c4825ac7ca7f047bdccb70a69024061374499fed58fb534",
                Description = "COS",
                ActualIP = string.Empty,
                SessionID = new Guid(),
                Friends = new List<int>() { 5 }
            });

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
            var userInfoNO_Response = JsonConvert.SerializeObject(new JsonClassResponse<UserInfo>() {
                RID = jsonRequest.RID,
                RequestCode = (int)RequestsCodes.NO,
                Response = null
            });
            requestCode = (RequestsCodes)jsonRequest.RequestCode;
            switch (requestCode) {

                case RequestsCodes.REGISTER: {
                        string login = jsonRequest.Parameters[0];
                        string firstName = jsonRequest.Parameters[1];
                        string secondName = jsonRequest.Parameters[2];
                        string hashPassword = jsonRequest.Parameters[3];
                        if (userDatabaseOperations.TryRegisterNewUser(firstName, secondName, login, hashPassword, ""))
                            return JsonConvert.SerializeObject(new JsonClassResponse<string> {
                                RID = jsonRequest.RID,
                                RequestCode = (int)RequestsCodes.OK,
                                Response = UserDatabaseOperations.RETURN_OK
                            });
                        else {
                            return JsonConvert.SerializeObject(new JsonClassResponse<string> {
                                RID = jsonRequest.RID,
                                RequestCode = (int)RequestsCodes.NO,
                                Response = UserDatabaseOperations.RETURN_NO
                            });
                        }
                    }
                case RequestsCodes.HELLO: {
                        string login = jsonRequest.Parameters[0];
                        string password = jsonRequest.Parameters[1];
                        var userLogin = userDatabaseOperations.TryToLoginUser(login, password);
                        JsonClassResponse<UserLogin> response = new JsonClassResponse<UserLogin> {
                            RID = jsonRequest.RID,
                            RequestCode = (int)RequestsCodes.WELCOME,
                            Response = userLogin,
                        };
                        return JsonConvert.SerializeObject(response);

                    }
                case RequestsCodes.ADD_FRIEND_TO_LIST: {

                    }
                    break;
                case RequestsCodes.CHANE_USER_DATA:
                    break;
                case RequestsCodes.CHANGE_USER_PASSWORD:
                    break;
                case RequestsCodes.LOOK_FOR_USER_BY_NAME:
                    break;
                case RequestsCodes.LOOK_FOR_USER_BY_LOGIN: {
                        string login = jsonRequest.Parameters[0];
                        var user = userDatabaseOperations.LookForUser(login);
                        int responseCode = 0;
                        if (user != null)
                            responseCode = (int)RequestsCodes.OK;
                        else
                            responseCode = (int)RequestsCodes.NO;

                        JsonClassResponse<UserInfo> response = new JsonClassResponse<UserInfo> {
                            RID = jsonRequest.RID,
                            RequestCode = responseCode,
                            Response = user,
                        };
                    }
                    break;
                case RequestsCodes.LOGOUT: {
                        Guid sessionId = new Guid();
                        Guid.TryParse(jsonRequest.Parameters[0], out sessionId);
                        int userId = 0;
                        int.TryParse(jsonRequest.Parameters[1], out userId);
                        int friendId = 0;
                        int.TryParse(jsonRequest.Parameters[2], out friendId);
                        userDatabaseOperations.LogoutUser(sessionId, userId);
                    }
                    break;
                case RequestsCodes.WELCOME:
                    break;
                case RequestsCodes.CALL: {

                        Guid sessionId = new Guid();
                        Guid.TryParse(jsonRequest.Parameters[0], out sessionId);
                        int userId = 0;
                        int.TryParse(jsonRequest.Parameters[1], out userId);
                        int friendId = 0;
                        int.TryParse(jsonRequest.Parameters[2], out friendId);

                        UserInfo friend = userDatabaseOperations.CallToUser(sessionId, userId, friendId);
                        if (friend != null) {
                            if (friend.ActualIP != UserDatabaseOperations.RETURN_NO) {
                                var user = userDatabaseOperations.FindOneUser(sessionId, userId);
                                JsonClassResponse<UserInfo> jsonResponse = new JsonClassResponse<UserInfo> {
                                    RID = jsonRequest.RID,
                                    RequestCode = (int)RequestsCodes.RINGING,
                                    Response = UserInfo.Convert(user)
                                };
                                var friendResponse = Ring(friend.ActualIP, ClientPort, JsonConvert.SerializeObject(jsonResponse));
                                if (friendResponse == (int)RequestsCodes.OK)
                                    return JsonConvert.SerializeObject(new JsonClassResponse<UserInfo>() {
                                        RID = jsonRequest.RID,
                                        RequestCode = (int)RequestsCodes.OK,
                                        Response = friend
                                    });
                                else
                                    return userInfoNO_Response;

                            }
                            else
                                return userInfoNO_Response;

                            //JsonClassResponse<UserInfo> jsonFriendResponse = JsonConvert.DeserializeObject<JsonClassResponse<UserInfo>>(friendResponse);
                            //if(jsonFriendResponse.RequestCode == (int)RequestsCodes.OK) {
                            //    return friendResponse;
                            //}
                            //else {
                            //    return 
                            //}
                        }
                        else {
                            JsonClassResponse<UserInfo> jsonNOResponse = new JsonClassResponse<UserInfo> {
                                RID = jsonRequest.RID,
                                RequestCode = (int)RequestsCodes.RINGING,
                                Response = null
                            };
                            return JsonConvert.SerializeObject(jsonNOResponse);
                        }
                    }
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

        public static string TakeClientRequest(JsonClassRequest jsonRequest,IPAddress ipaddress) {
            string login = jsonRequest.Parameters[0];
            string password = jsonRequest.Parameters[1];
            var userLogin = userDatabaseOperations.TryToLoginUser(login, password,ipaddress.ToString());
            JsonClassResponse<UserLogin> response = new JsonClassResponse<UserLogin> {
                RID = jsonRequest.RID,
                RequestCode = (int)RequestsCodes.WELCOME,
                Response = userLogin,
            };
            return JsonConvert.SerializeObject(response);
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
