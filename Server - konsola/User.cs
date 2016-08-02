using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server___konsola {
    class User {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SecondName { get; set; }
        public string Login { get; set; }
        /// <summary>
        /// ZMIENIC
        /// </summary>
        public string Password { get; set; }
        public string Description { get; set; }
        public List<string> Friends { get; set; }
    }
}
