﻿using System;
using System.Collections.Generic;
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
	}
}
