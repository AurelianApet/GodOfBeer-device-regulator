using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Config;
using System.Net;
using device.util;

namespace device.network
{
    public class MainServer
    {
        TcpListener tcpServer = null;
        int tcp_port = -1;
        Thread waitClient;
        CommonHandler CommonHan = new CommonHandler();
        List<TClient> tclients = new List<TClient>();

        public MainServer()
        {
        }

        public void CreateServer(int port)
        {
            try
            {
                tcp_port = port;
                tcpServer = new TcpListener(IPAddress.Any, port);
                tcpServer.Start();
                waitClient = new Thread(new ThreadStart(ReceiveWait));
                waitClient.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Regulator장치 연결 실패!") + ex.ToString());
                Thread.Sleep(5000);
                CreateServer(port);
            }
            //finally
            //{
            //    tcpServer.Stop();
            //}
        }

        private void ReceiveWait()
        {
            while (true)
            {
                Console.WriteLine("Waiting for a connection...");
                TClient client = new TClient(tcpServer.AcceptTcpClient());
                Console.WriteLine("New Client Connected! ip:" + client._clientIP + ", no:" + client.regulator_no);
                tclients.Add(client);
                //Thread checkStatus = new Thread(new ThreadStart(checkClientStatus));
                //checkStatus.Start();
            }
        }

        private void checkClientStatus()
        {
            TClient tc = null;
            try
            {
                tc = tclients[tclients.Count - 1];
            }
            catch (Exception ex)
            {
                return;
            }
            while (tc != null)
            {
                try
                {
                    if (!tc.status)
                    {
                        tclients.Remove(tc);
                        break;
                    }
                    Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                    break;
                }
            }
        }

        public void Send_REQ_PING(int regulator_no)
        {
            for (int i = 0; i < tclients.Count; i++)
            {
                if (regulator_no == ConfigSetting.getRegulatorNo(tclients[i]._clientIP))
                {
                    CommonHan.REQ_PING(tclients[i].networkStream);
                }
            }
        }

        public void Send_REQ_SET_PRESSURE_CTRL(int regulator_no, int ch_value, long pressure, long temperature, long constant, long tolerance, int ctrl_state)
        {
            for (int i = 0; i < tclients.Count; i++)
            {
                if (regulator_no == ConfigSetting.getRegulatorNo(tclients[i]._clientIP))
                {
                    CommonHan.REQ_SET_PRESSURE_CTRL(tclients[i].networkStream, ch_value, pressure, temperature, constant, tolerance, ctrl_state);
                    tclients[i].networkStream.Flush();
                }
            }
        }

        public void Send_REQ_GET_PRESSURE_VALUE(int regulator_no, int ch_value)
        {
            for (int i = 0; i < tclients.Count; i++)
            {
                if (regulator_no == ConfigSetting.getRegulatorNo(tclients[i]._clientIP))
                {
                    CommonHan.REQ_GET_PRESSURE_VALUE(tclients[i].networkStream, ch_value);
                    tclients[i].networkStream.Flush();
                }
            }
        }
    }
}
