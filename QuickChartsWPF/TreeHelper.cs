#region Namespaces

using System.Windows;
using System.Windows.Controls;

#endregion

#if SILVERLIGHT
	using System.Windows.Media;
#endif

namespace AmCharts.Windows.QuickCharts
{
	/// <summary>
	///     Helper class to unify certain aspects of navigating VisualTree in WPF and SilverLight.
	/// </summary>
	public static class TreeHelper
	{
		/// <summary>
		///     Finds object in control's template by it's name.
		/// </summary>
		/// <param name="name">Objects name.</param>
		/// <param name="templatedParent">Templated parent.</param>
		/// <returns>Object reference if found, null otherwise.</returns>
		public static object TemplateFindName(string name, FrameworkElement templatedParent)
		{
#if SILVERLIGHT
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(templatedParent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(templatedParent, i);
                if (child is FrameworkElement)
                {
                    if (((FrameworkElement)child).Name == name)
                    {
                        return child;
                    }
                    else
                    {
                        object subChild = TreeHelper.TemplateFindName(name, (FrameworkElement)child);
                        if (subChild != null && subChild is FrameworkElement && ((FrameworkElement)subChild).Name == name)
                        {
                            return subChild;
                        }
                    }
                }
            }
            return null;
#else
			return ((Control)templatedParent).Template.FindName(name, templatedParent);
#endif
		}
	}
}