using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.Navigation;
using AndroidX.Navigation.Fragment;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui
{
	public class MauiFragmentNavDestination : FragmentNavigator.Destination
	{
		public IView Page { get; }
		public IMauiContext MauiContext => NavigationLayout.MauiContext ?? throw new InvalidOperationException($"MauiContext cannot be null here");
		public NavigationLayout NavigationLayout { get; }

		// Todo we want to generate the same ids for each page so if the app is recreated
		// we want these to match up
		static Dictionary<IView, int> Pages = new Dictionary<IView, int>();

		public MauiFragmentNavDestination(Navigator fragmentNavigator, IView page, NavigationLayout navigationLayout) : base(fragmentNavigator)
		{
			_ = page ?? throw new ArgumentNullException(nameof(page));
			_ = navigationLayout ?? throw new ArgumentNullException(nameof(navigationLayout));
			SetClassName(Java.Lang.Class.FromType(typeof(NavHostPageFragment)).CanonicalName);

			if (!Pages.ContainsKey(page))
			{
				Id = global::Android.Views.View.GenerateViewId();
				Pages.Add(page, Id);
			}

			Id = Pages[page];
			this.Page = page;
			this.NavigationLayout = navigationLayout;
		}

		public static MauiFragmentNavDestination AddDestination(
			IView page,
			NavigationLayout navigationLayout,
			NavGraph navGraph,
			FragmentNavigator navigator)
		{
			var destination = new MauiFragmentNavDestination(navigator, page, navigationLayout);

			navGraph.AddDestination(destination);
			return destination;
		}
	}
}
