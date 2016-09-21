using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Server___konsola {
    /// <summary>
    /// Obsługa zapytań. Zawiera adres klienta, czas zapytania kod zapytania, oraz Data. Format JSON.
    /// </summary>
    public class ClientData {
        public IPAddress IpAddress { get; set; }
        public DateTime TimeOfReqest { get; set; }
        public int RequestId { get; set; }
        public string Data { get; set; } 

        public ClientData(IPAddress ipAddress, DateTime timeOfRequest, int request) {
            IpAddress = ipAddress;
            TimeOfReqest = timeOfRequest;
            RequestId = request;
        }
        public ClientData() {
            IpAddress = IPAddress.None;
            TimeOfReqest = DateTime.Now;
            RequestId = 0;
        }
    }
}
