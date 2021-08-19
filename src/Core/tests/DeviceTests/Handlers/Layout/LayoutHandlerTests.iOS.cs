using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.DeviceTests.Handlers.Layout
{
	public partial class LayoutHandlerTests
	{
		double GetNativeChildCount(LayoutHandler layoutHandler)
		{
			return ((layoutHandler as IElementHandler).NativeView as UIView).Subviews.Length;
		}
	}
}