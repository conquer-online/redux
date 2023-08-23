using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace TinyMap
{
    public class Scene
    {
        public ScenePart[] Parts { get; private set; }

        public Scene(String path)
        {
            using (BinaryReader br = new BinaryReader(new FileStream(path, FileMode.Open)))
            {
                Int32 Count = br.ReadInt32();
                this.Parts = new ScenePart[Count];
                for (Int32 i = 0; i < this.Parts.Length; i++)
                {
                    br.BaseStream.Seek(332L, SeekOrigin.Current);

                    Parts[i].Size = new Size(br.ReadInt32(), br.ReadInt32());

                    br.ReadInt32();

                    Parts[i].Start = new Point(br.ReadInt32(), br.ReadInt32());

                    br.ReadInt32();

                    Parts[i].Valid = new Boolean[Parts[i].Size.Width, Parts[i].Size.Height];

                    for (int h = 0; h < Parts[i].Size.Height; h++)
                    {
                        for (int w = 0; w < Parts[i].Size.Width; w++)
                        {
                            Int32 val = br.ReadInt32();
                            if (val == 0)
                                Parts[i].Valid[w, h] = true;
                            else
                                Parts[i].Valid[w, h] = false;
                            br.ReadInt64();
                        }
                    }
                }
                
            }
        }

    }

    public struct ScenePart
    {
        public Size Size;
        public Point Start;
        public Boolean[,] Valid;
    }
}
