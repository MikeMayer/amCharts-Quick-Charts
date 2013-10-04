#region Namespaces

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

#endregion

namespace AmCharts.Windows.QuickCharts
{
	/// <summary>
	///     Displays serial charts (line, column, etc.).
	/// </summary>
	public class SerialChart : Control
	{
		/// <summary>
		///     Instantiates SerialChart.
		/// </summary>
		public SerialChart()
		{
			DefaultStyleKey = typeof (SerialChart);

			_graphs.CollectionChanged += OnGraphsCollectionChanged;

			LayoutUpdated += OnLayoutUpdated;

			Padding = new Thickness(10);
		}

		private void OnGraphsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				foreach (SerialGraph graph in e.OldItems)
				{
					graph.ValueMemberPathChanged -= OnGraphValueMemberPathChanged;
					RemoveGraphFromCanvas(graph);
					RemoveIndicator(graph);
				}
			}

			if (e.NewItems != null)
			{
				foreach (SerialGraph graph in e.NewItems)
				{
					graph.ValueMemberPathChanged += OnGraphValueMemberPathChanged;
					if (graph.Brush == null && PresetBrushes.Count > 0)
					{
						graph.Brush = PresetBrushes[_graphs.IndexOf(graph) % PresetBrushes.Count];
					}
					AddGraphToCanvas(graph);
					AddIndicator(graph);
				}
			}
		}

		private void AddIndicator(SerialGraph graph)
		{
			var indicator = new Indicator();

			var fillBinding = new Binding("Brush")
			{
				Source = graph
			};
			indicator.SetBinding(Indicator.FillProperty, fillBinding);

			var strokeBinding = new Binding("PlotAreaBackground")
			{
				Source = this
			};
			indicator.SetBinding(Indicator.StrokeProperty, strokeBinding);

#if WINDOWS_PHONE
            indicator.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(OnIndicatorManipulationStarted);
#else
			indicator.MouseEnter += OnIndicatorMouseEnter;
			indicator.MouseLeave += OnIndicatorMouseLeave;
#endif

			_indicators.Add(graph, indicator);
			AddIndicatorToCanvas(indicator);
		}

#if WINDOWS_PHONE
        void OnIndicatorManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            DisplayBalloon((Indicator)sender);
            e.Handled = true;
        }
#else
		private void OnIndicatorMouseEnter(object sender, MouseEventArgs e)
		{
			DisplayBalloon((Indicator)sender);
		}

		private void OnIndicatorMouseLeave(object sender, MouseEventArgs e)
		{
			_balloon.Visibility = Visibility.Collapsed;
		}
#endif

		private void DisplayBalloon(Indicator indicator)
		{
			_balloon.Text = indicator.Text;
			_balloon.Visibility = Visibility.Visible;
			_balloon.Measure(new Size(_graphCanvasDecorator.ActualWidth, _graphCanvasDecorator.ActualHeight));
			var balloonLeft = (double)indicator.GetValue(Canvas.LeftProperty) - _balloon.DesiredSize.Width / 2 + indicator.ActualWidth / 2;
			if (balloonLeft < 0)
			{
				balloonLeft = (double)indicator.GetValue(Canvas.LeftProperty);
			}
			else if (balloonLeft + _balloon.DesiredSize.Width > _graphCanvasDecorator.ActualWidth)
			{
				balloonLeft = (double)indicator.GetValue(Canvas.LeftProperty) - _balloon.DesiredSize.Width;
			}
			var balloonTop = (double)indicator.GetValue(Canvas.TopProperty) - _balloon.DesiredSize.Height - 5;
			if (balloonTop < 0)
			{
				balloonTop = (double)indicator.GetValue(Canvas.TopProperty) + 17;
			}
			_balloon.SetValue(Canvas.LeftProperty, balloonLeft);
			_balloon.SetValue(Canvas.TopProperty, balloonTop);
		}

		private void AddIndicatorToCanvas(Indicator indicator)
		{
			if (_graphCanvas != null)
			{
				_graphCanvas.Children.Add(indicator);
			}
		}

		private void RemoveIndicator(SerialGraph graph)
		{
			if (_graphCanvas != null)
			{
				_graphCanvas.Children.Remove(_indicators[graph]);
			}
			_indicators.Remove(graph);
		}

		private void AddIndicatorsToCanvas()
		{
			foreach (var indicator in _indicators.Values)
				AddIndicatorToCanvas(indicator);
		}

		private void AddGraphToCanvas(UIElement graph)
		{
			if (_graphCanvas != null && !_graphCanvas.Children.Contains(graph))
			{
				_graphCanvas.Children.Add(graph);
			}
		}

		private void RemoveGraphFromCanvas(UIElement graph)
		{
			if (_graphCanvas != null && _graphCanvas.Children.Contains(graph))
			{
				_graphCanvas.Children.Remove(graph);
			}
		}

		private void AddGraphsToCanvas()
		{
			foreach (var graph in _graphs)
				AddGraphToCanvas(graph);
		}

		private void OnGraphValueMemberPathChanged(object sender, DataPathEventArgs e) {}

		private readonly DiscreetClearObservableCollection<SerialGraph> _graphs = new DiscreetClearObservableCollection<SerialGraph>();

		/// <summary>
		///     Gets collection of <see cref="SerialGraph" /> objects representing graphs for this chart.
		/// </summary>
		public DiscreetClearObservableCollection<SerialGraph> Graphs
		{
			get { return _graphs; }
			set { throw new NotSupportedException("Setting Graphs collection is not supported"); }
		}

		private readonly Dictionary<SerialGraph, Indicator> _indicators = new Dictionary<SerialGraph, Indicator>();

		/// <summary>
		///     Assigns template parts
		/// </summary>
		public override void OnApplyTemplate()
		{
			AssignGraphCanvas();

			AddGraphsToCanvas();
			AddIndicatorsToCanvas();

			AssignGridParts();

			AssignLegend();

			AssignBalloon();
		}

		private void AssignGraphCanvas()
		{
			_graphCanvasDecorator = (Border)TreeHelper.TemplateFindName("PART_GraphCanvasDecorator", this);
			_graphCanvasDecorator.SizeChanged += OnGraphCanvasDecoratorSizeChanged;
			_graphCanvas = (Canvas)TreeHelper.TemplateFindName("PART_GraphCanvas", this);

#if WINDOWS_PHONE
            _graphCanvas.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(OnGraphCanvasManipulationStarted);
#else
			_graphCanvas.MouseEnter += OnGraphCanvasMouseEnter;
			_graphCanvas.MouseMove += OnGraphCanvasMouseMove;
			_graphCanvas.MouseLeave += OnGraphCanvasMouseLeave;
#endif
		}

		private void AssignGridParts()
		{
			_valueAxis = (ValueAxis)TreeHelper.TemplateFindName("PART_ValueAxis", this);
			_valueGrid = (ValueGrid)TreeHelper.TemplateFindName("PART_ValueGrid", this);

			var formatBinding = new Binding("ValueFormatString")
			{
				Source = this
			};
			_valueAxis.SetBinding(ValueAxis.ValueFormatStringProperty, formatBinding);

			_categoryAxis = (CategoryAxis)TreeHelper.TemplateFindName("PART_CategoryAxis", this);
#if WINDOWS_PHONE
            _categoryAxis.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(OnGridManipulationStarted);
            _valueAxis.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(OnGridManipulationStarted);
#endif
		}

#if WINDOWS_PHONE
        void OnGridManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (_legend != null)
            {
                if (_legend.Visibility == Visibility.Collapsed)
                {
                    _legend.Visibility = Visibility.Visible;
                    _valueAxis.Visibility = Visibility.Visible;
                }
                else
                {
                    _legend.Visibility = Visibility.Collapsed;
                    _valueAxis.Visibility = Visibility.Collapsed;
                }
            }
            HideIndicators();
        }
#endif

		private void AssignLegend()
		{
			_legend = (Legend)TreeHelper.TemplateFindName("PART_Legend", this);
			_legend.LegendItemsSource = Graphs; // TODO: handle changes in Graphs
#if WINDOWS_PHONE
            _legend.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(OnGridManipulationStarted);
#endif
		}

#if !SILVERLIGHT
		private Brush _balloonBrush = Brushes.WhiteSmoke;

		/// <summary>
		///     Set the Balloon's Brush
		/// </summary>
		/// <param name="brush">Brush to use when drawing the Balloon</param>
		public void SetBalloonBrush(Brush brush)
		{
			_balloonBrush = brush;
		}
#endif

		private void AssignBalloon()
		{
			_balloon = (Balloon)TreeHelper.TemplateFindName("PART_Balloon", this);
#if !SILVERLIGHT
			_balloon.BorderBrush = _balloonBrush;
#endif
		}

#if WINDOWS_PHONE
        void OnGraphCanvasManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (_balloon != null)
            {
                _balloon.Visibility = Visibility.Collapsed;
            }
            PositionIndicators(e.ManipulationOrigin);
            SetToolTips(e.ManipulationOrigin);
        }
#else
		private void OnGraphCanvasMouseEnter(object sender, MouseEventArgs e)
		{
			var position = e.GetPosition(_graphCanvas);
			PositionIndicators(position);
		}

		private void OnGraphCanvasMouseMove(object sender, MouseEventArgs e)
		{
			var position = e.GetPosition(_graphCanvas);
			PositionIndicators(position);
			SetToolTips(position);
		}

		private void OnGraphCanvasMouseLeave(object sender, MouseEventArgs e)
		{
			HideIndicators();
		}
#endif

		private void SetToolTips(Point position)
		{
			var index = GetIndexByCoordinate(position.X);

			if (index <= -1)
			{
				return;
			}

			foreach (var t in _graphs)
			{
				var tooltipContent = t.Title + ": " + _categoryValues[index] + " | "
									+ (string.IsNullOrEmpty(ValueFormatString)
										? _values[t.ValueMemberPath][index].ToString(CultureInfo.InvariantCulture)
										: _values[t.ValueMemberPath][index].ToString(ValueFormatString));
				//ToolTipService.SetToolTip(_indicators[_graphs[i]], tooltipContent);
				//ToolTipService.SetToolTip(_graphs[i], tooltipContent);
				_indicators[t].Text = tooltipContent;
			}
		}

		private void PositionIndicators(Point position)
		{
			var index = GetIndexByCoordinate(position.X);

			if (index <= -1)
			{
				return;
			}

			foreach (var graph in _graphs)
			{
				_indicators[graph].Visibility = Visibility.Visible;
				_indicators[graph].SetPosition(_locations[graph.ValueMemberPath][index]);
			}
			//_balloon.Visibility = Visibility.Collapsed;
		}

		private void HideIndicators()
		{
			foreach (var graph in _graphs)
				_indicators[graph].Visibility = Visibility.Collapsed;
			_balloon.Visibility = Visibility.Collapsed;
		}

		private void OnGraphCanvasDecoratorSizeChanged(object sender, SizeChangedEventArgs e)
		{
			_plotAreaInnerSize = new Size(_graphCanvasDecorator.ActualWidth, _graphCanvasDecorator.ActualHeight);
			AdjustGridCount();
			SetLocations();
			HideIndicators();
		}

		private void AdjustGridCount()
		{
			AdjustValueGridCount();
			AdjustCategoryGridCount();
		}

		private void AdjustCategoryGridCount()
		{
			var oldCount = _categoryGridCount;
			_categoryGridCount = (int)(_plotAreaInnerSize.Width / (MinimumCategoryGridStep * 1.1));
			_categoryGridCount = Math.Max(1, _categoryGridCount);

			if (oldCount != _categoryGridCount)
			{
				_categoryAxis.Visibility = _categoryGridCount > 1
					? Visibility.Visible
					: Visibility.Collapsed;
			}
		}

		private void AdjustValueGridCount()
		{
			var oldCount = _desiredValueGridCount;
			_desiredValueGridCount = (int)(_plotAreaInnerSize.Height / (MinimumValueGridStep * 1.1));
			_desiredValueGridCount = Math.Max(1, _desiredValueGridCount);

			if (oldCount == _desiredValueGridCount)
			{
				return;
			}

			if (_desiredValueGridCount > 1)
			{
				_valueAxis.Visibility = Visibility.Visible;
				_valueGrid.Visibility = Visibility.Visible;
			}
			else
			{
				_valueAxis.Visibility = Visibility.Collapsed;
				_valueGrid.Visibility = Visibility.Collapsed;
			}
			SetMinMax();
		}

		private Border _graphCanvasDecorator;
		private Canvas _graphCanvas;

		private Size _plotAreaInnerSize;

		private CategoryAxis _categoryAxis;
		private ValueAxis _valueAxis;
		private ValueGrid _valueGrid;

		private Legend _legend;

		private Balloon _balloon;

		/// <summary>
		///     Identifies <see cref="DataSource" /> dependency property.
		/// </summary>
		public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register(
			"DataSource",
			typeof (IEnumerable),
			typeof (SerialChart),
			new PropertyMetadata(null, OnDataSourcePropertyChanged));

		/// <summary>
		///     Gets or sets data source for the chart.
		///     This is a dependency property.
		/// </summary>
		public IEnumerable DataSource { get { return (IEnumerable)GetValue(DataSourceProperty); } set { SetValue(DataSourceProperty, value); } }

		private static void OnDataSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var chart = d as SerialChart;
			DetachOldDataSourceCollectionChangedListener(chart, e.OldValue);
			AttachDataSourceCollectionChangedListener(chart, e.NewValue);

			if (chart != null)
			{
				chart.ProcessData();
			}
		}

		private static void DetachOldDataSourceCollectionChangedListener(SerialChart chart, object dataSource)
		{
			if (dataSource is INotifyCollectionChanged)
			{
				(dataSource as INotifyCollectionChanged).CollectionChanged -= chart.OnDataSourceCollectionChanged;
			}
		}

		private static void AttachDataSourceCollectionChangedListener(SerialChart chart, object dataSource)
		{
			if (dataSource is INotifyCollectionChanged)
			{
				(dataSource as INotifyCollectionChanged).CollectionChanged += chart.OnDataSourceCollectionChanged;
			}
		}

		private void OnDataSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// TODO: implement intelligent mechanism to handle multiple changes in one batch
			ProcessData();
		}

		/// <summary>
		///     Identifies <see cref="CategoryValueMemberPath" /> dependency property.
		/// </summary>
		public static readonly DependencyProperty CategoryValueMemberPathProperty = DependencyProperty.Register(
			"CategoryValueMemberPath",
			typeof (string),
			typeof (SerialChart),
			new PropertyMetadata(null)
			);

		/// <summary>
		///     Gets or sets path to the property holding category values in data source.
		///     This is a dependency property.
		/// </summary>
		public string CategoryValueMemberPath
		{
			get { return (string)GetValue(CategoryValueMemberPathProperty); }
			set { SetValue(CategoryValueMemberPathProperty, value); }
		}

		private readonly Dictionary<string, List<double>> _values = new Dictionary<string, List<double>>();
		private readonly Dictionary<string, PointCollection> _locations = new Dictionary<string, PointCollection>();
		private readonly List<string> _categoryValues = new List<string>();
		private readonly List<double> _categoryLocations = new List<double>();

		private double _minimumValue;
		private double _maximumValue;
		private double _adjustedMinimumValue;
		private double _adjustedMaximumValue;
		private double _groundValue; // 0 or closest to 0

		private double _valueGridStep;
		private readonly List<double> _valueGridValues = new List<double>();
		private readonly List<double> _valueGridLocations = new List<double>();

		private readonly List<string> _categoryGridValues = new List<string>();
		private readonly List<double> _categoryGridLocations = new List<double>();

		private void ProcessData()
		{
			if (DataSource != null)
			{
				var paths = GetDistinctPaths().ToArray();

				var bindingEvaluators = CreateBindingEvaluators(paths);
				ResetValues(paths);

				// add data items
				foreach (var dataItem in DataSource)
					AddSingleStepValues(paths, bindingEvaluators, dataItem);

				ProcessCategoryData();
			}
			else
			{
				_values.Clear();
			}
			InvalidateMinMax();
		}

		private void ProcessCategoryData()
		{
			_categoryValues.Clear();

			if (DataSource == null || string.IsNullOrEmpty(CategoryValueMemberPath))
			{
				return;
			}

			var evaluator = new BindingEvaluator(CategoryValueMemberPath);

			foreach (var val in from object dataItem in DataSource
				select string.IsNullOrEmpty(CategoryFormatString)
					? (evaluator.Eval(dataItem).ToString())
					: string.Format(CategoryFormatString, evaluator.Eval(dataItem)))
				_categoryValues.Add(val);
		}

		private void AddSingleStepValues(IEnumerable<string> paths, IDictionary<string, BindingEvaluator> bindingEvaluators, object dataItem)
		{
			foreach (var path in paths)
				_values[path].Add(Convert.ToDouble(bindingEvaluators[path].Eval(dataItem)));
		}

		private IEnumerable<string> GetDistinctPaths()
		{
			// get all distinct ValueMemberPath properties set on graphs
			var paths = (from g in Graphs
				select g.ValueMemberPath).Distinct();
			return paths;
		}

		private static Dictionary<string, BindingEvaluator> CreateBindingEvaluators(IEnumerable<string> paths)
		{
			return paths.ToDictionary(path => path, path => new BindingEvaluator(path));
		}

		private void ResetValues(IEnumerable<string> paths)
		{
			_values.Clear();
			foreach (var path in paths)
				_values.Add(path, new List<double>());
		}

		private void SetMinMax()
		{
			if (_values.Count <= 0)
			{
				return;
			}

			var minimumValues = (from vs in _values.Values
				where vs.Count > 0
				select vs.Min()).ToArray();
			_minimumValue = minimumValues.Any() ? minimumValues.Min() : 0;

			var maximumValues = (from vs in _values.Values
				where vs.Count > 0
				select vs.Max()).ToArray();

			_maximumValue = maximumValues.Any() ? maximumValues.Max() : 9;

			AdjustMinMax(_desiredValueGridCount);

			SetValueGridValues();

			SetLocations();

			RenderGraphs();
		}

		private void SetLocations()
		{
			if (_valueAxis == null || _valueGrid == null || _categoryAxis == null)
			{
				return;
			}

			// in SilverLight event sequence is different and SetValueGridValues() is called too early for the first time
			if (_valueGridValues.Count == 0)
			{
				SetValueGridValues();
			}

			SetPointLocations();
			SetValueGridLocations();

			_valueAxis.SetLocations(_valueGridLocations);
			_valueGrid.SetLocations(_valueGridLocations);

			SetCategoryGridLocations();

			_categoryAxis.SetValues(_categoryGridValues);
			_categoryAxis.SetLocations(_categoryGridLocations);
		}

		private void SetCategoryGridLocations()
		{
			var gridCount = GetCategoryGridCount(_categoryGridCount);

			if (gridCount == 0)
			{
				return;
			}

			var gridStep = _categoryValues.Count / gridCount;

			_categoryGridValues.Clear();
			_categoryGridLocations.Clear();

			if (gridStep <= 0)
			{
				return;
			}

			for (var i = 0 ; i < _categoryValues.Count ; i += gridStep)
			{
				_categoryGridValues.Add(_categoryValues[i]);
				_categoryGridLocations.Add(_categoryLocations[i]);
			}
		}

		private int GetCategoryGridCount(int gridCountHint)
		{
			var gridCount = gridCountHint;
			if (gridCountHint >= _categoryValues.Count)
			{
				gridCount = _categoryValues.Count;
			}
			else
			{
				var hint = gridCountHint;
				while ((_categoryValues.Count - 1) % hint != 0 && hint > 1)
				{
					hint--;
				}

				if (hint == 1)
				{
					hint = gridCountHint;
					while ((_categoryValues.Count - 1) % hint != 0 && hint < Math.Min(_categoryValues.Count, gridCountHint * 2))
					{
						hint++;
					}
				}

				if (hint < gridCountHint * 2)
				{
					gridCount = hint;
				}
			}
			return gridCount;
		}

		private void AdjustMinMax(int desiredGridCount)
		{
			// TODO: refactor into something more comprehensible
			var min = _minimumValue;
			var max = _maximumValue;

			if (Math.Abs(min - 0) < Double.Epsilon && Math.Abs(max - 0) < Double.Epsilon)
			{
				max = 9;
			}

			if (min > max)
			{
				min = max - 1;
			}

			// "beautify" min/max
			var initialMinimum = min;
			var initialMaximum = max;

			var difference = max - min;
			double evaluatedDifference;

			if (Math.Abs(difference - 0) < Double.Epsilon)
			{
				// difference is 0 if all values of the period are equal
				// then difference will be
				evaluatedDifference = Math.Pow(10, Math.Floor(Math.Log(Math.Abs(max)) * Math.Log10(Math.E))) / 10;
			}
			else
			{
				evaluatedDifference = Math.Pow(10, Math.Floor(Math.Log(Math.Abs(difference)) * Math.Log10(Math.E))) / 10;
			}

			// new min and max
			max = Math.Ceiling(max / evaluatedDifference) * evaluatedDifference + evaluatedDifference;
			min = Math.Floor(min / evaluatedDifference) * evaluatedDifference - evaluatedDifference;

			// new difference
			difference = max - min;
			evaluatedDifference = Math.Pow(10, Math.Floor(Math.Log(Math.Abs(difference)) * Math.Log10(Math.E))) / 10;

			// approx size of the step
			var step = Math.Ceiling((difference / desiredGridCount) / evaluatedDifference) * evaluatedDifference;
			var evaluatedStep = Math.Pow(10, Math.Floor(Math.Log(Math.Abs(step)) * Math.Log10(Math.E)));

			var temp = Math.Ceiling(step / evaluatedStep); //number from 1 to 10

			if (temp > 5)
			{
				temp = 10;
			}

			if (temp <= 5 && temp > 2)
			{
				temp = 5;
			}

			//real step
			step = Math.Ceiling(step / (evaluatedStep * temp)) * evaluatedStep * temp;

			min = step * Math.Floor(min / step); //final max
			max = step * Math.Ceiling(max / step); //final min

			if (min < 0 && initialMinimum >= 0)
			{
				//min is zero if initial min > 0
				min = 0;
			}

			if (max > 0 && initialMaximum <= 0)
			{
				//min is zero if initial min > 0
				max = 0;
			}

			_valueGridStep = step;
			_adjustedMinimumValue = min;
			_adjustedMaximumValue = max;

			// ground value (starting point for column an similar graphs)
			_groundValue = (min <= 0 && 0 <= max) ? 0 : (max > 0 ? min : max);
		}

		private void InvalidateMinMax()
		{
			SetMinMax();
		}

		private Point GetPointCoordinates(int index, double value)
		{
			return new Point(GetXCoordinate(index), GetYCoordinate(value));
		}

		private double GetXCoordinate(int index)
		{
			var count = _values[_values.Keys.First()].Count;
			var step = _plotAreaInnerSize.Width / count;

			return step * index + step / 2;
		}

		private double GetYCoordinate(double value)
		{
			return _plotAreaInnerSize.Height - _plotAreaInnerSize.Height * ((value - _adjustedMinimumValue) / (_adjustedMaximumValue - _adjustedMinimumValue));
		}

		private int GetIndexByCoordinate(double x)
		{
			var index = -1;

			if (_values.Count <= 0)
			{
				return index;
			}

			var count = _values[_values.Keys.First()].Count;
			var step = _plotAreaInnerSize.Width / count;

			index = (int)Math.Round((x - step / 2) / step);
			index = Math.Max(0, index);
			index = Math.Min(count - 1, index);

			return index;
		}

		private void SetPointLocations()
		{
			_locations.Clear();

			if (_values.Count > 0)
			{
				var paths = GetDistinctPaths();

				foreach (var path in paths)
				{
					_locations.Add(path, new PointCollection());
					for (var i = 0 ; i < _values[path].Count ; i++)
						_locations[path].Add(GetPointCoordinates(i, _values[path][i]));
				}

				foreach (var graph in _graphs)
					graph.SetPointLocations(_locations[graph.ValueMemberPath], GetYCoordinate(_groundValue));
			}

			SetCategoryLocations();
		}

		private void SetCategoryLocations()
		{
			_categoryLocations.Clear();

			for (var i = 0 ; i < _categoryValues.Count ; i++)
				_categoryLocations.Add(GetXCoordinate(i));
		}

		private void SetValueGridValues()
		{
			if (_valueAxis == null || !(_valueGridStep > 0))
			{
				return;
			}

			_valueGridValues.Clear();
			for (var d = _adjustedMinimumValue + _valueGridStep ; d <= _adjustedMaximumValue ; d += _valueGridStep)
				_valueGridValues.Add(d);
			_valueAxis.SetValues(_valueGridValues);
		}

		private void SetValueGridLocations()
		{
			_valueGridLocations.Clear();
			foreach (var value in _valueGridValues)
				_valueGridLocations.Add(GetYCoordinate(value));
		}

		private void OnLayoutUpdated(object sender, EventArgs e)
		{
			RenderGraphs();
		}

		private void RenderGraphs()
		{
			foreach (var graph in _graphs)
				graph.Render();
		}

		private readonly List<Brush> _presetBrushes = new List<Brush>
		{
			new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x66, 0x00)),
			new SolidColorBrush(Color.FromArgb(0xFF, 0xFC, 0xD2, 0x02)),
			new SolidColorBrush(Color.FromArgb(0xFF, 0xB0, 0xDE, 0x09)),
			new SolidColorBrush(Color.FromArgb(0xFF, 0x0D, 0x8E, 0xCF)),
			new SolidColorBrush(Color.FromArgb(0xFF, 0x2A, 0x0C, 0xD0)),
			new SolidColorBrush(Color.FromArgb(0xFF, 0xCD, 0x0D, 0x74)),
			new SolidColorBrush(Color.FromArgb(0xFF, 0xCC, 0x00, 0x00)),
			new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0xCC, 0x00)),
			new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0xCC)),
			new SolidColorBrush(Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD)),
			new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x99, 0x99)),
			new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0x33, 0x33)),
			new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x00, 0x00))
		};

		/// <summary>
		///     Gets a collection of preset brushes used for graphs when their Brush property isn't set explicitly.
		/// </summary>
		public List<Brush> PresetBrushes { get { return _presetBrushes; } }

		private int _desiredValueGridCount = 5;
		private int _categoryGridCount = 5;

		/// <summary>
		///     Identifies <see cref="MinimumValueGridStep" /> dependency property.
		/// </summary>
		public static readonly DependencyProperty MinimumValueGridStepProperty = DependencyProperty.Register(
			"MinimumValueGridStep",
			typeof (double),
			typeof (SerialChart),
			new PropertyMetadata(30.0)
			);

		/// <summary>
		///     Gets or sets minimum size of a single step in value grid/value axis values.
		///     This is a dependency property.
		///     The default is 30.
		/// </summary>
		/// <remarks>
		///     When chart is resized and distance between grid lines becomes lower than value of MinimumValueGridStep
		///     chart decreases number of grid lines.
		/// </remarks>
		public double MinimumValueGridStep { get { return (double)GetValue(MinimumValueGridStepProperty); } set { SetValue(MinimumValueGridStepProperty, value); } }

		/// <summary>
		///     Identifies <see cref="MinimumCategoryGridStep" /> dependency property.
		/// </summary>
		public static readonly DependencyProperty MinimumCategoryGridStepProperty = DependencyProperty.Register(
			"MinimumCategoryGridStep",
			typeof (double),
			typeof (SerialChart),
			new PropertyMetadata(70.0)
			);

		/// <summary>
		///     Gets or sets minimum distance between 2 value tick on category axis.
		///     This is a dependency property.
		///     The default is 70.
		/// </summary>
		/// <remarks>
		///     When chart is resized and distance between grid lines becomes lower than value of MinimumCategoryGridStep
		///     chart decreases number of grid lines.
		/// </remarks>
		public double MinimumCategoryGridStep
		{
			get { return (double)GetValue(MinimumCategoryGridStepProperty); }
			set { SetValue(MinimumCategoryGridStepProperty, value); }
		}

		//// PLOT AREA

		/// <summary>
		///     Identifies <see cref="PlotAreaBackground" /> dependency property.
		/// </summary>
		public static readonly DependencyProperty PlotAreaBackgroundProperty = DependencyProperty.Register(
			"PlotAreaBackground",
			typeof (Brush),
			typeof (SerialChart),
			new PropertyMetadata(new SolidColorBrush(Colors.White))
			);

		/// <summary>
		///     Gets or sets a brush used as a background for plot area (the area inside of axes).
		///     This is a dependency property.
		///     The default is White.
		/// </summary>
		public Brush PlotAreaBackground { get { return (Brush)GetValue(PlotAreaBackgroundProperty); } set { SetValue(PlotAreaBackgroundProperty, value); } }

		//// LEGEND

		/// <summary>
		///     Identifies <see cref="LegendVisibility" /> dependency property.
		/// </summary>
		public static readonly DependencyProperty LegendVisibilityProperty = DependencyProperty.Register(
			"LegendVisibility",
			typeof (Visibility),
			typeof (SerialChart),
			new PropertyMetadata(Visibility.Visible));

		/// <summary>
		///     Gets or sets visibility of the chart legend.
		///     This is a dependency property.
		///     The default is Visible.
		/// </summary>
		public Visibility LegendVisibility { get { return (Visibility)GetValue(LegendVisibilityProperty); } set { SetValue(LegendVisibilityProperty, value); } }

		/// <summary>
		///     Identifies <see cref="AxisForeground" /> dependency property.
		/// </summary>
		public static readonly DependencyProperty AxisForegroundProperty = DependencyProperty.Register(
			"AxisForeground",
			typeof (Brush),
			typeof (SerialChart),
			new PropertyMetadata(new SolidColorBrush(Colors.Black))
			);

		/// <summary>
		///     Gets or sets foreground color of the axes.
		///     This is a dependency property.
		///     The default is Black.
		/// </summary>
		public Brush AxisForeground { get { return (Brush)GetValue(AxisForegroundProperty); } set { SetValue(AxisForegroundProperty, value); } }

		/// <summary>
		///     Gets or sets the format string to be used to format the category values.
		///     This is a dependency property.
		/// </summary>
		public string CategoryFormatString { get { return (string)GetValue(CategoryFormatStringProperty); } set { SetValue(CategoryFormatStringProperty, value); } }

		/// <summary>
		///     Identifies <see cref="CategoryFormatString" /> dependency property.
		/// </summary>
		public static readonly DependencyProperty CategoryFormatStringProperty = DependencyProperty.Register(
			"CategoryFormatString",
			typeof (string),
			typeof (SerialChart),
			new PropertyMetadata(null)
			);

		/// <summary>
		///     Identifies <see cref="GridStroke" /> dependency property.
		/// </summary>
		public static readonly DependencyProperty GridStrokeProperty = DependencyProperty.Register(
			"GridStroke",
			typeof (Brush),
			typeof (SerialChart),
			new PropertyMetadata(new SolidColorBrush(Colors.LightGray))
			);

		/// <summary>
		///     Gets or sets stroke brush for the value grid lines.
		///     This is a dependency property.
		///     The default is LightGray.
		/// </summary>
		public Brush GridStroke { get { return (Brush)GetValue(GridStrokeProperty); } set { SetValue(GridStrokeProperty, value); } }

		/// <summary>
		///     Identifies <see cref="ValueFormatString" /> dependency property.
		/// </summary>
		public static readonly DependencyProperty ValueFormatStringProperty = DependencyProperty.Register(
			"ValueFormatString",
			typeof (string),
			typeof (SerialChart),
			new PropertyMetadata(null)
			);

		/// <summary>
		///     Gets or sets the format string used to format values on axes and in tooltips.
		///     This is a dependency property.
		/// </summary>
		public string ValueFormatString { get { return (string)GetValue(ValueFormatStringProperty); } set { SetValue(ValueFormatStringProperty, value); } }
	}
}