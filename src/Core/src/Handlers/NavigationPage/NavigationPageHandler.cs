using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;


namespace Microsoft.Maui.Handlers
{
	internal partial class NavigationPageHandler
	{
		public static PropertyMapper<INavigationView, NavigationPageHandler> NavigationPageMapper
			   = new PropertyMapper<INavigationView, NavigationPageHandler>(ViewHandler.ViewMapper);

		public static CommandMapper<INavigationView, NavigationPageHandler> NavigationViewCommandMapper = new(ViewCommandMapper)
		{
			[nameof(INavigationViewInternal.PushAsync)] = PushAsyncTo,
			[nameof(INavigationViewInternal.PopAsync)] = PopAsyncTo,
			//[nameof(INavigationViewInternal.InsertPageBefore)] = PopAsyncTo,
			//[nameof(INavigationViewInternal.RemovePage)] = PopAsyncTo
		};

		public NavigationPageHandler() : base(NavigationPageMapper, NavigationViewCommandMapper)
		{
		}

		public NavigationPageHandler(PropertyMapper? mapper = null) : base(mapper ?? NavigationPageMapper, NavigationViewCommandMapper)
		{

		}
	}
}
