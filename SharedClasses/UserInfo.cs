using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedClasses {
    public class UserInfo {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SecondName { get; set; }
        public string Login { get; set; }
        public string Description { get; set; }
        public string ActualIP { get; set; }

        public static UserInfo Convert(User user) {
            return new UserInfo {
                Id = user.Id,
                Name = user.Name,
                SecondName = user.SecondName,
                Login = user.Login,
                Description = user.Description,
                ActualIP = user.ActualIP
            };
        }
    }
}
