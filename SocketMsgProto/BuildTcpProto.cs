using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Force.Crc32;
using SocketMsgProto;

namespace SocketService
{
    public class BuildTcpProto
    {
        public static int GetStructLength<TStruct>() where TStruct:struct
        {
            var type = typeof(TStruct);
            var totalLen = 0;
            foreach (var field in type.GetFields())
            {
                if (!field.FieldType.IsValueType)
                {
                    throw new Exception("消息头成员必须为值类型");
                }
                var fLenght = GetTypeLength(field.FieldType);
                totalLen += fLenght;    
            }
            return totalLen;
        }

        public static int MsgHeaderLength => GetStructLength<MessageHeader>();

        public static int GetTypeLength(Type type)
        {
            return Marshal.SizeOf(type);
        }
        
        public static int GetTypeLength<T>()
        {
            return Marshal.SizeOf<T>();
        }

        public static byte[] BuildSendBuffer(MessageHeader header, string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var totalLength = GetStructLength<MessageHeader>()+ messageBytes.Length;
            var bytes = new byte[totalLength];
            var cursor = 0;

            var signBytes = Util.ShortToBytes(header.StartSign);
            signBytes.CopyTo(bytes, cursor);
            cursor += signBytes.Length;

            header.Length = totalLength;
            var lengthBytes = Util.IntToBytes(header.Length);
            lengthBytes.CopyTo(bytes, cursor);
            cursor += lengthBytes.Length;

            var crc32 = Crc32Algorithm.Compute(Encoding.UTF8.GetBytes(message));
            var crc32Bytes = Util.UIntToBytes(crc32);
            crc32Bytes.CopyTo(bytes, cursor);
            cursor += crc32Bytes.Length;
            messageBytes.CopyTo(bytes, cursor);
            cursor += messageBytes.Length;
            if (cursor != totalLength)
            {
                throw new Exception("wdnmd生成字节出错");
            }
            return bytes;
        }

        public static MessageHeader ParseHeader(byte[] bytes)
        {
            if (bytes.Length!=MsgHeaderLength)
            {
                throw new Exception("参数错误");
            }
            MessageHeader recieveHeader = new MessageHeader();
            var cursor = 0;

            var shortLength = BuildTcpProto.GetTypeLength<short>();
            var intLength = BuildTcpProto.GetTypeLength<int>();
            var uintLength = BuildTcpProto.GetTypeLength<uint>();

            var signBytes = bytes.Take(shortLength).ToArray();
            recieveHeader.StartSign = Util.BytesToShort(signBytes);
            cursor += signBytes.Length;

            var lengthBytes = bytes.Skip(cursor).Take(intLength).ToArray();
            recieveHeader.Length = Util.BytesToInt(lengthBytes);
            cursor += lengthBytes.Length;

            var crc32Bytes = bytes.Skip(cursor).Take(intLength).ToArray();
            recieveHeader.Crc32 = Util.BytesToUInt(crc32Bytes);
            cursor += crc32Bytes.Length;

            return recieveHeader;
        }
        public static MessageHeader SeperatePecketBuffer(byte[] pBytes, out string message)
        {
            var msgResult = string.Empty;
            var headerLength = GetStructLength<MessageHeader>();
            var headerBytes = pBytes.Take(headerLength).ToArray();
            var messageBytes = pBytes.Skip(headerLength).Take(pBytes.Length- headerLength).ToArray();
            MessageHeader recieveHeader=new MessageHeader();
            var cursor = 0;

            var shortLength = BuildTcpProto.GetTypeLength<short>();
            var intLength = BuildTcpProto.GetTypeLength<int>();
            var uintLength= BuildTcpProto.GetTypeLength<uint>();

            var signBytes = headerBytes.Take(shortLength).ToArray();
            recieveHeader.StartSign = Util.BytesToShort(signBytes) ;
            cursor += signBytes.Length;

            var lengthBytes = headerBytes.Skip(cursor).Take(intLength).ToArray();
            recieveHeader.Length = Util.BytesToInt(lengthBytes);
            cursor += lengthBytes.Length;

            var crc32Bytes = headerBytes.Skip(cursor).Take(intLength).ToArray();
            recieveHeader.Crc32 = Util.BytesToUInt(crc32Bytes);
            cursor += crc32Bytes.Length;

            var rmsg = Encoding.UTF8.GetString(messageBytes);
            var rCrc32 = Crc32Algorithm.Compute(messageBytes, 0, messageBytes.Length);
            if (rCrc32==recieveHeader.Crc32)
            {
                message = rmsg;
            }
            else
            {
                throw new Exception("wdnmd");
            }

            return recieveHeader;
        }

        public static string ParseMessage(MessageHeader header,byte[] bytes)
        {
            if (header.StartSign!=0xff)
            {
                throw new Exception("未发现数据边界");
            }

            var messageBytes = bytes.Skip(MsgHeaderLength).Take(bytes.Length - MsgHeaderLength).ToArray();
            if (header.Crc32 != Crc32Algorithm.Compute(messageBytes))
            {
                throw new Exception("接收数据出错");
            }
            return Encoding.UTF8.GetString(messageBytes);
        }
    }
}
