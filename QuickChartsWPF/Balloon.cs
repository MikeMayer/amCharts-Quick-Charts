﻿#region Namespaces

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#endregion

namespace AmCharts.Windows.QuickCharts
{
	/// <summary>
	///     Represents a value balloon (tooltip).
	/// </summary>
	public class Balloon : Control
	{
		/// <summary>
		///     Identifies <see cref="Text" /> dependency property.
		/// </summary>
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			"Text",
			typeof (string),
			typeof (Balloon),
			new PropertyMetadata(null)
			);

		/// <summary>
		///     Instantiates Balloon.
		/// </summary>
		public Balloon()
		{
			this.DefaultStyleKey = typeof (Balloon);
			this.IsHitTestVisible = false;
#if !SILVERLIGHT
			Background = Brushes.WhiteSmoke;
#endif
		}

		/// <summary>
		///     Gets or sets balloon text.
		///     This is a dependency property.
		/// </summary>
		public string Text { get { return (string)GetValue(TextProperty); } set { SetValue(TextProperty, value); } }
	}
}