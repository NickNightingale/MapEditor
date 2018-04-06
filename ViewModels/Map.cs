using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using MapEditor.Utilities;

namespace MapEditor.ViewModels
{
	public class Map : ViewModelBase
	{
		private int _extentX;
		public int ExtentX
		{
			get { return _extentX; }
			set
			{
				if (_extentX != value)
				{
					_extentX = value;
					RaisePropertyChanged();
				}
			}
		}

		private int _extentY;
		public int ExtentY
		{
			get { return _extentY; }
			set
			{
				if (_extentY != value)
				{
					_extentY = value;
					RaisePropertyChanged();
				}
			}
		}

		private ObservableCollection2<Tile> _tiles;

		public ObservableCollection2<Tile> Tiles
		{
			get
			{
				if (_tiles == null)
				{
					Tiles = new ObservableCollection2<Tile>();
				}
				return _tiles;
			}
			set
			{
				if (_tiles != value)
				{
					if (_tiles != null)
					{
						_tiles.CollectionChanged -= Tiles_CollectionChanged;
					}
					_tiles = value;
					if (_tiles != null)
					{
						_tiles.CollectionChanged += Tiles_CollectionChanged;
					}
					RaisePropertyChanged();
				}
			}
		}

		private void Tiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			UpdateTileGridPositions();
			RaisePropertyChanged();
		}

		public void UpdateTileGridPositions()
		{
			//for (int i = 0; i < ExtentY; ++i)
			//{
			//	for (int j = 0; j < ExtentX; ++j)
			//	{
			//		var index = i + j * ExtentY;
			//		if (index >= Tiles.Count)
			//		{
			//			break;
			//		}
			//		Tiles[i + j * ExtentY].GridX = i;
			//		Tiles[i + j * ExtentY].GridY = j;
			//	}
			//}
		}

		public static Map FromFile(string fileName)
		{
			using (var file = File.OpenText(fileName))
			{
				return Map.FromStream(file, Path.GetDirectoryName(fileName));
			}
		}

		public static Map FromStream(StreamReader stream, string mapFolder)
		{
			int minX = int.MaxValue, maxX = -int.MaxValue;
			int minY = int.MaxValue, maxY = -int.MaxValue;
			var tiles = new Dictionary<string, Tile>();
			while (!stream.EndOfStream)
			{
				if (stream.ReadLine().Contains("[map]"))
				{
					var tile = Tile.FromStream(stream, mapFolder);
					if (tile.GridX < minX) minX = tile.GridX;
					if (tile.GridX > maxX) maxX = tile.GridX;
					if (tile.GridY < minY) minY = tile.GridY;
					if (tile.GridY > maxY) maxY = tile.GridY;
					tiles.Add(GetKey(tile.GridX, tile.GridY), tile);
				}
			}
			var map = new Map
			{
				ExtentX = maxX - minX + 1,
				ExtentY = maxY - minY + 1
			};
			for (int y = maxY; y >= minY; --y)
			{
				for (int x = minX; x <= maxX; ++x)
				{
					if (tiles.TryGetValue(GetKey(x, y), out Tile tile))
					{
						map.Tiles.Add(tile);
					}
					else
					{
						map.Tiles.Add(new Tile { GridX = x, GridY = y, Brush = Brushes.Gray });
					}
				}
			}
			return map;
		}

		static string GetKey(int x, int y)
		{
			return $"{x},{y}";
		}
	}


	public class TileComparer :  Singleton<TileComparer>, IComparer<Tile>
	{
		public int Compare(Tile x, Tile y)
		{
			if (x.GridY == y.GridY && x.GridX == y.GridX)
			{
				return 0;
			}
			if (x.GridY == y.GridY)
			{
				return x.GridX > y.GridX ? 1 : -1;
			}
			return x.GridY > y.GridY ? 1 : -1;
		}
	}
}
