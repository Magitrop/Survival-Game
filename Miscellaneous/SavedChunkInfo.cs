using System;
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

		public SavedTileInfo(byte _coords, byte _type, byte[] _info)
		{
			coords = _coords;
			type = _type;
			additionalInformation = _info;
		}
	}

	public sealed class SavedChunkInfo
	{
		public int chunkX, chunkY;
		public List<SavedTileInfo> changedTiles;

		public SavedChunkInfo(string chunkInfo)
		{
			changedTiles = new List<SavedTileInfo>();

			string[] chunkAndChanges = chunkInfo.Split(':');
			chunkX = int.Parse(chunkAndChanges[0].Split(',')[0]);
			chunkY = int.Parse(chunkAndChanges[0].Split(',')[1]);

			foreach (string change in chunkAndChanges[1].Split('|'))
			{
				string[] part = change.Split(',');
				List<byte> _info = new List<byte>();
				for (int i = 3; i < part.Length; i++)
					_info.Add(byte.Parse(part[i]));
				AddTile(byte.Parse(part[0]), byte.Parse(part[1]), byte.Parse(part[2]), _info.ToArray());
			}
		}

		public SavedChunkInfo(int _chunkX, int _chunkY)
		{
			changedTiles = new List<SavedTileInfo>();

			chunkX = _chunkX;
			chunkY = _chunkY;
		}

		public void AddTile(byte _x, byte _y, byte _type, byte[] _info)
		{
			SavedTileInfo curChange = new SavedTileInfo((byte)((_y << 3) + _x), _type, _info);
			// первый байт - всегда номер объекта на клетке
			if (_info.Length > 0 && _info[0] < 100)
				curChange.additionalInformation[0] = 0;
			changedTiles.Add(curChange);
		}

		public static (int x, int y) GetTile(byte coords)
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
				result += t.x + "," + t.y + "," + changedTiles[i].type;
				if (changedTiles[i].additionalInformation.Length > 0)
					result += ",";
				for (int j = 0; j < changedTiles[i].additionalInformation.Length; j++)
				{
					result += changedTiles[i].additionalInformation[j];
					if (j < changedTiles[i].additionalInformation.Length - 1)
						result += ",";
				}
					
				if (i != changedTiles.Count - 1)
					result += "|";
			}
			return result;
		}
	}
}