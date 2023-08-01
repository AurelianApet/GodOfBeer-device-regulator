using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace device.util
{
    public struct DeivceInfo
    {
        public string ip;
        public int serial_number;//기기번호
    }

    public struct ClientInfo
    {
        public string clientIp;
        public NetworkStream networkStream;
    }

    [Serializable]
    public class ConfigSetting
    {
        public static string api_server_domain { get; set; }
        public static string api_prefix { get; set; }
        public static string socketServerUrl { get; set; }
        public static string server_address { get; set; }
        public static DeivceInfo[] devices { get; set; }
        public static int getRegulatorNo(string ip)
        {
            int regulatorNo = -1;
            for (int i = 0; i < devices.Length; i++)
            {
                if (devices[i].ip == ip)
                {
                    regulatorNo = devices[i].serial_number;
                    break;
                }
            }
            return regulatorNo;
        }
    }
}
