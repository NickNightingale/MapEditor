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
		private ObservableCollection<Tile> _tiles;

		public ObservableCollection<Tile> Tiles
		{
			get
			{
				if (_tiles == null)
				{
					Tiles = new ObservableCollection<Tile>();
				}
				return _tiles;
			}
			set
			{
				if (_tiles != value)
				{
					_tiles = value;
					RaisePropertyChanged();
				}
			}
		}
	}
}
