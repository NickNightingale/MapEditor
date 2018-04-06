using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			for (int i = 0; i < ExtentY; ++i)
			{
				for (int j = 0; j < ExtentX; ++j)
				{
					var index = i + j * ExtentY;
					if (index >= Tiles.Count)
					{
						break;
					}
					Tiles[i + j * ExtentY].GridX = i;
					Tiles[i + j * ExtentY].GridY = j;
				}
			}
		}
	}
}
