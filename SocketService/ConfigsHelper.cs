using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace SocketService
{
    public static class ConfigsHelper
    {
        private static IPAddress _ipAddress=IPAddress.Any;

        public static IPAddress IpAddress => _ipAddress;

        private static int _port;

        public static int Port => _port;

        private static int _bufferSize;
        public static int BufferSize => _bufferSize;

        static ConfigsHelper()
        {
            var settingsStr = ReaderSettings();
            InitSettings(settingsStr);
        }

        private static string ReaderSettings()
        {
            var cfgPathStr = "Settings.json";
            var cfgPath = Path.Combine(System.Environment.CurrentDirectory, cfgPathStr);
            if (!File.Exists(cfgPath))
            {
                throw new FileLoadException("配置文件不存在！");
            }
            var text = File.ReadAllText(cfgPath);
            return text;
        }

        private static void InitSettings(string input)
        {
            var configs = JsonConvert.DeserializeObject<dynamic>(input);
            var ip = configs["ip"].ToString();
            _port = (int) configs["port"];
            _ipAddress = IPAddress.Parse(ip);

            _bufferSize = (int) configs["bufferSize"];
        }
    }
}
