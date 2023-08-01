using System;
using device.network;
using Quobject.SocketIoClientDotNet.Client;
using device.util;
using Newtonsoft.Json.Linq;
using SimpleJSON;
using device.restful;
using System.Threading;
using System.Runtime.InteropServices;

namespace device
{
    class Program
    {
        public const int tcpPort = 23000;

        public static MainServer mainServer = ServerManager.Instance.mainServer;

        public static bool is_socket_open = false;

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static void Main(string[] args)
        {
            ConfigSetting.api_prefix = @"/m-api/device/";
            try
            {
                Console.WriteLine("Regulator Exe");
                if (args.Length > 0)
                {
                    string title = "Regulator Exe";
                    Console.Title = title;
                    IntPtr hWnd = FindWindow(null, title);
                    if (hWnd != IntPtr.Zero)
                    {
                        ShowWindow(hWnd, 2); // minimize the winodw  
                    }
                    ConfigSetting.server_address = args[0];
                    Console.WriteLine("server_address :" + ConfigSetting.server_address);
                    ConfigSetting.api_server_domain = @"http://" + ConfigSetting.server_address + ":3006";
                    ConfigSetting.socketServerUrl = @"http://" + ConfigSetting.server_address + ":3006";
                    ConfigSetting.devices = new DeivceInfo[(args.Length - 1) / 2];
                    for (int i = 1; i < args.Length; i++)
                    {
                        Console.WriteLine("ip : " + args[i]);
                        ConfigSetting.devices[(i - 1) / 2].ip = args[i];
                        Console.WriteLine("no : " + args[i + 1]);
                        ConfigSetting.devices[(i - 1) / 2].serial_number = int.Parse(args[i + 1]);
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex.Message);
            }

            try
            {
                Socket socket = IO.Socket(ConfigSetting.socketServerUrl);

                socket.On(Socket.EVENT_CONNECT, () =>
                {
                    try
                    {
                        if (is_socket_open)
                        {
                            return;
                        }
                        Console.WriteLine("Socket Connected!");
                        var UserInfo = new JObject();
                        socket.Emit("regulatorSetInfo", UserInfo);
                        //mainServer.Send_REQ_PING();
                        is_socket_open = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception : " + ex);
                    }
                });

                socket.On(Socket.EVENT_CONNECT_ERROR, (data) =>
                {
                    try
                    {
                        Console.WriteLine("Socket Connect failed : " + data.ToString());
                        is_socket_open = false;
                        //socket.Close();
                        //socket = IO.Socket(ConfigSetting.socketServerUrl);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception : " + ex);
                    }
                });

                socket.On(Socket.EVENT_DISCONNECT, (data) =>
                {
                    try
                    {
                        Console.WriteLine("Socket Disconnect : " + data.ToString());
                        is_socket_open = false;
                        //socket.Close();
                        //socket = IO.Socket(ConfigSetting.socketServerUrl);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception : " + ex);
                    }
                });

                mainServer.CreateServer(tcpPort);

                socket.On("regulatorInfo", (data) =>
                {
                    try
                    {
                        Console.WriteLine("regulatorInfo : " + data.ToString());
                        JSONNode jsonNode = SimpleJSON.JSON.Parse(data.ToString());
                        int regulator_no = jsonNode["regulator_no"].AsInt;
                        int ch_value = jsonNode["ch_value"].AsInt;
                        int pressure = jsonNode["pressure"].AsInt;
                        int temperature = jsonNode["temperature"].AsInt;
                        int constant = jsonNode["constant"].AsInt;
                        int tolerance = jsonNode["tolerance"].AsInt;
                        int ctrl_state = jsonNode["ctrl_state"].AsInt;
                        if (ctrl_state == 1)
                        {
                            Console.WriteLine("start 이벤트 발생");
                        }
                        else
                        {
                            Console.WriteLine("stop 이벤트 발생");
                        }
                        mainServer.Send_REQ_SET_PRESSURE_CTRL(regulator_no, ch_value, pressure, temperature, constant, tolerance, ctrl_state);
                        //mainServer.Send_REQ_PING(regulator_no);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception : " + ex);
                    }
                });

                socket.On("sendRegulatorRefresh", (data) =>
                {
                    try
                    {
                        Console.WriteLine("sendRegulatorRefresh : " + data.ToString());
                        JSONNode jsonNode = SimpleJSON.JSON.Parse(data.ToString());
                        int regulator_no = jsonNode["regulator_no"].AsInt;
                        string ch_values = jsonNode["ch_values"];
                        string[] ch_val = ch_values.Split(',');
                        for (int i = 0; i < ch_val.Length; i++)
                        {
                            int ch_value = int.Parse(ch_val[i]);
                            mainServer.Send_REQ_GET_PRESSURE_VALUE(regulator_no, ch_value);
                            //mainServer.Send_REQ_PING(regulator_no);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception : " + ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex.Message);
            }

            Console.ReadLine();
        }
    }
}
