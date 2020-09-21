
using System;

namespace SocketMsgProto
{
    public class Util
    {
        public static byte[] IntToBytes(int number)
        {
            return BitConverter.GetBytes(number);
        }

        public static int BytesToInt(byte[] bytes)
        {
            return BitConverter.ToInt32(bytes,0);
        }
        public static byte[] UIntToBytes(uint number)
        {
            return BitConverter.GetBytes(number);
        }

        public static uint BytesToUInt(byte[] bytes)
        {
            return BitConverter.ToUInt32(bytes,0);
        }
        public static byte[] ShortToBytes(short number)
        {
            return BitConverter.GetBytes(number);
        }

        public static short BytesToShort(byte[] bytes)
        {
            return BitConverter.ToInt16(bytes,0);
        }
    }
}
