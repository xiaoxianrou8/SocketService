using System;
using System.Linq;
using System.Net;
using SocketMsgProto;

namespace SocketService
{
    class Program
    {

        static void Main(string[] args)
        {
           
            var info =Dns.GetHostEntry("www.baidu.com");
            //var tcp =new TCPService();
            //tcp.StartService();
            //Console.Read();
            var udp=new UDPService();
            udp.StartService();
            Console.Read();
        }

    }
}
