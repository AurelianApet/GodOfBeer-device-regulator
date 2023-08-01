using device.util;
using RestSharp;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;

namespace device.restful
{
    public class ApiClient : GenericSingleton<ApiClient>
    {
        public class ApiInfo
        {
            public string api { get; set; }
            public object resObject { get; set; }
        }

        public class ApiResponse
        {
            public int? suc { get; set; }
            public string msg { get; set; }
            public Dictionary<string, object> dataMap { get; set; }
        }

        Dictionary<Type, ApiInfo> matchDic = null;

        JsonSerializer json = new JsonSerializer();

        public class SetRegulatorValveStatusApi
        {
            public int? regulator_no
            {
                get; set;
            }
            public int? ch_value
            {
                get; set;
            }
            public int? valve
            {
                get; set;
            }
            public int? is_valve_error
            {
                get; set;
            }
            public int? error_status
            {
                get; set;
            }
        }

        public class SetRegulatorStatusResponseApi
        {
            public int? regulator_no
            {
                get; set;
            }
            public int? ch_value
            {
                get; set;
            }
            public long? pressure
            {
                get; set;
            }
            public long? temperature
            {
                get; set;
            }
        }

        public ApiClient()
        {
            matchDic = new Dictionary<Type, ApiInfo>();
            matchDic.Add(typeof(SetRegulatorStatusResponseApi), new ApiInfo() { api = "set-regulator-response", resObject = new ApiResponse() });
            matchDic.Add(typeof(SetRegulatorValveStatusApi), new ApiInfo() { api = "set-regulator-valve", resObject = new ApiResponse() });
        }

        public ApiResponse PostQuery(object postData)
        {
            ApiResponse result = null;
            try
            {
                var client = new RestClient(ConfigSetting.api_server_domain);
                var request = new RestRequest(ConfigSetting.api_prefix + matchDic[postData.GetType()].api, Method.POST);
                request.AddHeader("Content-Type", "application/json; charset=utf-8");
                request.AddJsonBody(postData);
                var response = client.Execute(request);
                result = json.Deserialize<ApiResponse>(response);
            }
            catch (Exception ex)
            {
                result = new ApiResponse();
                result.suc = 0;
                result.msg = ex.Message;
                result.dataMap = null;
            }
            return result;
        }

        public ApiResponse SetRegulatorStatusResponseFunc(int regulator_no, int ch_value, long pressure, long temperature)
        {
            SetRegulatorStatusResponseApi info = new SetRegulatorStatusResponseApi();
            info.regulator_no = regulator_no;
            info.ch_value = ch_value;
            info.pressure = pressure;
            info.temperature = temperature;
            return PostQuery(info);
        }

        public ApiResponse SetRegulatorValveStatusFunc(int regulator_no, int ch_value, int valve, int is_valve_error, int error_status)
        {
            SetRegulatorValveStatusApi info = new SetRegulatorValveStatusApi();
            info.regulator_no = regulator_no;
            info.ch_value = ch_value;
            info.valve = valve;
            info.is_valve_error = is_valve_error;
            info.error_status = error_status;
            return PostQuery(info);
        }
    }
}
