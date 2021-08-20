using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests.Handlers.Layout
{
	public partial class LayoutHandlerTests
	{
		double GetNativeChildCount(IElementHandler layoutHandler)
		{
			return GetNativeChildCount(layoutHandler.NativeView as LayoutViewGroup);
		}

		double GetNativeChildCount(object nativeView)
		{
			return (nativeView as LayoutViewGroup).ChildCount;
		}
	}
}