#region Namespaces

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;

#endregion

namespace QuickCharts
{
	/// <summary>
	///     Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1
	{
		private readonly ObservableCollection<TestDataItem> _data = new ObservableCollection<TestDataItem>
		{
			new TestDataItem
			{
				Category = "cat1",
				Value1 = 5,
				Value2 = 15,
				Value3 = 12
			},
			new TestDataItem
			{
				Category = "cat2",
				Value1 = 15.2,
				Value2 = 1.5,
				Value3 = 2.1
			},
			new TestDataItem
			{
				Category = "cat3",
				Value1 = 25,
				Value2 = 5,
				Value3 = 2
			},
			new TestDataItem
			{
				Category = "cat4",
				Value1 = 8.1,
				Value2 = 1,
				Value3 = 22
			},
		};

		private Random _random;

		public Window1()
		{
			InitializeComponent();
		}

		public ObservableCollection<TestDataItem> Data { get { return _data; } }

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			DataContext = this;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			_data.Insert(2,
				new TestDataItem
				{
					Category = "cat2.2",
					Value1 = 1,
					Value2 = 2,
					Value3 = 3
				});
			_data.Insert(4,
				new TestDataItem
				{
					Category = "cat3.2",
					Value1 = 31,
					Value2 = 32,
					Value3 = 33
				});
			_data.Add(new TestDataItem
			{
				Category = "cat_new",
				Value1 = 12,
				Value2 = 22,
				Value3 = 32
			});
		}

		private void Button2_Click(object sender, RoutedEventArgs e)
		{
			DataContext = null;
			_data.Clear();
			var random = new Random();
			for (var i = 0 ; i < 1000 ; i++)
			{
				_data.Add(new TestDataItem
				{
					Category = i.ToString(CultureInfo.InvariantCulture),
					Value1 = random.NextDouble() * 8,
					Value2 = random.NextDouble() * 18,
					Value3 = random.NextDouble() * 5 + 3
				});
			}
			DataContext = this;
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			_random = new Random();

			var timer = new DispatcherTimer();
			timer.Tick += timer_Tick;
			timer.Interval = TimeSpan.FromSeconds(1);
			timer.Start();
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			_data.Add(new TestDataItem
			{
				Category = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture),
				Value1 = _random.NextDouble() * 8,
				Value2 = _random.NextDouble() * 18,
				Value3 = _random.NextDouble() * 5 + 3
			});
			_data.RemoveAt(0);
		}
	}

	public class TestDataItem
	{
		public string Category { get; set; }
		public double Value1 { get; set; }
		public double Value2 { get; set; }
		public double Value3 { get; set; }
	}
}