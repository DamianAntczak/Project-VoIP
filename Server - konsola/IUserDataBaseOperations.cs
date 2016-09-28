using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClasses;

namespace Server___konsola {
    /// <summary>
    /// Interface of database operations.
    /// </summary>
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
        void AddNewFriend(Guid SessionID, int UserID, string Login2);
        void ChangeUserData(Guid SessionID, int UserID, string Name, string SecondName, string Description);
        void ChangeUserPassword(Guid SessionID, int UserID, string OldPasswordHash, string NewPasswordHash);
        UserLogin TryToLoginUser(string Login, string Password);
        void LogoutUser(Guid SessionID, int UserID);
        string HashPassword(string ClientHashedPassword);
        User FindOneUser(Guid SessionID, int UserID);
        User FindOneUser(string Login);
        string RingTouser(Guid SessionID, int UserID, int FriendYouWantCallToID);

    }
}
