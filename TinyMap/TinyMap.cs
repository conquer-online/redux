using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TinyMap
{
    public class TinyMap
    {
        public UInt16 MapID { get; private set; }
        public UInt16 MaxX { get; private set; }
        public UInt16 MaxY { get; private set; }
        public TileStats[,] Tiles { get; private set; }
        public List<Portal> Portals { get; private set; }

        public TinyMap(String file, Boolean LoadHeight)
        {
            using (BinaryReader br = new BinaryReader(new FileStream(file, FileMode.Open)))
            {
                this.MapID = br.ReadUInt16();
                this.MaxX = br.ReadUInt16();
                this.MaxY = br.ReadUInt16();
                this.Tiles = new TileStats[MaxX, MaxY];
                this.Portals = new List<Portal>();
                for (UInt16 x = 0; x < MaxX; x++)
                {
                    for (UInt16 y = 0; y < MaxY; y++)
                    {
                        this.Tiles[x, y].Flag = (TileFlag)br.ReadByte();
                        if (LoadHeight)
                            this.Tiles[x, y].Height = br.ReadByte();
                    }
                }
                var portalCount = br.ReadInt32();
                for (var x = 0; x < portalCount; x++)
                    Portals.Add(new Portal(br.ReadUInt32(), br.ReadUInt16(), br.ReadUInt16()));
                br.Close();
            }
        }

        public TinyMap(DMap dmap, String file, Boolean LoadHeight)
        {
            FileStream fs = File.Create(file);
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(dmap.MapID);
                bw.Write(dmap.MaxX);
                bw.Write(dmap.MaxY);
                for (UInt16 x = 0; x < dmap.MaxX; x++)
                {
                    for (UInt16 y = 0; y < dmap.MaxY; y++)
                    {
                        TileStats tStats = dmap.Tiles[x, y];
                        bw.Write((Byte)tStats.Flag);
                        if (LoadHeight)
                            bw.Write(tStats.Height);
                    }
                }
                bw.Write(dmap.Portals.Count);
                foreach (var portal in dmap.Portals)
                {
                    bw.Write(portal.PortalID);
                    bw.Write(portal.X);
                    bw.Write(portal.Y);
                }
                bw.Close();
            }
        }
    }
}
