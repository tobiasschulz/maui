using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests.Handlers.Layout
{
	public partial class LayoutHandlerTests
	{
		double GetNativeChildCount(LayoutHandler layoutHandler)
		{
			return ((layoutHandler as IElementHandler).NativeView as LayoutViewGroup).ChildCount;
		}
	}
}