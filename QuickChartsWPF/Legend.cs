#region Namespaces

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls;

#endregion

namespace AmCharts.Windows.QuickCharts
{
	/// <summary>
	///     Represents chart legend.
	/// </summary>
	public class Legend : ItemsControl
	{
		private readonly ObservableCollection<LegendItem> _itemsSource = new ObservableCollection<LegendItem>();

		private IEnumerable<ILegendItem> _legendItemsSource;

		/// <summary>
		///     Instantiates Legend.
		/// </summary>
		public Legend()
		{
			DefaultStyleKey = typeof (Legend);

			ItemsSource = _itemsSource;
		}

		/// <summary>
		///     Gets or sets legend item source.
		/// </summary>
		public IEnumerable<ILegendItem> LegendItemsSource
		{
			get { return _legendItemsSource; }
			set
			{
				if (value is INotifyCollectionChanged)
				{
					(value as INotifyCollectionChanged).CollectionChanged += OnLegendItemsSourceCollectionChanged;
				}
				_legendItemsSource = value;
				_itemsSource.Clear();
				AddLegendItems(value.ToList());
			}
		}

		private void OnLegendItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				_itemsSource.Clear();
			}
			else
			{
				if (e.OldItems != null)
				{
					foreach (ILegendItem item in e.NewItems)
						_itemsSource.Remove(_itemsSource.First(p => p.OriginalItem == item));
				}

				AddLegendItems(e.NewItems);
			}
		}

		private void AddLegendItems(IEnumerable items)
		{
			if (items == null)
			{
				return;
			}

			foreach (ILegendItem item in items)
				_itemsSource.Add(new LegendItem(item));
		}
	}
}