#region Namespaces

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

#endregion

namespace AmCharts.Windows.QuickCharts
{
	/// <summary>
	///     Represents grid in serial chart.
	/// </summary>
	public class ValueGrid : Control
	{
		private readonly List<Line> _gridLines = new List<Line>();
		private Canvas _gridCanvas;
		private List<double> _locations = new List<double>();

		/// <summary>
		///     Initializes a new instance of ValueGrid class.
		/// </summary>
		public ValueGrid()
		{
			DefaultStyleKey = typeof (ValueGrid);

			VerticalAlignment = VerticalAlignment.Stretch;
			HorizontalAlignment = HorizontalAlignment.Stretch;

			LayoutUpdated += OnValueGridLayoutUpdated;
		}

		private void OnValueGridLayoutUpdated(object sender, EventArgs e)
		{
			SetupLines();
		}

		/// <summary>
		///     Assigns control template parts.
		/// </summary>
		public override void OnApplyTemplate()
		{
			_gridCanvas = (Canvas)TreeHelper.TemplateFindName("PART_GridCanvas", this);
		}

		/// <summary>
		///     Sets locations (coordinates) of grid lines.
		/// </summary>
		/// <param name="locations">Locations (coordinates) of grid lines.</param>
		public void SetLocations(IEnumerable<double> locations)
		{
			_locations = new List<double>(locations);
			SetupLines();
		}

		private void SetupLines()
		{
			var count = Math.Min(_locations.Count, _gridLines.Count);

			SetLineLocations(count);

			if (_locations.Count == _gridLines.Count)
			{
				return;
			}

			if (_locations.Count > _gridLines.Count)
			{
				AddGridLines(count);
			}

			else if (_locations.Count < _gridLines.Count)
			{
				RemoveGridLines(count);
			}
		}

		private void SetLineLocations(int count)
		{
			for (var i = 0 ; i < count ; i++)
			{
				SetLineX(i);
				SetLineY(i);
			}
		}

		private void SetLineX(int i)
		{
			if (_gridCanvas == null)
			{
				return;
			}

			if (Math.Abs(_gridLines[i].X2 - _gridCanvas.ActualWidth) > double.Epsilon)
			{
				_gridLines[i].X2 = _gridCanvas.ActualWidth;
			}
		}

		private void SetLineY(int i)
		{
			if (Math.Abs(_gridLines[i].Y1 - _locations[i]) <= Double.Epsilon)
			{
				return;
			}

			_gridLines[i].Y1 = _locations[i];
			_gridLines[i].Y2 = _gridLines[i].Y1;
		}

		private void AddGridLines(int count)
		{
			for (var i = count ; i < _locations.Count ; i++)
			{
				var line = new Line
				{
					Stroke = Foreground,
					StrokeThickness = 1,
					X1 = 0
				};
				_gridLines.Add(line);

				SetLineX(i);
				SetLineY(i);

				_gridCanvas.Children.Add(_gridLines[i]);
			}
		}

		private void RemoveGridLines(int count)
		{
			for (var i = _gridLines.Count - 1 ; i >= count ; i--)
			{
				_gridCanvas.Children.Remove(_gridLines[i]);
				_gridLines.RemoveAt(i);
			}
		}
	}
}