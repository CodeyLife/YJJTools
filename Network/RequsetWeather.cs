using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace YJJTool
{
    public class RequsetWeather : MonoBehaviour
    {
        private string mWeatherKey = "&key=ec7c60c6a1ddbaea8a43441ecdd1f8ae";
        private string mWeatherType = "&extensions=all";
        public UnityEvent<WeatherData> WeatherEvent = new();
        public UnityEvent<Live> LiveEvent = new UnityEvent<Live>();

        private void Awake()
        {
            RequestWeather();  
        }

        [Button]
        void RequestWeather()
        {
            var filePath = Path.Combine(Application.streamingAssetsPath, "天气api配置.json");
            JObject json = null;
            if (File.Exists(filePath))
            {
                json = JObject.Parse(File.ReadAllText(filePath));
            }
            else
            {
                json = new JObject();
                json.Add("url", "https://restapi.amap.com/v3/weather/weatherInfo");
                json.Add("key", "ec7c60c6a1ddbaea8a43441ecdd1f8ae");
                json.Add("city", "500000");
                File.WriteAllTextAsync(filePath, json.ToString());
            }
            string url = $"{json["url"]}?key={json["key"]}&city={json["city"]}";

           
            StartCoroutine(RequestDataBase.Get(url, (data) =>
            {
                var live = JObject.Parse(data)["lives"].ToArray()[0].ToObject<Live>();
                //Debug.Log(JsonConvert.SerializeObject(live).ToString());
                LiveEvent?.Invoke(live);
            }));
            ////天气预报
            StartCoroutine(RequestDataBase.Get<WeatherData>(url + "&extensions=all", (data) =>
             {
                 Debug.Log(JsonConvert.SerializeObject(data));
                 WeatherEvent?.Invoke(data);
             }));
        }
        #region 数据
        public class Casts
        {
            /// <summary>
            /// 
            /// </summary>
            public DateTime date { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string week { get; set; }
            /// <summary>
            /// 阴
            /// </summary>
            public string dayweather { get; set; }
            /// <summary>
            /// 小雨
            /// </summary>
            public string nightweather { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string daytemp { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string nighttemp { get; set; }
            /// <summary>
            /// 东北
            /// </summary>
            public string daywind { get; set; }
            /// <summary>
            /// 东北
            /// </summary>
            public string nightwind { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string daypower { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string nightpower { get; set; }
        }

        public class Forecasts
        {
            /// <summary>
            /// 九龙坡区
            /// </summary>
            public string city { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string adcode { get; set; }
            /// <summary>
            /// 重庆
            /// </summary>
            public string province { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public DateTime reporttime { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<Casts> casts { get; set; }
        }

        public class WeatherData
        {
            /// <summary>
            /// 
            /// </summary>
            public string status { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string count { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string info { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string infocode { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<Forecasts> forecasts { get; set; }
        }


        public class Live
        {
            /// <summary>
            /// 重庆
            /// </summary>
            public string province;
            /// <summary>
            /// 重庆市
            /// </summary>
            public string city;
            /// <summary>
            /// 500000
            /// </summary>
            public string adcode;
            /// <summary>
            /// 阴
            /// </summary>
            public string weather;
            /// <summary>
            /// 10
            /// </summary>
            public string temperature;
            /// <summary>
            /// 东
            /// </summary>
            public string winddirection;
            /// <summary>
            /// ≤3
            /// </summary>
            public string windpower;
            /// <summary>
            /// 61
            /// </summary>
            public string humidity;
            /// <summary>
            /// 2025-01-10 13:01:14
            /// </summary>
            public string reporttime;
            /// <summary>
            /// 10.0
            /// </summary>
            public string temperature_float;
            /// <summary>
            /// 61.0
            /// </summary>
            public string humidity_float;
        }
        #endregion
    }
}
