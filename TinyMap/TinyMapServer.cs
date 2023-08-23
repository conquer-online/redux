using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Drawing;

using SevenZip;

namespace TinyMap
{
    public class TinyMapServer
    {
        /// <summary>
        /// Directory where you installed Conquer Online 2.0.
        /// Must include folders map and ini
        /// </summary>
        public String ConquerDirectory = @"C:\Program Files (x86)\Conquer 2.0";

        /// <summary>
        /// If set to true, will load Tiny Maps on a seperate thread.
        /// DEFAULT: false
        /// </summary>
        public Boolean Threading = false;

        /// <summary>
        /// If set to true, will write what TinyMap is doing to the console.
        /// DEFAULT: false
        /// </summary>
        public Boolean ShowOutput = false;

        /// <summary>
        /// If set to true, will write errors to the console.
        /// DEFAULT: true
        /// </summary>
        public Boolean ShowErrors = true;

        /// <summary>
        /// If set to true, will add portal data to Tiny Maps.
        /// DEFAULT: false
        /// </summary>
        public Boolean LoadPortals = false;

        /// <summary>
        /// If set to true, will add height data to Tiny Maps.
        /// DEFAULT: false
        /// </summary>
        public Boolean LoadHeight = false;

        /// <summary>
        /// If set to true, will extract the DMaps that are zipped
        /// This is used for newer versions of conquer.
        /// DEFAULT: true
        /// </summary>
        public Boolean ExtractDMaps = true;

        /// <summary>
        /// If set to true, will only load the TinyMap when a player enters the map.
        /// DEFAULT: false
        /// </summary>
        public Boolean LoadOnEnter = false;

        /// <summary>
        /// Dictionary where TinyMap data is stored. Key = MapID
        /// </summary>
        public Dictionary<uint, TinyMap> MapData { get; private set; }

        /// <summary>
        /// Returns true if the TinyMaps have been loaded.
        /// </summary>
        public Boolean Loaded { get; private set; }

        private String SevenZipDll = "7z.dll";
        private String DMapDirectory = @"map\map";
        private String TinyMapDirectory = @"map\tinymap";
        private String TinyMapPNGDirectory = @"map\tinymap\png";
        private String DMapExtractDirectory = @"map\tinymap\tmp";

        private Thread LoadThread;

        private Dictionary<uint, String> GameMapDat = new Dictionary<uint, String>();

        private void _LoadEnteredMap(Object o)
        {
            uint MapID = (uint)o;

            String str;
            if (GameMapDat.TryGetValue(MapID, out str))
            {
                String name = str.Substring(str.LastIndexOf('/') + 1) + ".TinyMap";
                String path = Path.Combine(TinyMapDirectory, name);

                if (File.Exists(path))
                {
                    if (ShowOutput)
                        Console.WriteLine("Loading \"{0}\" ({1})", name, MapID);
                    TinyMap tmap = new TinyMap(path, LoadHeight);
                    if (!MapData.ContainsKey(tmap.MapID))
                        MapData.Add(tmap.MapID, tmap);
                    else
                        MapData[tmap.MapID] = tmap;
                }
            }
        }

        public void LoadEnteredMap(uint MapID)
        {
            if (LoadOnEnter)
            {
                if (!MapData.ContainsKey(MapID))
                {
                    //new Thread(new ParameterizedThreadStart(_LoadEnteredMap)).Start(MapID);
                    String str;
                    if (GameMapDat.TryGetValue(MapID, out str))
                    {
                        String name = str.Substring(str.LastIndexOf('/') + 1) + ".TinyMap";
                        String path = Path.Combine(TinyMapDirectory, name);

                        if (File.Exists(path))
                        {
                            if (ShowOutput)
                                Console.WriteLine("Loading \"{0}\" ({1})", name, MapID);
                            TinyMap tmap = new TinyMap(path, LoadHeight);
                            if (!MapData.ContainsKey(tmap.MapID))
                                MapData.Add(tmap.MapID, tmap);
                            else
                                MapData[tmap.MapID] = tmap;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if the coordinates are valid.
        /// </summary>
        /// <param name="MapID">Player's map</param>
        /// <param name="X">Player's X Coordinate</param>
        /// <param name="Y">Player's Y Coordinate</param>
        /// <returns>True if the coordinates are valid</returns>
        public bool Valid(uint MapID, UInt16 X, UInt16 Y)
        {
            if (Loaded)
            {
                LoadEnteredMap(MapID);
                TinyMap tmap = null;
                lock (MapData)
                    MapData.TryGetValue(MapID, out tmap);
                if (tmap != null)
                {
                    if (X < tmap.MaxX && Y < tmap.MaxY)
                    {
                        return !(tmap.Tiles[X, Y].Flag.HasFlag(TileFlag.Invalid));
                    }
                }
                return false;
            }
            else return true;
        }

        /// <summary>
        /// Checks if moving from one coordinate to the other is valid, includes height checks.
        /// </summary>
        /// <param name="MapID">Player's map</param>
        /// <param name="oldX">Player's original X Coordinate</param>
        /// <param name="oldY">Player's original Y Coordinate</param>
        /// <param name="newX">Player's new X Coordinate</param>
        /// <param name="newY">Player's new Y Coordinate</param>
        /// <returns>True if the coordinates are valid</returns>
        public bool Valid(uint MapID, UInt16 oldX, UInt16 oldY, UInt16 newX, UInt16 newY)
        {
            if (Loaded)
            {
                LoadEnteredMap(MapID);
                if (Valid(MapID, newX, newY))
                {
                    float m = (float)(newY - oldY) / (float)(newX - oldX);
                    float c = (float)oldY - m * (float)oldX;

                    int offset = 50;

                    if (m > -1 && m < 1)
                    {
                        for (int i = Math.Min(oldX, newX); i <= Math.Max(oldX, newX); i++)
                        {
                            int x = i;
                            int y = (int)Math.Round(m * x + c);
                            if (y < 0) y = oldY;

                            Byte newHeight = (Byte)(MapData[MapID].Tiles[x, y].Height + (offset - 1));
                            Byte oldHeight = (Byte)(MapData[MapID].Tiles[oldX, oldY].Height + offset);

                            if (newHeight > oldHeight)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        for (int i = Math.Min(oldY, newY); i <= Math.Max(oldY, newY); i++)
                        {
                            int y = i;
                            int x = (int)Math.Round((y - c) / m);
                            if (x < 0) x = oldX;

                            Byte newHeight = (Byte)(MapData[MapID].Tiles[x, y].Height + (offset - 1));
                            Byte oldHeight = (Byte)(MapData[MapID].Tiles[oldX, oldY].Height + offset);

                            if (newHeight > oldHeight)
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }
                return false;
            }
            else return true;
        }

        public void AddFlag(uint map, UInt16 x, UInt16 y, TileFlag flag)
        {
            TinyMap tMap = GetTMap(map);
            if (tMap != null)
                if (x < tMap.MaxX && y < tMap.MaxY)
                {
                    tMap.Tiles[x, y].Flag |= flag;
                }

        }

        public void RemoveFlag(uint map, UInt16 x, UInt16 y, TileFlag flag)
        {
            TinyMap tMap = GetTMap(map);
            if (tMap != null)
                if (x < tMap.MaxX && y < tMap.MaxY)
                {
                    tMap.Tiles[x, y].Flag &= ~flag;
                }
        }

        public bool HasFlag(uint map, UInt16 x, UInt16 y, TileFlag flag)
        {
            TinyMap tMap = GetTMap(map);
            if (tMap != null)
                if (x < tMap.MaxX && y < tMap.MaxY)
                    return tMap.Tiles[x, y].Flag.HasFlag(flag);                
            return false;
        }

        public TinyMap GetTMap(uint map)
        {
            if (this.MapData != null)
            {
                TinyMap tMap;
                if (this.MapData.TryGetValue(map, out tMap))
                {
                    return tMap;
                }
            }
            return null;
        }

        /// <summary>
        /// Converts all the TinyMaps to PNG files.
        /// </summary>
        public void ConvertToPNG()
        {
            if (ShowOutput)
                Console.WriteLine("Converting TinyMaps to PNG...");
            foreach (TinyMap tmap in MapData.Values)
            {
                ConvertToPNG(tmap);
            }
        }

        /// <summary>
        /// Converts the specified TinyMap to a PNG file.
        /// COLOR CODE:
        ///  Black = Invalid
        ///  White = Valid
        ///  Teal = Portal
        /// </summary>
        /// <param name="tmap"></param>
        public void ConvertToPNG(TinyMap tmap)
        {
            try
            {
                if (!Directory.Exists(TinyMapPNGDirectory))
                    Directory.CreateDirectory(TinyMapPNGDirectory);
                String name = Path.Combine(TinyMapPNGDirectory, tmap.MapID + ".png");
                if (!File.Exists(name))
                {
                    Bitmap png = new Bitmap(tmap.MaxX, tmap.MaxY);
                    for (UInt16 y = 0; y < tmap.MaxY; y++)
                    {
                        for (UInt16 x = 0; x < tmap.MaxX; x++)
                        {
                            Color c = Color.White;
                            if (tmap.Tiles[x, y].Flag.HasFlag(TileFlag.Invalid))
                                c = Color.Black;

                            if (tmap.Tiles[x, y].Flag.HasFlag(TileFlag.Portal))
                                c = Color.Teal;

                            png.SetPixel(x, y, c);
                        }
                    }
                    png.Save(name, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            catch { }
        }

        /// <summary>
        /// Begins loading TinyMaps, converting DMaps if required.
        /// </summary>
        public void Load()
        {
            if (!Threading)
            {
                _Load();
            }
            else
            {
                LoadThread = new Thread(new ThreadStart(_Load));
                LoadThread.IsBackground = true;
                LoadThread.Start();
            }
        }

        private void _Load()
        {
            if (!Loaded)
            {
                DMapDirectory = Path.Combine(this.ConquerDirectory, DMapDirectory);
                TinyMapDirectory = Path.Combine(this.ConquerDirectory, TinyMapDirectory);
                TinyMapPNGDirectory = Path.Combine(this.ConquerDirectory, TinyMapPNGDirectory);
                DMapExtractDirectory = Path.Combine(this.ConquerDirectory, DMapExtractDirectory);

                PopulateGameMapDat();
                if (!Directory.Exists(TinyMapDirectory))
                {
                    if (ExtractDMaps)
                        Extract();
                    ConvertDMaps(ExtractDMaps ? DMapExtractDirectory : DMapDirectory);
                }
                MapData = new Dictionary<uint, TinyMap>();
                if (!LoadOnEnter)
                    LoadTinyMaps();
                GC.Collect();

                Loaded = true;
            }
        }
        
        private void LoadTinyMaps()
        {
            if (ShowOutput)
                Console.WriteLine("Loading TinyMaps...");
            foreach (String map in Directory.GetFiles(TinyMapDirectory, "*.TinyMap"))
            {
                TinyMap tmap = new TinyMap(map, LoadHeight);
                if (!MapData.ContainsKey(tmap.MapID))
                    MapData.Add(tmap.MapID, tmap);
                else
                    MapData[tmap.MapID] = tmap;
            }
        }

        private void ConvertDMaps(String DMapLocation)
        {
            if (ShowOutput)
                Console.WriteLine("Converting DMaps to TinyMaps...");
            if (!Directory.Exists(TinyMapDirectory))
                Directory.CreateDirectory(TinyMapDirectory);
            foreach (KeyValuePair<uint, String> kvp in this.GameMapDat)
            {
                String dmapName = Path.Combine(DMapLocation,
                    kvp.Value.Substring(kvp.Value.LastIndexOf('/') + 1)) + ".DMap";
                String tmapName = Path.Combine(TinyMapDirectory,
                    kvp.Key + ".TinyMap");

                if (File.Exists(dmapName))
                {
                    DMap dmap = new DMap(kvp.Key, dmapName, ConquerDirectory, LoadPortals, LoadHeight);
                    TinyMap tmap = new TinyMap(dmap, tmapName, LoadHeight);
                }
                else
                    Console.WriteLine("Dmap {0} Doesn't exist", dmapName);
            }
            if (ExtractDMaps)
                Directory.Delete(DMapExtractDirectory, true);
        }

        private void PopulateGameMapDat()
        {
            if (ShowOutput)
                Console.WriteLine("Populating GameMapDat");
            using (BinaryReader br = new BinaryReader(
                new FileStream(Path.Combine(this.ConquerDirectory, @"ini\GameMap.dat"), FileMode.Open)))
            {
                Int32 MapCount = br.ReadInt32();
                for (int i = 0; i < MapCount; i++)
                {
                    uint mapId = br.ReadUInt32();
                    String path = Encoding.ASCII.GetString(br.ReadBytes(br.ReadInt32()));
                    path = path.Substring(0, path.LastIndexOf('.'));
                    br.BaseStream.Seek(4L, SeekOrigin.Current);

                    if (!this.GameMapDat.ContainsKey(mapId))
                        this.GameMapDat.Add(mapId, path);
                    else
                        this.GameMapDat[mapId] = path;
                }
                br.Close();
            }
        }

        private void Extract()
        {
            if (ShowOutput)
                Console.WriteLine("Extracting Zipped DMaps...");
            if (File.Exists(SevenZipDll))
            {
                Directory.CreateDirectory(TinyMapDirectory);
                Directory.CreateDirectory(DMapExtractDirectory);
                SevenZipExtractor.SetLibraryPath(SevenZipDll);
                foreach (KeyValuePair<uint, String> kvp in this.GameMapDat)
                {
                    String path = Path.Combine(ConquerDirectory, kvp.Value + ".7z");
                    if (File.Exists(path))
                    {
                        SevenZipExtractor extractor = new SevenZipExtractor(path);
                        extractor.ExtractArchive(DMapExtractDirectory);
                        extractor.Dispose();
                    }
                }
            }
            else
            {
                if (ShowErrors)
                {
                    Console.WriteLine("{0} not found." + SevenZipDll);
                    Console.ReadLine();
                }
                Environment.Exit(7);
            }
        }
    }
}
