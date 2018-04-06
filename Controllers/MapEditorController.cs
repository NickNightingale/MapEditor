using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MapEditor.ViewModels;
using MapEditor.Views;
using Microsoft.Win32;

namespace MapEditor.Controllers
{
	public class MapEditorController
	{
		public MapEditorModel Model { get; private set; }
		public MapEditorView View { get; private set; }

		public MapEditorController(MapEditorModel model, MapEditorView view)
		{
			Model = model;
			View = view;
		}

		public void Start()
		{
			View.DataContext = Model;

			View.CommandBindings.Add(new CommandBinding(Commands.Commands.Browse, Browse_Executed, Browse_CanExecute));
			View.CommandBindings.Add(new CommandBinding(MediaCommands.Play, Play_Executed, Play_CanExecute));
		}

		private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.Handled = true;
			e.CanExecute = !string.IsNullOrEmpty(Model.MapFile);
		}

		private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			e.Handled = true;
			Model.Map = Map.FromFile(Model.MapFile);
		}

		private void Browse_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.Handled = true;
			e.CanExecute = true;
		}

		private void Browse_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			e.Handled = true;
			var dialog = new OpenFileDialog
			{
				Filter = "Map config files (*.cfg)|*.cfg|All files (*.*)|*.*"
			};
			if (!string.IsNullOrEmpty(Model.MapFile))
			{
				dialog.FileName = Model.MapFile;
			}
			if (dialog.ShowDialog(Application.Current.MainWindow) == true)
			{
				Model.MapFile = dialog.FileName;
			}
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
