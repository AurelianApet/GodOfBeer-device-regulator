using device.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace device.network
{
    class ServerManager : GenericSingleton<ServerManager>
    {
        public MainServer mainServer = new MainServer();

        public event EventHandler OnOrderUpdate;
        public event EventHandler OnDeviceUpdate;

        public void UpdateOrder()
        {
            OnOrderUpdate?.Invoke(this, null);
        }

        public void UpdateDeivce()
        {
            OnDeviceUpdate.Invoke(this, null);
        }
    }
}
