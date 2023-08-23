using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TinyMap
{
    public class DMap
    {
        public UInt16 MaxX { get; private set; }
        public UInt16 MaxY { get; private set; }
        public uint MapID { get; private set; }
        public TileStats[,] Tiles { get; private set; }
        public List<Portal> Portals { get; private set; }

        public DMap(uint id, String path, String ConquerDirectory, Boolean LoadPortals, Boolean LoadHeight)
        {
            if (id > UInt16.MaxValue) return;
            this.MapID = Convert.ToUInt32(id);
            using (BinaryReader br = new BinaryReader(new FileStream(path, FileMode.Open)))
            {
                br.BaseStream.Seek(268L, SeekOrigin.Begin);
                this.MaxX = Convert.ToUInt16(br.ReadInt32());
                this.MaxY = Convert.ToUInt16(br.ReadInt32());
                this.Tiles = new TileStats[this.MaxX, this.MaxY];

                for (UInt16 i = 0; i < this.MaxY; i++)
                {
                    for (UInt16 j = 0; j < this.MaxX; j++)
                    {
                        SetValid(j, i, !Convert.ToBoolean(br.ReadInt16()));
                        var surface = br.ReadInt16();
                        var height = br.ReadInt16();
                        if (LoadHeight)
                            Tiles[j, i].Height = (byte)height;                        
                    }
                    br.ReadInt32();
                }

                Int32 PortalCount = br.ReadInt32();
                if (LoadPortals)
                {
                    Portals = new List<Portal>();
                    for (Int32 p = 0; p < PortalCount; p++)
                    {
                        UInt16 pX = Convert.ToUInt16(br.ReadInt32());
                        UInt16 pY = Convert.ToUInt16(br.ReadInt32());
                        if (pX < MaxX && pY < MaxY)
                            this.Tiles[pX, pY].Flag |= TileFlag.Portal;
                        Portals.Add(new Portal(br.ReadUInt32(), pX, pY));
                    }
                }
                else
                {
                    br.BaseStream.Seek(PortalCount * 12L, SeekOrigin.Current);
                }

                Int32 ObjectCount = br.ReadInt32();
                for (Int32 i = 0; i < ObjectCount; i++)
                {
                    switch (br.ReadInt32())
                    {
                        case 1:
                            {
                                String sPath = Encoding.ASCII.GetString(br.ReadBytes(260));
                                sPath = sPath.Remove(sPath.IndexOf('\0')).Trim();
                                try
                                {
                                    sPath = Path.Combine(ConquerDirectory, sPath);

                                    UInt16 sX = Convert.ToUInt16(br.ReadInt32());
                                    UInt16 sY = Convert.ToUInt16(br.ReadInt32());

                                    if (File.Exists(sPath))
                                    {
                                        Scene scene = new Scene(sPath);
                                        CombineScene(scene, sX, sY);
                                    }
                                }
                                catch { }
                                break;
                            }
                        case 4:
                            br.BaseStream.Seek(416L, SeekOrigin.Current);
                            break;
                        case 10:
                            br.BaseStream.Seek(72L, SeekOrigin.Current);
                            break;
                        case 15:
                            br.BaseStream.Seek(276L, SeekOrigin.Current);
                            break;
                    }
                }
                br.Close();
            }
        }

        private void SetValid(Int32 X, Int32 Y, Boolean valid)
        {
            if (valid)
            {
                if (this.Tiles[X, Y].Flag.HasFlag(TileFlag.Invalid))
                    this.Tiles[X, Y].Flag &= ~TileFlag.Invalid;
            }
            else
            {
                if (!this.Tiles[X, Y].Flag.HasFlag(TileFlag.Invalid))
                    this.Tiles[X, Y].Flag |= TileFlag.Invalid;
            }
        }

        private void CombineScene(Scene s, UInt16 locX, UInt16 locY)
        {
            foreach (ScenePart part in s.Parts)
            {
                for (int j = 0; j < part.Size.Width; j++)
                {
                    for (int k = 0; k < part.Size.Height; k++)
                    {
                        Int32 X = ((locX + part.Start.X) + j) - part.Size.Width;
                        Int32 Y = ((locY + part.Start.Y) + k) - part.Size.Height;
                        SetValid(X, Y, part.Valid[j, k]);
                    }
                }
            }
        }

    }
}
