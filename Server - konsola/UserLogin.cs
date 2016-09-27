using System;

namespace Server___konsola {
   public class UserLogin {
        public string Name { get; set; }
        public string SecondName { get; set; }
        public string Login { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        public Guid SessionID { get; set; }

        public static UserLogin Convert(User user) {
            return new UserLogin {
                Name = user.Name,
                SecondName = user.SecondName,
                Login = user.Login,
                Description = user.Description,
                Id = user.Id,
                SessionID = user.SessionID
            };
        }
    }
}
