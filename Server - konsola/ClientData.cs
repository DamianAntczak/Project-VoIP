using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Server___konsola {
    public class ClientData {
        public IPAddress ipAddress { get; set; }
        public DateTime timeOfRequest { get; set; }
        public int request { get; set; }

        public ClientData(IPAddress ipAddress, DateTime timeOfRequest, int request) {
            this.ipAddress = ipAddress;
            this.timeOfRequest = timeOfRequest;
            this.request = request;
        }
        public ClientData() {
            ipAddress = IPAddress.None;
            timeOfRequest = DateTime.Now;
            request = 0;
        }
    }
}
