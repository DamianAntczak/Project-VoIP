using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using System.Security.Cryptography;
using SharedClasses;

namespace Server___konsola {
    public class UserDatabaseOperations : IUserDataBaseOperations {

        public static LiteDatabase dataBase;
        public static LiteCollection<User> dataBaseCollection;
        public static Guid newGuid;

        public static string RETURN_NO = "NO";
        public static string RETURN_OK = "OK";

        public UserDatabaseOperations(LiteDatabase ldb, LiteCollection<User> lc) {
            dataBase = ldb;
            dataBaseCollection = lc;
            newGuid = new Guid();
        }

        public UserInfo LookForUser(string Login) {
            var ui = FindOneUser(Login);
            if (ui != null)
                return new UserInfo { Login = ui.Login, Name = ui.Name, SecondName = ui.SecondName, Description = ui.Description };
            else
                return null;
            
        }

        public List<UserInfo> LookForUser(string Name, string SecondName) {
            var ui = dataBaseCollection.Find(n => n.Name == Name && n.SecondName == SecondName);
            List<UserInfo> userInfo = new List<UserInfo>();
            foreach (var u in ui) {
                userInfo.Add(new UserInfo { Name = u.Name, SecondName = u.SecondName, Login = u.Login, Description = u.Description });
            }
            return userInfo;
        }

        public bool TryRegisterNewUser(string Name, string SecondName, string Login, string PasswordHash, string Description) {
            Console.WriteLine("Próba zapisania nowego użytkownika do bazy");
            if (dataBase == null)
                throw new ArgumentNullException();
            var user = new User { Name = Name, SecondName = SecondName, Login = Login, PasswordHash = HashPassword(PasswordHash), Description = Description };
            try {
                dataBaseCollection.Insert(user);
                return true;
            }
            catch (LiteException e) {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Dodaje przyjaciela do listy
        /// </summary>
        /// <param name="Login1"></param>
        /// <param name="Login2"></param>
        public bool AddNewFriend(Guid SessionID, int UserID, string Login2) {
            var user = FindOneUser(SessionID, UserID);
            var friend = FindOneUser(Login2);
            if(user != null && friend != null) {
                if (user.Friends == null)
                    user.Friends = new List<int>();
                user.Friends.Add(friend.Id);
                if (friend.Friends == null)
                    friend.Friends = new List<int>();
                friend.Friends.Add(friend.Id);
                user.Friends.Sort();
                friend.Friends.Sort();
                dataBaseCollection.Update(user);
                dataBaseCollection.Update(friend);
                return true;
            }
            return false;
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
        public void ChangeUserData(Guid SessionID, int UserID, string Name, string SecondName, string Description) {
            var user = FindOneUser(SessionID, UserID);
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

        public void ChangeUserPassword(Guid SessionID, int UserID, string OldPasswordHash, string NewPasswordHash) {
            var user = FindOneUser(SessionID, UserID);
            var oldPassword = HashPassword(OldPasswordHash);
            var newPassword = HashPassword(NewPasswordHash);
            if (oldPassword != null && oldPassword == user.PasswordHash && newPassword != null)
                user.PasswordHash = newPassword;
        }

        public UserLogin TryToLoginUser(string Login, string Password) {
            try {
                var user = FindOneUser(Login);
                if (user != null) {
                    var userHashedPassword = user.PasswordHash;
                    var passwordHashToCompare = HashPassword(Password);
                    if (userHashedPassword == passwordHashToCompare) {
                        Guid sessionId = Guid.NewGuid();
                        user.SessionID = sessionId;
                        dataBaseCollection.Update(user);
                        return new UserLogin {
                            Name = user.Name,
                            SecondName = user.SecondName,
                            Description = user.Description,
                            Login = user.Login,
                            Id = user.Id,
                            SessionID = sessionId,
                            Friends = GetFriendsInfo(user.Friends)
                        };
                    }
                    else
                        return new UserLogin();
                }
                else {
                    return new UserLogin();
                }
            }
            catch (ArgumentException e) {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Message);
                return new UserLogin();
            }
        }

        private List<UserInfo> GetFriendsInfo(List<int> friendsIds) {
            List<UserInfo> userFriends = new List<UserInfo>();
            if (friendsIds != null) {
                foreach (var friend in friendsIds) {
                    userFriends.Add(FindOneUserInfo(friend));
                }
            }
            return userFriends;
        }

        public string HashPassword(string ClientHashedPassword) {
            var byteArrayClientPassword = Encoding.UTF8.GetBytes(ClientHashedPassword);
            SHA256 sha = new SHA256Managed();
            sha.Initialize();
            var hashedPasswordBytes = sha.ComputeHash(byteArrayClientPassword);
            //var hashedPasswordString = Encoding.UTF8.GetString(hashedPasswordBytes);
            string result = "";
            foreach (var h in hashedPasswordBytes) {
                result += string.Format("{0:x2}", h);
            }
            sha.Clear();
            //Console.WriteLine(hashedPasss);
            return result;
        }

        public void LogoutUser(Guid SessionID, int UserID) {
            var user = FindOneUser(SessionID, UserID);
            user.SessionID = newGuid;
            user.ActualIP = string.Empty;
            dataBaseCollection.Update(user);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="SessionID"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"/>
        public User FindOneUser(Guid SessionID, int UserID) {
            if (SessionID != null && SessionID != newGuid && UserID > 0)
                return dataBaseCollection.FindOne(x => x.Id == UserID && x.SessionID == SessionID);
            else
                throw new ArgumentException("SessionID is null or empty(00000..) or UserID < 1", "SessionID || UserID");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Login"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"/>
        public User FindOneUser(string Login) {
            if (Login != null && Login != string.Empty)
                return dataBaseCollection.FindOne(x => x.Login == Login);
            else
                throw new ArgumentException("Login is null or empty", "Login");
        }

        public User FindOneUser(int UserId) {
            if (UserId > 0)
                return dataBaseCollection.FindOne(x => x.Id == UserId);
            else
                throw new ArgumentException("UserId isn't correct.", "UserId");
        }

        public UserInfo FindOneUserInfo(int Id) {
            if (Id > 0)
                return UserInfo.Convert(dataBaseCollection.FindOne(x => x.Id == Id));
            else
                throw new ArgumentException("Login is null or empty", "Login");
        }

        public string RingTouser(Guid SessionId, int UserId, int FriendYouWantCallToID) {
            if (UserId != FriendYouWantCallToID) {
                User user;
                User friend;
                try {
                    user = FindOneUser(SessionId, UserId);
                    friend = FindOneUser(FriendYouWantCallToID);
                }
                catch (ArgumentException e) {
                    return RETURN_NO;
                }
                if (friend.ActualIP != null) {
                    return friend.ActualIP;
                }
                else {
                    return RETURN_NO;
                }
            }
            else {
                return RETURN_NO;
            }
        }
    }
}
