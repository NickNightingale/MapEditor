using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapEditor.ViewModels
{
	public class Tile : ViewModelBase
	{
		private int _gridX;
		public int GridX
		{
			get { return _gridX; }
			set
			{
				if (_gridX != value)
				{
					_gridX = value;
					RaisePropertyChanged();
				}
			}
		}

		private int _gridY;
		public int GridY
		{
			get { return _gridY; }
			set
			{
				if (_gridY != value)
				{
					_gridY = value;
					RaisePropertyChanged();
				}
			}
		}
	}
}
