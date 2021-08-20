using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Navigation;
using AndroidX.Navigation.Fragment;
using Google.Android.Material.AppBar;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public class NavigationLayout : CoordinatorLayout, NavController.IOnDestinationChangedListener
	{
		NavHostFragment? _navHost;
		FragmentNavigator? _fragmentNavigator;
		Toolbar? _toolbar;
		AppBarLayout? _appBar;

		internal IView? VirtualView { get; private set;  }
		internal INavigationView? NavigationView { get; private set; }

		public IMauiContext MauiContext => VirtualView?.Handler?.MauiContext ?? 
			throw new InvalidOperationException($"MauiContext cannot be null");

#pragma warning disable CS0618 //FIXME: [Preserve] is obsolete
		[Preserve(Conditional = true)]
		public NavigationLayout(Context context) : base(context)
		{
		}

		[Preserve(Conditional = true)]
		public NavigationLayout(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		[Preserve(Conditional = true)]
		public NavigationLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
		}

		[Preserve(Conditional = true)]
		protected NavigationLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}
#pragma warning restore CS0618 //FIXME: [Preserve] is obsolete

		NavHostFragment NavHost
		{
			get => _navHost ?? throw new InvalidOperationException($"NavHost cannot be null");
			set => _navHost = value;
		}

		FragmentNavigator FragmentNavigator
		{
			get => _fragmentNavigator ?? throw new InvalidOperationException($"FragmentNavigator cannot be null");
			set => _fragmentNavigator = value;
		}

		int NativeNavigationStackCount => NavHost?.NavController.BackStack.Size() - 1 ?? 0;
		int NavigationStackCount => NavigationView?.NavigationStack.Count ?? 0;

		internal Toolbar Toolbar
		{
			get => _toolbar ?? throw new InvalidOperationException($"ToolBar cannot be null");
			set => _toolbar = value;
		}

		internal AppBarLayout AppBar
		{
			get => _appBar ?? throw new InvalidOperationException($"AppBar cannot be null");
			set => _appBar = value;
		}


		public virtual void SetVirtualView(IView navigationView)
		{			
			_toolbar = FindViewById<Toolbar>(Resource.Id.maui_toolbar);
			_appBar = FindViewById<AppBarLayout>(Resource.Id.appbar);

			VirtualView = navigationView;
			NavigationView = (INavigationView)navigationView;
		}

		internal void Connect()
		{
			var fragmentManager = Context?.GetFragmentManager();
			_ = fragmentManager ?? throw new InvalidOperationException($"GetFragmentManager returned null");
			_ = NavigationView ?? throw new InvalidOperationException($"VirtualView cannot be null");

			NavHost = (NavHostFragment)
				fragmentManager.FindFragmentById(Resource.Id.nav_host);

			FragmentNavigator =
				(FragmentNavigator)NavHost
					.NavController
					.NavigatorProvider
					.GetNavigator(Java.Lang.Class.FromType(typeof(FragmentNavigator)));


			var navGraphNavigator =
				(NavGraphNavigator)NavHost
					.NavController
					.NavigatorProvider
					.GetNavigator(Java.Lang.Class.FromType(typeof(NavGraphNavigator)));

			NavGraph graph = new NavGraph(navGraphNavigator);

			NavDestination navDestination;
			List<int> destinations = new List<int>();
			foreach (var page in NavigationView.NavigationStack)
			{
				navDestination =
					MauiFragmentNavDestination.
						AddDestination(
							page,
							this,
							graph,
							FragmentNavigator);

				destinations.Add(navDestination.Id);
			}

			graph.StartDestination = destinations[0];

			NavHost.NavController.SetGraph(graph, null);

			for (var i = NativeNavigationStackCount; i < NavigationStackCount; i++)
			{
				var dest = destinations[i];
				NavHost.NavController.Navigate(dest);
			}

			NavHost.NavController.AddOnDestinationChangedListener(this);
			NavHost.ChildFragmentManager.RegisterFragmentLifecycleCallbacks(new FragmentLifecycleCallback(this), false);
		}

		internal virtual void OnPageFragmentDestroyed(AndroidX.Fragment.App.FragmentManager fm, NavHostPageFragment navHostPageFragment)
		{
		}

		internal virtual void OnFragmentResumed(AndroidX.Fragment.App.FragmentManager fm, NavHostPageFragment navHostPageFragment)
		{
		}


		class FragmentLifecycleCallback : AndroidX.Fragment.App.FragmentManager.FragmentLifecycleCallbacks
		{
			NavigationLayout _navigationLayout;

			public FragmentLifecycleCallback(NavigationLayout navigationLayout)
			{
				_navigationLayout = navigationLayout;
			}


			public override void OnFragmentResumed(AndroidX.Fragment.App.FragmentManager fm, AndroidX.Fragment.App.Fragment f)
			{
				if (f is NavHostPageFragment pf)
					_navigationLayout.OnFragmentResumed(fm, pf);
			}

			public override void OnFragmentAttached(AndroidX.Fragment.App.FragmentManager fm, AndroidX.Fragment.App.Fragment f, Context context)
			{
				base.OnFragmentAttached(fm, f, context);
			}

			public override void OnFragmentViewDestroyed(
				AndroidX.Fragment.App.FragmentManager fm, 
				AndroidX.Fragment.App.Fragment f)
			{
				if (f is NavHostPageFragment pf)
					_navigationLayout.OnPageFragmentDestroyed(fm, pf);

				base.OnFragmentViewDestroyed(fm, f);
			}
		}

		public virtual void Push(MauiNavigationRequestedEventArgs e)
		{
			var destination =
				MauiFragmentNavDestination.AddDestination(e.Page, this, NavHost.NavController.Graph, FragmentNavigator);

			NavOptions? navOptions = null;

			if (e.Animated)
			{
				new NavOptions.Builder()
					 .SetEnterAnim(Resource.Animation.enterfromright)
					 .SetExitAnim(Resource.Animation.exittoleft)
					 .SetPopEnterAnim(Resource.Animation.enterfromleft)
					 .SetPopExitAnim(Resource.Animation.exittoright)
					 .Build();
			}

			NavHost.NavController.Navigate(destination.Id, null, navOptions);
		}

		public virtual void Pop(object? arg3)
		{
			NavHost.NavController.NavigateUp();
		}

		internal void OnPop()
		{
			_ = NavigationView ?? throw new InvalidOperationException($"VirtualView cannot be null");

			NavigationView
				.PopAsync()
				.FireAndForget((e) =>
				{
					//Log.Warning(nameof(NavigationPageHandler), $"{e}");
				});
		}

		public void OnDestinationChanged(NavController p0, NavDestination p1, Bundle p2)
		{
		}
	}
}
