using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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

		private string _tileFile;
		public string TileFile
		{
			get { return _tileFile; }
			set
			{
				if (_tileFile != value)
				{
					_tileFile = value;
					RaisePropertyChanged();
				}
			}
		}

		private string _previewFile;
		public string PreviewFile
		{
			get { return _previewFile; }
			set
			{
				if (_previewFile != value)
				{
					_previewFile = value;
					RaisePropertyChanged();
				}
			}
		}

		private Brush _brush;
		public Brush Brush
		{
			get { return _brush; }
			set
			{
				if (_brush != value)
				{
					_brush = value;
					RaisePropertyChanged();
				}
			}
		}

		public static Tile FromStream(StreamReader stream, string mapFolder)
		{
			int.TryParse(stream.ReadLine(), out int x);
			int.TryParse(stream.ReadLine(), out int y);
			var tileFile = stream.ReadLine();

			return new Tile
			{
				GridX = x,
				GridY = y,
				TileFile = Path.Combine(mapFolder, tileFile),
				PreviewFile = Path.Combine(mapFolder, Path.Combine(@"texture\map", tileFile) + ".roadmap.bmp"),
				Brush = Brushes.Blue
			};
		}
	}
}
