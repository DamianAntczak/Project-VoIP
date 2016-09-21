using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server___konsola {
    interface IUserDataBaseOperations {
        /// <summary>
        /// Zwraca użytkowników, którzy mają pasujące imię i nazwisko.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="SecondName"></param>
        /// <returns></returns>
        List<UserInfo> LookForUser(string Name, string SecondName);
        /// <summary>
        /// Zwraca użytkownika po loginie.
        /// </summary>
        /// <param name="Login"></param>
        /// <returns></returns>
        UserInfo LookForUser(string Login);
        bool TryRegisterNewUser(string Name, string SecondName, string Login, string PasswordHash, string Description);
        void AddNewFriendToList(string Login1, string Login2);
        void ChangeUserData(int UserID, string Login, string Name, string SecondName, string Description);
        void ChangeUserPassword(int UserID, string Login, string OldPasswordHash, string NewPasswordHash);
        UserLogin TryToLoginUser(string Login, string Password);
        string HashPassword(string ClientHashedPassword);

    }
}
