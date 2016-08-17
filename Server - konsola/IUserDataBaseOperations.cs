using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server___konsola {
    interface IUserDataBaseOperations {
        UserInfo LookForUser(string Name, string SecondName, string Login);
        bool TryRegisterNewUser(string Name, string SecondName, string Login, string PasswordHash, string Description);
        void AddNewFriendToList(string Login1, string Login2);
        void ChangeUserData(string Login, string Name, string SecondName, string Description);
        void ChangeUserPassword(string Login, string OldPasswordHash, string NewPasswordHash);
        bool TryToLoginUser(string Login, string Password);
        string HashPassword(string ClientHashedPassword);

    }
}
