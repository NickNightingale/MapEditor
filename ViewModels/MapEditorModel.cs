using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapEditor.ViewModels
{
	public class MapEditorModel : ViewModelBase
	{
		private string _mapFile;
		public string MapFile
		{
			get { return _mapFile;  }
			set
			{
				if (_mapFile != value)
				{
					_mapFile = value;
					RaisePropertyChanged();
				}
			}
		}

		private Map _map;
		public Map Map
		{
			get { return _map; }
			set
			{
				if (_map != value)
				{
					_map = value;
					RaisePropertyChanged();
				}
			}
		}
	}
}
