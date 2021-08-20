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
			[nameof(INavigationView.PushAsync)] = PushAsyncTo,
			[nameof(INavigationView.PopAsync)] = PopAsyncTo,
			//[nameof(INavigationView.InsertPageBefore)] = PopAsyncTo,
			//[nameof(INavigationView.RemovePage)] = PopAsyncTo
		};

		public NavigationPageHandler() : base(NavigationPageMapper, NavigationViewCommandMapper)
		{
		}

		public NavigationPageHandler(PropertyMapper? mapper = null) : base(mapper ?? NavigationPageMapper, NavigationViewCommandMapper)
		{

		}
	}
}
