using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TinyMap
{
    public class Portal
    {
        public Portal(uint portalID, ushort x, ushort y)
        {
            X = x;
            Y = y;
            PortalID = portalID;
        }
        public UInt16 X { get; private set; }
        public UInt16 Y { get; private set; }
        public UInt32 PortalID { get; private set; }
    }
}
