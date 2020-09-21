using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMsgProto
{
    public struct MessageHeader
    {
        public short StartSign;
        public int Length;
        public uint Crc32;
    }

}
