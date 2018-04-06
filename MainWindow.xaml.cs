﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MapEditor.Controllers;
using MapEditor.ViewModels;
using MapEditor.Views;

namespace MapEditor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			Loaded += MainWindow_Loaded;
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			Loaded -= MainWindow_Loaded;

			var view = new MapView();
			var model = new Map();
			view.DataContext = model;
			var controller = new MapController(model, view);
			Host.Content = view;

			controller.Start();
		}
	}
}
