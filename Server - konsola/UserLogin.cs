using System;

namespace Server___konsola {
   public class UserLogin {
        public string Name { get; set; }
        public string SecondName { get; set; }
        public string Login { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        public Guid SessionID { get; set; }
    }
}
