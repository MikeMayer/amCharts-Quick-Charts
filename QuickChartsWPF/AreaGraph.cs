#region Namespaces

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

#endregion

namespace AmCharts.Windows.QuickCharts
{
	/// <summary>
	///     Facilitates rendering of area graphs.
	/// </summary>
	public class AreaGraph : SerialGraph
	{
		private readonly Polygon _areaGraph;
		private Canvas _graphCanvas;

		/// <summary>
		///     Instantiates AreaGraph.
		/// </summary>
		public AreaGraph()
		{
			this.DefaultStyleKey = typeof (AreaGraph);
			_areaGraph = new Polygon();

			BindBrush();
		}

		private void BindBrush()
		{
			var brushBinding = new Binding("Brush")
			{
				Source = this
			};
			_areaGraph.SetBinding(Polygon.FillProperty, brushBinding);
		}

		/// <summary>
		///     Applies control template.
		/// </summary>
		public override void OnApplyTemplate()
		{
			_graphCanvas = (Canvas)TreeHelper.TemplateFindName("PART_GraphCanvas", this);
			_graphCanvas.Children.Add(_areaGraph);
		}

		/// <summary>
		///     Renders area graph.
		/// </summary>
		public override void Render()
		{
			var newPoints = GetAreaPoints();
			if (_areaGraph.Points.Count != newPoints.Count)
			{
				_areaGraph.Points = newPoints;
			}
			else
			{
				for (var i = 0 ; i < newPoints.Count ; i++)
				{
					if (!_areaGraph.Points[i].Equals(newPoints[i]))
					{
						_areaGraph.Points = newPoints;
						break;
					}
				}
			}
		}

		private PointCollection GetAreaPoints()
		{

			var points = new PointCollection();

			if (Locations == null)
			{
				return points;
			}

			CopyLocationsToPoints(points);

			if (points.Count > 0)
			{
				points.Insert(0, new Point(points[0].X, GroundLevel));
				points.Add(new Point(points[points.Count - 1].X, GroundLevel));
			}
			return points;
		}

		private void CopyLocationsToPoints(ICollection<Point> points)
		{
			// copy Points from location cause SL doesn't support PointCollection() with parameter
			foreach (var p in Locations)
				points.Add(p);
		}
	}
}