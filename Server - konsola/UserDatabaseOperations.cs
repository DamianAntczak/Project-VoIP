using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using System.Security.Cryptography;

namespace Server___konsola {
    public class UserDatabaseOperations :IUserDataBaseOperations {

        public static LiteDatabase dataBase;
        public static LiteCollection<User> dataBaseCollection;

        public UserDatabaseOperations(LiteDatabase ldb, LiteCollection<User> lc) {
            dataBase = ldb;
            dataBaseCollection = lc;
        }

        public UserInfo LookForUser(string Login) {
            var ui = dataBaseCollection.FindOne(n => n.Login == Login);
            UserInfo userInfo = new UserInfo { Login = ui.Login, Name = ui.Name, SecondName = ui.SecondName, Description = ui.Description, ActualIP = ui.ActualIP };
            return userInfo;
        }

        public List<UserInfo> LookForUser(string Name, string SecondName) {
           var ui = dataBaseCollection.Find(n => n.Name == Name && n.SecondName == SecondName);
            List<UserInfo> userInfo = new List<UserInfo>();
            foreach (var u in ui){
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

        public UserLogin TryToLoginUser(string Login, string Password) {
            var user = dataBaseCollection.FindOne(x => x.Login == Login);
            var userHashedPassword = user.PasswordHash;
            var passwordHashToCompare = HashPassword(Password);
            if (userHashedPassword == passwordHashToCompare)
                return new UserLogin { Name = user.Name, SecondName = user.SecondName, Description = user.Description,
                    Login = user.Login, Id = user.Id, SessionID = Guid.NewGuid() } ;
            else
                return new UserLogin();
        }

        public string HashPassword(string ClientHashedPassword) {
            //haslo >8 liter sprawdzane po stronie kilenta
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
        public byte[] HashPasswordReturnBytes(string ClientHashedPassword) {
            var byteArrayClientPassword = Encoding.UTF8.GetBytes(ClientHashedPassword);
            var sha = new SHA256Managed();
            sha.Initialize();
            var hashedPasswordBytes = sha.ComputeHash(byteArrayClientPassword);
            sha.Clear();
            return hashedPasswordBytes;
        }
    }
}
