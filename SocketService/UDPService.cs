using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketService
{
    public class UDPService
    {
        private Socket _udpSocket;

        public UDPService()
        {
            _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var endPoint = new IPEndPoint(ConfigsHelper.IpAddress, ConfigsHelper.Port);
            _udpSocket.Bind(endPoint);
        }

        public void StartService()
        {
            try
            {
                Console.WriteLine("UDP服务初始化成功。。。");
                while (true)
                {
                    var buffer = new byte[65536];
                    var length = _udpSocket.Receive(buffer);
                    var result = Encoding.UTF8.GetString(buffer, 0, length);
                    Console.WriteLine(result);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}
