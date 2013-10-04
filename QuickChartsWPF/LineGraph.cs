#region Namespaces

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;

#endregion

namespace AmCharts.Windows.QuickCharts
{
	/// <summary>
	///     Facilitates rendering of line graphs.
	/// </summary>
	public class LineGraph : SerialGraph
	{
		/// <summary>
		///     Identifies <see cref="StrokeThickness" /> dependency property.
		/// </summary>
		public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
			"StrokeThickness",
			typeof (double),
			typeof (LineGraph),
			new PropertyMetadata(2.0)
			);

		private Canvas _graphCanvas;
		private readonly Polyline _lineGraph;

		/// <summary>
		///     Instantiates LineGraph.
		/// </summary>
		public LineGraph()
		{
			DefaultStyleKey = typeof (LineGraph);
			_lineGraph = new Polyline();

			BindBrush();
			BindStrokeThickness();
		}

		/// <summary>
		///     Gets or sets stroke thickness for a line graph line.
		///     This is a dependency property.
		///     The default is 2.
		/// </summary>
		public double StrokeThickness { get { return (double)GetValue(StrokeThicknessProperty); } set { SetValue(StrokeThicknessProperty, value); } }

		private void BindBrush()
		{
			var brushBinding = new Binding("Brush")
			{
				Source = this
			};
			_lineGraph.SetBinding(Shape.StrokeProperty, brushBinding);
		}

		private void BindStrokeThickness()
		{
			var thicknessBinding = new Binding("StrokeThickness")
			{
				Source = this
			};
			_lineGraph.SetBinding(Shape.StrokeThicknessProperty, thicknessBinding);
		}

		/// <summary>
		///     Applies control template.
		/// </summary>
		public override void OnApplyTemplate()
		{
			_graphCanvas = (Canvas)TreeHelper.TemplateFindName("PART_GraphCanvas", this);
			_graphCanvas.Children.Add(_lineGraph);
		}

		/// <summary>
		///     Renders line graph.
		/// </summary>
		public override void Render()
		{
			_lineGraph.Points = Locations;
		}
	}
}