#region Namespaces

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#endregion

namespace AmCharts.Windows.QuickCharts
{
	/// <summary>
	///     Base class for graphs in serial chart.
	/// </summary>
	public abstract class SerialGraph : Control, ILegendItem
	{
		/// <summary>
		///     Identifies <see cref="ValueMemberPath" /> dependency property.
		/// </summary>
		public static readonly DependencyProperty ValueMemberPathProperty = DependencyProperty.Register(
			"ValueMemberPath",
			typeof (string),
			typeof (SerialGraph),
			new PropertyMetadata(null, OnValueMemberPathPropertyChanged)
			);

		/// <summary>
		///     Identifies <see cref="Title" /> dependency property.
		/// </summary>
		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
			"Title",
			typeof (string),
			typeof (SerialGraph),
			new PropertyMetadata(null)
			);

		/// <summary>
		///     Identifies <see cref="Brush" /> dependency property.
		/// </summary>
		public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
			"Brush",
			typeof (Brush),
			typeof (SerialGraph),
			new PropertyMetadata(null)
			);

		/// <summary>
		///     Gets or sets path to the member in the DataSource holding values for this graph.
		///     This is a dependency property.
		/// </summary>
		public string ValueMemberPath { get { return (string)GetValue(ValueMemberPathProperty); } set { SetValue(ValueMemberPathProperty, value); } }

		/// <summary>
		///     Gets locations (coordinates) of data points for the graph.
		/// </summary>
		protected PointCollection Locations { get; private set; }

		/// <summary>
		///     Gets Y-coordinate of 0 or a value closest to 0.
		/// </summary>
		protected double GroundLevel { get; private set; }

		/// <summary>
		///     Gets single x-axis step size (the distance between 2 neighbor data points).
		/// </summary>
		protected double XStep
		{
			get
			{
				if (Locations.Count > 1)
				{
					return Locations[1].X - Locations[0].X;
				}

				if (Locations.Count == 1)
				{
					return Locations[0].X * 2;
				}

				return 0;
			}
		}

		/// <summary>
		///     Gets or sets the title of the graph.
		///     This is a dependency property.
		/// </summary>
		public string Title { get { return (string)GetValue(TitleProperty); } set { SetValue(TitleProperty, value); } }

		/// <summary>
		///     Gets or sets brush for the graph.
		///     This is a dependency property.
		/// </summary>
		public Brush Brush { get { return (Brush)GetValue(BrushProperty); } set { SetValue(BrushProperty, value); } }

		/// <summary>
		///     Event is raised when ValueMemberPath changes.
		/// </summary>
		public event EventHandler<DataPathEventArgs> ValueMemberPathChanged;

		private static void OnValueMemberPathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var graph = d as SerialGraph;
			if (graph != null && graph.ValueMemberPathChanged != null)
			{
				graph.ValueMemberPathChanged(graph, new DataPathEventArgs(e.NewValue as string));
			}
		}

		/// <summary>
		///     Sets point coordinates and ground level.
		/// </summary>
		/// <param name="locations">Locations (coordinates) of data points for the graph.</param>
		/// <param name="groundLevel">Y-coordinate of 0 value or value closest to 0.</param>
		public void SetPointLocations(PointCollection locations, double groundLevel)
		{
			Locations = locations;
			GroundLevel = groundLevel;
		}

		/// <summary>
		///     When implemented in inheriting classes, renders the graphs visual.
		/// </summary>
		public abstract void Render();
	}
}