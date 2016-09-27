using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedClasses {
    public class User {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SecondName { get; set; }
        public string Login { get; set; }
        /// <summary>
        /// Pasword -> ClientHash -> Server -> ServerHash
        /// </summary>
        public string PasswordHash { get; set; }
        public string Description { get; set; }
        public string ActualIP { get; set; }
        public List<int> Friends { get; set; }
        public Guid SessionID { get; set; }
    }
}
