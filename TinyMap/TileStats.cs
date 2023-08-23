using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyMap
{
    public struct TileStats
    {
        public TileFlag Flag;
        public Byte Height;
    }

    [Flags]
    public enum TileFlag : byte
    {
        Invalid = 0x01,
        Portal  = 0x02,
        Item    = 0x04,
        Monster = 0x08,
        Npc     = 0x10,
    }
}
