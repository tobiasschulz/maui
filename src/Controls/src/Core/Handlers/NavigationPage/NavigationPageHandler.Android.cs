using System;
using Android.Runtime;
using Android.Views;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers
{
	// Currently only inheriting because we can't tap into CreateNativeView
	internal partial class NavigationPageHandler : Microsoft.Maui.Handlers.NavigationPageHandler
	{
		public new NavigationPageView NativeView =>
			(NavigationPageView)base.NativeView;

		//public static PropertyMapper<NavigationPage, NavigationPageHandler> NavigationPageMapper = 
		//	new PropertyMapper<NavigationPage, NavigationPageHandler>(ViewHandler.ViewMapper)
		//{

		//};

		public NavigationPageHandler() : base()
		{

		}

		NavigationPage _oldView;
		protected override NavigationLayout CreateNativeView()
		{
			LayoutInflater li = LayoutInflater.From(Context);
			_ = li ?? throw new InvalidOperationException($"LayoutInflater cannot be null");
			var view = li.Inflate(Resource.Layout.navigationlayoutcontrols, null).JavaCast<NavigationPageView>();
			return view;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			if (view != _oldView)
			{
				NativeView.OnElementChanged(new ElementChangedEventArgs<NavigationPage>(_oldView, (NavigationPage)view));
				_oldView = (NavigationPage)view;
			}
		}
	}
}
