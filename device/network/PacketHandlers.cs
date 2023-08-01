using device.util;
using device.restful;
using System;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;

namespace device.network
{
    #region CommonHandler
    public class CommonHandler
    {
        TokenManager tm = TokenManager.Instance;
        
        public void REQ_PING(NetworkStream stream)
        {
            try
            {
                //uint64 TIMESTAMP 본 패킷의 전송 시간
                //int date = NetUtils.ToInt32(requestInfo.Body, 0);
                //int time = NetUtils.ToInt32(requestInfo.Body, 4);
                DateTime now = DateTime.Now;
                Int32 length = PacketInfo.HeaderSize + 8;
                Int32 opcode = (int)Opcode.RES_PING;
                byte[] packet = new byte[length];
                Array.Copy(NetUtils.GetBytes(length), 0, packet, 0, 4);
                Array.Copy(NetUtils.GetBytes(opcode), 0, packet, 4, 4);
                Array.Copy(NetUtils.GetBytes((long)0), 0, packet, 8, 8);
                Array.Copy(NetUtils.GetBytes((long)0), 0, packet, 16, 8);
                int pos = 0;
                Array.Copy(NetUtils.GetBytes(NetUtils.ConvertDateTimeToNetDate(now)), 0, packet, PacketInfo.HeaderSize + pos, 4); pos += 4;
                Array.Copy(NetUtils.GetBytes(NetUtils.ConvertDateTimeToNetTime(now)), 0, packet, PacketInfo.HeaderSize + pos, 4); pos += 4;
                stream.Write(packet, 0, packet.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex.ToString());
            }
        }

        public void RES_PING(PacketInfo requestInfo)
        {
            try
            {
                // uint64 TIMESTAMP REQ_PING 패킷의 TIMESTAMP 값
                int date = NetUtils.ToInt32(requestInfo.Body, 0);
                int time = NetUtils.ToInt32(requestInfo.Body, 4);
                Console.WriteLine("[RES_PING] date : " + date + ", time : " + time);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex.ToString());
            }
        }

        public void REQ_SET_PRESSURE_CTRL(NetworkStream stream, int ch_value, long pressure, long temperature, long constant, long tolerance, int ctrl_state)
        {
            try
            {
                Int32 length = PacketInfo.HeaderSize + 37;
                Int32 opcode = (int)Opcode.REQ_SET_PRESSURE_CTRL;
                byte[] packet = new byte[length];
                Array.Copy(NetUtils.GetBytes(length), 0, packet, 0, 4);
                Array.Copy(NetUtils.GetBytes(opcode), 0, packet, 4, 4);
                Array.Copy(NetUtils.GetBytes((long)0), 0, packet, 8, 8);
                Array.Copy(NetUtils.GetBytes((long)0), 0, packet, 16, 8);
                Array.Copy(NetUtils.GetBytes(ch_value), 0, packet, 24, 4);
                Array.Copy(NetUtils.GetBytes(pressure), 0, packet, 28, 8);
                Array.Copy(NetUtils.GetBytes(temperature), 0, packet, 36, 8);
                Array.Copy(NetUtils.GetBytes(constant), 0, packet, 44, 8);
                Array.Copy(NetUtils.GetBytes(tolerance), 0, packet, 52, 8);
                packet[60] = (byte)ctrl_state;
                stream.Write(packet, 0, packet.Length);
                Console.WriteLine("[REQ_SET_PRESSURE_CTRL] : ch_value : " + ch_value + ", Pressure : " + pressure + ", Temperature : " + temperature + ", Constant : " + constant + ", Tolerance : " + tolerance + ", Ctrl_state : " + ctrl_state);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex.ToString());
            }
        }

        public void RES_SET_PRESSURE_CTRL(PacketInfo requestInfo)
        {
        }

        public void REQ_SET_VALVE_STATUS(NetworkStream stream, PacketInfo requestInfo, int regulator_no)
        {
            try
            {
                int ch_value = NetUtils.ToInt32(requestInfo.Body, 0);
                int valve = NetUtils.ToInt32(requestInfo.Body, 4);
                byte is_valve_error = requestInfo.Body[8];
                byte error_status = requestInfo.Body[9];
                Console.WriteLine("[REQ_SET_VALVE_STATUS] : ch_value : " + ch_value + ", Valve : " + valve + ", Is_valve_error : " + is_valve_error + ", error_status : " + error_status);
                ApiClient.Instance.SetRegulatorValveStatusFunc(regulator_no, ch_value, valve, is_valve_error, error_status);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex.ToString());
            }
        }

        public void RES_GET_PRESSURE_VALUE(PacketInfo requestInfo, int regulator_no)
        {
            try
            {
                int ch_value = NetUtils.ToInt32(requestInfo.Body, 0);
                long pressure = NetUtils.ToInt64(requestInfo.Body, 4);
                long temperature = NetUtils.ToInt64(requestInfo.Body, 12);
                Console.WriteLine("[RES_GET_PRESSURE_VALUE] : ch_value : " + ch_value + ", Pressure : " + pressure + ", Temperature : " + temperature);
                ApiClient.Instance.SetRegulatorStatusResponseFunc(regulator_no, ch_value, pressure / 100, temperature / 100);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex.ToString());
            }
        }

        public void REQ_GET_PRESSURE_VALUE(NetworkStream stream, int ch_value)
        {
            try
            {
                Int32 length = PacketInfo.HeaderSize + 12;
                Int32 opcode = (int)Opcode.REQ_GET_PRESSURE_VALUE;
                byte[] packet = new byte[length];
                Array.Copy(NetUtils.GetBytes(length), 0, packet, 0, 4);
                Array.Copy(NetUtils.GetBytes(opcode), 0, packet, 4, 4);
                Array.Copy(NetUtils.GetBytes((long)0), 0, packet, 8, 8);
                Array.Copy(NetUtils.GetBytes((long)0), 0, packet, 16, 8);
                Array.Copy(NetUtils.GetBytes(ch_value), 0, packet, 24, 4);
                stream.Write(packet, 0, packet.Length);
                Console.WriteLine("[REQ_GET_PRESSURE_VALUE]");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex.ToString());
            }
        }

        public void ERROR_MESSAGE(PacketInfo requestInfo)
        {
            // uint32 OPCODE 에러가 발생한 요청의 OPCODE
            // uint32 ERROR_CODE 에러 코드
            // char[256] ERROR_MESSAGE 에러 메시지

            UInt32 opcode = NetUtils.ToUInt32(requestInfo.Body, 0);
            UInt32 error_code = NetUtils.ToUInt32(requestInfo.Body, 4);

            byte[] error_message = new byte[256];
            Array.Copy(requestInfo.Body, 8, error_message, 0, 256);
            string str_error_message = NetUtils.ConvertByteArrayToStringASCII(error_message);

            Console.WriteLine("[ERROR_MESSAGE] " + opcode.ToString() + " : " + error_code.ToString());

            //session.Close();//disconnect
        }
    }
    #endregion
}
