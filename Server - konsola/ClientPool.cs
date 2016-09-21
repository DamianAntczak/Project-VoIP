using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Server___konsola {
    public class ClientPool {
        private Queue<ClientData> clientPool;
        private int queueLength = 10;
        public ClientPool() {
            clientPool = new Queue<ClientData>();
            for (int i = 0; i < queueLength; i++) {
                clientPool.Enqueue(new ClientData());
            }
        }
        public ClientData newClient(IPAddress ipAddress, int request) {
            while (true) {
                if (queueLength > 0) {
                    --queueLength;
                    var clientData = clientPool.Dequeue();
                    clientData.IpAddress = ipAddress;
                    clientData.TimeOfReqest = DateTime.Now;
                    clientData.RequestId = request;
                    return clientData;
                }
                else {
                    clientPool.Enqueue(new ClientData());
                    ++queueLength;
                }
            }
        }
       public void takeClient(ClientData clientData) {
            clientData.IpAddress = IPAddress.None;
            clientData.RequestId = 0;
            clientPool.Enqueue(clientData);
            ++queueLength;
        }
    }
}
