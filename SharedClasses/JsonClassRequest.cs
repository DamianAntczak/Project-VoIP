using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedClasses {
    /// <summary>
    /// Class for json requests
    /// </summary>
    /// <example>JsonClassRequest r = new JsonClassRequest {
    /// RID = 6, 
    /// RequestCode = RequestsCodes.HELLO, 
    /// Request = new List<string>() { "Login", "Password" } };</example>
    public class JsonClassRequest {
        public int RID { get; set; }
        public int RequestCode { get; set; }
        public List<string> Parameters { get; set; }
    }
}
