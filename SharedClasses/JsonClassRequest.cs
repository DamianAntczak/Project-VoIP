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
    /// IP = 127.0.0.1:6321, 
    /// RID = 6, 
    /// RequestCode = RequestsCodes.HELLO, 
    /// Request = new List<string>() { "Login", "Password" } };</example>
    public class JsonClassRequest {
        public int RID { get; set; }
        public string IP { get; set; }
        public int RequestCode { get; set; }
        public List<string> Request { get; set; }
        public JsonClassRequest() { }
    }
}
