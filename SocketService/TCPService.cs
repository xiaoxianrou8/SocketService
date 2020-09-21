using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SocketMsgProto;

namespace SocketService
{
    public class TCPService
    {
        private Socket _listener;
        public TCPService()
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endPoint = new IPEndPoint(ConfigsHelper.IpAddress, ConfigsHelper.Port);
            _listener.Bind(endPoint);
            _listener.Listen(int.MaxValue);
        }

        public void StartService()
        {
            while (true)
            {
                Socket socket = null;
                try
                {
                    Console.WriteLine("TCP服务正常准备连接。。。。");
                    socket = _listener.Accept();
                    var remoteInfo = socket.RemoteEndPoint;
                    Thread dataThread = new Thread(DealData);
                    dataThread.Start(socket);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    socket.Close();
                    socket.Dispose();
                }
            }

        }

        unsafe private void DealData(Object obj)
        {
            Socket socket = obj as Socket;
            byte[] tempBytes = null;
            int cursor = 0;
            if (socket == null)
            {
                return;
            }
            var buffer = new byte[ConfigsHelper.BufferSize];

            while (true)
            {
                try
                {
                    var length = socket.Receive(buffer);
                    DealtMessage(buffer.Take(length).ToArray(), ref tempBytes, ref cursor);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    tempBytes = null;
                    cursor = 0;
                    socket.Close();
                    socket.Dispose();
                    Console.WriteLine("Socket断开");
                    break;
                }
            }
        }
        private void DealtMessage(byte[] buffer, ref byte[] tempBytes, ref int cursor)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return;
            }
            if (tempBytes == null)
            {
                tempBytes = new byte[ConfigsHelper.BufferSize];
            }
            //总字节长度
            var cacheLength = buffer.Length + cursor;
            //需要扩展的空间倍数
            var ext = (int)Math.Ceiling((cacheLength - tempBytes.Length) / (double)ConfigsHelper.BufferSize);
            if (ext > 0)
            {
                var extBytes = new byte[ext * ConfigsHelper.BufferSize];
                tempBytes = tempBytes.Concat(extBytes).ToArray();
            }
            buffer.CopyTo(tempBytes, cursor);
            cursor += buffer.Length;
        
            while (cursor >= BuildTcpProto.MsgHeaderLength)
            {
                var headerBytes = tempBytes.Take(BuildTcpProto.MsgHeaderLength).ToArray();
                var header = BuildTcpProto.ParseHeader(headerBytes);
                if (cursor >= header.Length)
                {
                    var msgBytes = tempBytes.Take(header.Length).ToArray();
                    int surplusCount = cursor - header.Length;
                    var surplusBytes = tempBytes.Skip(header.Length).Take(surplusCount).ToArray();
                    tempBytes = surplusBytes;
                    cursor -= header.Length;
                    var msgstr = BuildTcpProto.ParseMessage(header, msgBytes);
                    Console.WriteLine(msgstr);
                }
                else
                {
                    break;
                }
            }
        }
    }
}
