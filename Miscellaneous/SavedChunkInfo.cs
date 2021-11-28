﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Miscellaneous
{
    public sealed class SavedTileInfo
    {
        public byte coords;
        public byte type;
        public byte[] additionalInformation;
    }

    public sealed class SavedChunkInfo
    {
        public int chunkX, chunkY;
        public List<(byte coords, byte type, byte obj)> changedTiles;

        public SavedChunkInfo(string info)
        {
            changedTiles = new List<(byte, byte, byte)>();

            string[] chunkAndChanges = info.Split(':');
            chunkX = int.Parse(chunkAndChanges[0].Split(',')[0]);
            chunkY = int.Parse(chunkAndChanges[0].Split(',')[1]);

            foreach (string change in chunkAndChanges[1].Split('|'))
            {
                string[] part = change.Split(',');
                AddTile(byte.Parse(part[0]), byte.Parse(part[1]), byte.Parse(part[2]), byte.Parse(part[3]));
            }
        }

        public SavedChunkInfo(int _chunkX, int _chunkY)
        {
            changedTiles = new List<(byte, byte, byte)>();

            chunkX = _chunkX;
            chunkY = _chunkY;
        }

        public void AddTile(byte _x, byte _y, byte _type, byte _object)
        {
            (byte coords, byte type, byte obj) curChange = (0, 0, 0);
            curChange.coords = (byte)(((curChange.coords = _y) << 3) + _x);
            curChange.type = _type;
            if (_object >= 100)
                curChange.obj = _object;
            else
                curChange.obj = 0;
            changedTiles.Add(curChange);
        }

        public (int, int) GetTile(byte coords)
        {
            //                        y     x
            // x => первые 3 бита  00 |000| |000|
            // y => вторые 3 бита  00 |000| |000|
            return (coords & 7, coords >> 3);
        }

        public override string ToString()
        {
            string result = chunkX + "," + chunkY + ":";
            (int x, int y) t;
            for (int i = 0; i < changedTiles.Count; i++)
            {
                t = GetTile(changedTiles[i].coords);
                result += t.x + "," + t.y + "," + changedTiles[i].type + "," + changedTiles[i].obj;
                if (i != changedTiles.Count - 1)
                    result += "|";
            }
            return result;
        }
    }
}