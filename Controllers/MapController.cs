using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using MapEditor.ViewModels;
using MapEditor.Views;

namespace MapEditor.Controllers
{
	public class MapController
	{
		Map _model;
		MapView _view;

		public MapController(Map model, MapView view)
		{
			_model = model;
			_view = view;
		}

		public void Start()
		{
			_model.ExtentX = 3;
			_model.ExtentY = 3;

			var tiles = new List<Tile>();
			for (int i = 0; i < _model.ExtentY * _model.ExtentX; ++i)
			{
				tiles.Add(new Tile { Brush = GetBrush() });
			}
			_model.Tiles.AddRange(tiles);
		}

		Random _random = new Random();

		public Brush GetBrush()
		{
			Brush result = Brushes.Transparent;
			var brushesType = typeof(Brushes);
			var properties = brushesType.GetProperties();
			int random = _random.Next(properties.Length);
			result = (Brush)properties[random].GetValue(null, null);
			return result;
		}
	}
}
