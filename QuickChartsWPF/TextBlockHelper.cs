#region Namespaces

using System.Windows;
using System.Windows.Controls;

#endregion

namespace AmCharts.Windows.QuickCharts
{
	/// <summary>
	///     Helps unify handling of differing aspects of TextBlock in SilverLight and WPF.
	/// </summary>
	public static class TextBlockHelper
	{
		/// <summary>
		///     Workaround for a Silverlight issue when DesiredSize is not set after a call to Measure()
		///     but ActualWidth and ActualHeight are set instead.
		/// </summary>
		/// <param name="textBlock">TextBlock</param>
		/// <returns>Size of the TextBlock</returns>
		public static Size GetDesiredSize(this TextBlock textBlock)
		{
#if SILVERLIGHT
            return new Size(textBlock.ActualWidth, textBlock.ActualHeight);
#else
			return textBlock.DesiredSize;
#endif
		}
	}
}