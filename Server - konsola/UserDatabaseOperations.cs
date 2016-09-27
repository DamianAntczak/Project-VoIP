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

        public UserDatabaseOperations(LiteDatabase ldb, LiteCollection<User> lc) {
            dataBase = ldb;
            dataBaseCollection = lc;
            newGuid = new Guid();
        }

        public UserInfo LookForUser(string Login) {
            var ui = FindOneUser(Login);
            UserInfo userInfo = new UserInfo { Login = ui.Login, Name = ui.Name, SecondName = ui.SecondName, Description = ui.Description, ActualIP = ui.ActualIP };
            return userInfo;
        }

        public List<UserInfo> LookForUser(string Name, string SecondName) {
            var ui = dataBaseCollection.Find(n => n.Name == Name && n.SecondName == SecondName);
            List<UserInfo> userInfo = new List<UserInfo>();
            foreach (var u in ui) {
                userInfo.Add(new UserInfo { Name = u.Name, SecondName = u.SecondName, Login = u.Login, Description = u.Description, ActualIP = u.ActualIP });
            }
            return userInfo;
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

        /// <summary>
        /// Dodaje przyjaciela do listy
        /// </summary>
        /// <param name="Login1"></param>
        /// <param name="Login2"></param>
        public void AddNewFriend(Guid SessionID, int UserID, string Login2) {
            var user = FindOneUser(SessionID, UserID);
            var friend = FindOneUser(Login2);
            if(user.Friends.IndexOf(friend.Id) > 0) {
                user.Friends.Add(friend.Id);
                friend.Friends.Add(friend.Id);
                user.Friends.Sort();
                friend.Friends.Sort();
            }
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
            var user = FindOneUser(Login);
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
                    SessionID = sessionId
                };
            }
            else
                return new UserLogin();
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
                return dataBaseCollection.FindOne(x =>x.Login == Login);
            else
                throw new ArgumentException("Login is null or empty", "Login");
        }
    }
}
