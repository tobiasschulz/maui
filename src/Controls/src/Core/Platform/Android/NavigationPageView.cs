using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.AppCompat.Graphics.Drawable;
using AndroidX.DrawerLayout.Widget;
using Google.Android.Material.AppBar;
using Microsoft.Maui.Controls.Internals;
using static Android.Views.View;
using static Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat.NavigationPage;
using ActionBarDrawerToggle = AndroidX.AppCompat.App.ActionBarDrawerToggle;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;
using AView = Android.Views.View;
using Color = Microsoft.Maui.Graphics.Color;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;
using FragmentTransaction = AndroidX.Fragment.App.FragmentTransaction;
using Object = Java.Lang.Object;

namespace Microsoft.Maui.Controls.Platform
{
	public class NavigationPageView : NavigationLayout, IManageFragments, IOnClickListener, ILifeCycleState
	{
		//Drawable _backgroundDrawable;
		Page _current;

		//bool _disposed;
		ActionBarDrawerToggle _drawerToggle;
		FragmentManager _fragmentManager;
		//int _lastActionBarHeight = -1;
		int _statusbarHeight;
		MaterialToolbar _toolbar;
		private AppBarLayout _appBar;
		ToolbarTracker _toolbarTracker;
		DrawerMultiplexedListener _drawerListener;
		DrawerLayout _drawerLayout;
		FlyoutPage _flyoutPage;
		bool _toolbarVisible;
		IViewHandler _titleViewHandler;
		Container _titleView;
		Android.Widget.ImageView _titleIconView;
		ImageSource _imageSource;
		//bool _isAttachedToWindow;
		string _defaultNavigationContentDescription;
		List<IMenuItem> _currentMenuItems = new List<IMenuItem>();
		List<ToolbarItem> _currentToolbarItems = new List<ToolbarItem>();

		// The following is based on https://android.googlesource.com/platform/frameworks/support.git/+/4a7e12af4ec095c3a53bb8481d8d92f63157c3b7/v4/java/android/support/v4/app/FragmentManager.java#677
		// Must be overriden in a custom renderer to match durations in XML animation resource files
		protected virtual int TransitionDuration { get; set; } = 220;
		bool ILifeCycleState.MarkedForDispose { get; set; } = false;

		NavigationPage Element { get; set; }

		public NavigationPageView(Context context) : base(context)
		{
			Id = AView.GenerateViewId();
		}

		public NavigationPageView(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		public NavigationPageView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
		}

		protected NavigationPageView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		INavigationPageController NavigationPageController => Element as INavigationPageController;

		internal int ContainerTopPadding { get; set; }
		internal int ContainerBottomPadding { get; set; }

		Page Current
		{
			get { return _current; }
			set
			{
				if (_current == value)
					return;

				if (_current != null)
					_current.PropertyChanged -= CurrentOnPropertyChanged;

				_current = value;

				if (_current != null)
				{
					_current.PropertyChanged += CurrentOnPropertyChanged;
					ToolbarVisible = NavigationPage.GetHasNavigationBar(_current);
				}
			}
		}

		FragmentManager FragmentManager => _fragmentManager ?? (_fragmentManager = Context.GetFragmentManager());

		IPageController PageController => Element;

		bool ToolbarVisible
		{
			get { return _toolbarVisible; }
			set
			{
				if (_toolbarVisible == value)
					return;

				_toolbarVisible = value;

				if (!IsLayoutRequested)
					RequestLayout();
			}
		}

		void IManageFragments.SetFragmentManager(FragmentManager childFragmentManager)
		{
			if (_fragmentManager == null)
				_fragmentManager = childFragmentManager;
		}

		internal void SetVirtualView(NavigationPage view)
		{
			Element = view;
			if (_toolbarTracker == null)
			{
				_toolbar = FindViewById<MaterialToolbar>(Resource.Id.maui_toolbar);
				_appBar = FindViewById<AppBarLayout>(Resource.Id.appbar);
				_toolbarTracker = new ToolbarTracker();
				_toolbarTracker.CollectionChanged += ToolbarTrackerOnCollectionChanged;
			}
			_toolbarTracker.AdditionalTargets = Element.GetParentPages();
		}

		internal void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == NavigationPage.BarBackgroundColorProperty.PropertyName)
				UpdateToolbar();
			else if (e.PropertyName == NavigationPage.BarBackgroundProperty.PropertyName)
				UpdateToolbar();
			else if (e.PropertyName == NavigationPage.BarTextColorProperty.PropertyName)
				UpdateToolbar();
			else if (e.PropertyName == BarHeightProperty.PropertyName)
				UpdateToolbar();
			else if (e.PropertyName == AutomationProperties.NameProperty.PropertyName)
				UpdateToolbar();
			else if (e.PropertyName == AutomationProperties.HelpTextProperty.PropertyName)
				UpdateToolbar();
		}


		//protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		//{
		//	var navView = GetChildAt(0);
		//	navView.Measure(widthMeasureSpec, heightMeasureSpec);
		//	SetMeasuredDimension(navView.MeasuredWidth, navView.MeasuredHeight);
		//}


		// TODO MAUI
		//protected override void OnLayout(bool changed, int l, int t, int r, int b)
		//{
		//	AToolbar bar = _toolbar;
		//	// make sure bar stays on top of everything
		//	bar.BringToFront();

		//	int barHeight = ActionBarHeight();

		//	if (Element.IsSet(BarHeightProperty))
		//		barHeight = Element.OnThisPlatform().GetBarHeight();

		//	if (barHeight != _lastActionBarHeight && _lastActionBarHeight > 0)
		//	{
		//		ResetToolbar();
		//		bar = _toolbar;
		//	}
		//	_lastActionBarHeight = barHeight;

		//	bar.Measure(MeasureSpecMode.Exactly.MakeMeasureSpec(r - l), MeasureSpecMode.Exactly.MakeMeasureSpec(barHeight));

		//	var barOffset = ToolbarVisible ? barHeight : 0;
		//	int containerHeight = b - t - ContainerTopPadding - barOffset - ContainerBottomPadding;

		//	PageController.ContainerArea = new Rectangle(0, 0, Context.FromPixels(r - l), Context.FromPixels(containerHeight));

		//	// Potential for optimization here, the exact conditions by which you don't need to do this are complex
		//	// and the cost of doing when it's not needed is moderate to low since the layout will short circuit pretty fast
		//	Element.ForceLayout();

		//	//var navView = GetChildAt(0);
		//	//navView.Layout(l, t, r, b);
		//	base.OnLayout(changed, l, t, r, b);

		//	bool toolbarLayoutCompleted = false;
		//	for (var i = 0; i < ChildCount; i++)
		//	{
		//		AView child = GetChildAt(i);

		//		Page childPage = (child as PageContainer)?.Child?.VirtualView as Page;

		//		if (childPage == null)
		//			return;

		//		// We need to base the layout of both the child and the bar on the presence of the NavBar on the child Page itself.
		//		// If we layout the bar based on ToolbarVisible, we get a white bar flashing at the top of the screen.
		//		// If we layout the child based on ToolbarVisible, we get a white bar flashing at the bottom of the screen.
		//		bool childHasNavBar = NavigationPage.GetHasNavigationBar(childPage);

		//		if (childHasNavBar)
		//		{
		//			bar.Layout(0, 0, r - l, barHeight);
		//			child.Layout(0, barHeight + ContainerTopPadding, r, b - ContainerBottomPadding);
		//		}
		//		else
		//		{
		//			bar.Layout(0, -1000, r, barHeight - 1000);
		//			child.Layout(0, ContainerTopPadding, r, b - ContainerBottomPadding);
		//		}
		//		toolbarLayoutCompleted = true;
		//	}

		//	// Making the layout of the toolbar dependant on having a child Page could potentially mean that the toolbar is not laid out.
		//	// We'll do one more check to make sure it isn't missed.
		//	if (!toolbarLayoutCompleted)
		//	{
		//		if (ToolbarVisible)
		//		{
		//			bar.Layout(0, 0, r - l, barHeight);
		//		}
		//		else
		//		{
		//			bar.Layout(0, -1000, r, barHeight - 1000);
		//		}
		//	}
		//}

		protected virtual void SetupPageTransition(FragmentTransaction transaction, bool isPush)
		{
			if (isPush)
				transaction.SetTransitionEx((int)FragmentTransit.FragmentOpen);
			else
				transaction.SetTransitionEx((int)FragmentTransit.FragmentClose);
		}

		internal int GetNavBarHeight()
		{
			if (!ToolbarVisible)
				return 0;

			return ActionBarHeight();
		}

		int ActionBarHeight()
		{
			int attr = Resource.Attribute.actionBarSize;

			int actionBarHeight;
			using (var tv = new TypedValue())
			{
				actionBarHeight = 0;
				if (Context.Theme.ResolveAttribute(attr, tv, true))
					actionBarHeight = TypedValue.ComplexToDimensionPixelSize(tv.Data, Resources.DisplayMetrics);
			}

			if (actionBarHeight <= 0)
				return Device.Info.CurrentOrientation.IsPortrait() ? (int)Context.ToPixels(56) : (int)Context.ToPixels(48);

			if (Context.GetActivity().Window.Attributes.Flags.HasFlag(WindowManagerFlags.TranslucentStatus) || Context.GetActivity().Window.Attributes.Flags.HasFlag(WindowManagerFlags.TranslucentNavigation))
			{
				if (_toolbar.PaddingTop == 0)
					_toolbar.SetPadding(0, GetStatusBarHeight(), 0, 0);

				return actionBarHeight + GetStatusBarHeight();
			}

			return actionBarHeight;
		}

		void AnimateArrowIn()
		{
			var icon = _toolbar.NavigationIcon as DrawerArrowDrawable;
			if (icon == null)
				return;

			ValueAnimator valueAnim = ValueAnimator.OfFloat(0, 1);
			valueAnim.SetDuration(200);
			valueAnim.Update += (s, a) => icon.Progress = (float)a.Animation.AnimatedValue;
			valueAnim.Start();
		}

		int GetStatusBarHeight()
		{
			if (_statusbarHeight > 0)
				return _statusbarHeight;

			int resourceId = Resources.GetIdentifier("status_bar_height", "dimen", "android");
			if (resourceId > 0)
				_statusbarHeight = Resources.GetDimensionPixelSize(resourceId);

			return _statusbarHeight;
		}

		void AnimateArrowOut()
		{
			var icon = _toolbar.NavigationIcon as DrawerArrowDrawable;
			if (icon == null)
				return;

			ValueAnimator valueAnim = ValueAnimator.OfFloat(1, 0);
			valueAnim.SetDuration(200);
			valueAnim.Update += (s, a) => icon.Progress = (float)a.Animation.AnimatedValue;
			valueAnim.Start();
		}

		public void OnClick(AView v)
		{
			Element?.PopAsync();
		}

		void CurrentOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == NavigationPage.HasNavigationBarProperty.PropertyName)
				ToolbarVisible = NavigationPage.GetHasNavigationBar(Current);
			else if (e.PropertyName == Page.TitleProperty.PropertyName)
				UpdateToolbar();
			else if (e.PropertyName == NavigationPage.HasBackButtonProperty.PropertyName)
				UpdateToolbar();
			else if (e.PropertyName == NavigationPage.TitleIconImageSourceProperty.PropertyName ||
					 e.PropertyName == NavigationPage.TitleViewProperty.PropertyName)
				UpdateToolbar();
			else if (e.PropertyName == NavigationPage.IconColorProperty.PropertyName)
				UpdateToolbar();
		}

		TaskCompletionSource<bool> _taskCompletionSource;
		public override void Push(MauiNavigationRequestedEventArgs e)
		{
			UpdateToolbar();

			if (e.Animated && _drawerToggle != null && NavigationPageController.StackDepth == 2 && NavigationPage.GetHasBackButton((Page)e.Page))
				AnimateArrowIn();


			_taskCompletionSource = new TaskCompletionSource<bool>();
			base.Push(e);
		}

		public override void Pop(object arg3)
		{
			if (_drawerToggle != null && NavigationPageController.StackDepth == 2 && NavigationPage.GetHasBackButton(_current))
				AnimateArrowOut();

			_taskCompletionSource = new TaskCompletionSource<bool>();
			base.Pop(arg3);
		}


		internal override void OnPageFragmentDestroyed(FragmentManager fm, NavHostPageFragment navHostPageFragment)
		{
			_taskCompletionSource = null;
		}

		internal override void OnFragmentResumed(FragmentManager fm, NavHostPageFragment navHostPageFragment)
		{
			base.OnFragmentResumed(fm, navHostPageFragment);
			_toolbarTracker.Target = (Page)navHostPageFragment.NavDestination.Page;
		}

		void OnPushed(object sender, NavigationRequestedEventArgs e)
		{
			//e.Task = PushViewAsync(e.Page, e.Animated);
		}

		//void OnRemovePageRequested(object sender, NavigationRequestedEventArgs e)
		//{
		//	RemovePage(e.Page);
		//}

		void RegisterToolbar()
		{
			Context context = Context;
			AToolbar bar = _toolbar;
			Element page = Element.RealParent;

			_flyoutPage = null;
			while (page != null)
			{
				if (page is FlyoutPage)
				{
					_flyoutPage = page as FlyoutPage;
					break;
				}
				page = page.RealParent;
			}

			if (_flyoutPage == null)
			{
				if (PageController.InternalChildren.Count > 0)
					_flyoutPage = PageController.InternalChildren[0] as FlyoutPage;

				if (_flyoutPage == null)
					return;
			}

			if (((IFlyoutPageController)_flyoutPage).ShouldShowSplitMode)
				return;

			var renderer = _flyoutPage.ToNative(Element.Handler.MauiContext) as DrawerLayout;
			if (renderer == null)
				return;

			_drawerLayout = renderer;

			AutomationPropertiesProvider.GetDrawerAccessibilityResources(context, _flyoutPage, out int resourceIdOpen, out int resourceIdClose);

			if (_drawerToggle != null)
			{
				_drawerToggle.ToolbarNavigationClickListener = null;
				_drawerToggle.Dispose();
			}

			_drawerToggle = new ActionBarDrawerToggle(context.GetActivity(), _drawerLayout, bar,
				resourceIdOpen == 0 ? global::Android.Resource.String.Ok : resourceIdOpen,
				resourceIdClose == 0 ? global::Android.Resource.String.Ok : resourceIdClose)
			{
				ToolbarNavigationClickListener = new ClickListener(Element)
			};

			if (_drawerListener != null)
			{
				_drawerLayout.RemoveDrawerListener(_drawerListener);
				_drawerListener.Dispose();
			}

			_drawerListener = new DrawerMultiplexedListener { Listeners = { _drawerToggle, (DrawerLayout.IDrawerListener)_drawerLayout } };
			_drawerLayout.AddDrawerListener(_drawerListener);
		}

		//		void RemovePage(Page page)
		//		{
		//			if (!_isAttachedToWindow)
		//				PushCurrentPages();

		//			Fragment fragment = GetPageFragment(page);

		//			if (fragment == null)
		//			{
		//				return;
		//			}

		//#if DEBUG
		//			// Enables logging of moveToState operations to logcat
		//#pragma warning disable CS0618 // Type or member is obsolete
		//			FragmentManager.EnableDebugLogging(true);
		//#pragma warning restore CS0618 // Type or member is obsolete
		//#endif

		//			// Go ahead and take care of the fragment bookkeeping for the page being removed
		//			FragmentTransaction transaction = FragmentManager.BeginTransactionEx();
		//			transaction.RemoveEx(fragment);
		//			transaction.CommitAllowingStateLossEx();

		//			// And remove the fragment from our own stack
		//			_fragmentStack.Remove(fragment);

		//			Device.StartTimer(TimeSpan.FromMilliseconds(10), () =>
		//			{
		//				UpdateToolbar();
		//				return false;
		//			});
		//		}

		//void ResetToolbar()
		//{
		//	AToolbar oldToolbar = _toolbar;

		//	_toolbar.SetNavigationOnClickListener(null);
		//	_toolbar.RemoveFromParent();

		//	_toolbar.RemoveView(_titleView);
		//	_titleView = null;

		//	if (_titleViewHandler != null)
		//	{
		//		_titleViewHandler.VirtualView.Handler = null;
		//		_titleViewHandler = null;
		//	}

		//	_toolbar.RemoveView(_titleIconView);
		//	_titleIconView = null;

		//	_imageSource = null;

		//	_toolbar = null;

		//	SetupToolbar();

		//	// if the old toolbar had padding from transluscentflags, set it to the new toolbar
		//	if (oldToolbar.PaddingTop != 0)
		//		_toolbar.SetPadding(0, oldToolbar.PaddingTop, 0, 0);

		//	RegisterToolbar();
		//	UpdateToolbar();
		//	UpdateMenu();

		//	// Preserve old values that can't be replicated by calling methods above
		//	if (_toolbar != null)
		//		_toolbar.Subtitle = oldToolbar.Subtitle;
		//}

		//		Task<bool> SwitchContentAsync(Page page, bool animated, bool removed = false, bool popToRoot = false)
		//		{
		//			animated = false;
		//			// TODO MAUI
		//			//if (!IsAttachedToRoot(Element))
		//			//	return Task.FromResult(false);

		//			var tcs = new TaskCompletionSource<bool>();
		//			Fragment fragment = GetFragment(page, removed, popToRoot);

		//#if DEBUG
		//			// Enables logging of moveToState operations to logcat
		//#pragma warning disable CS0618 // Type or member is obsolete
		//			FragmentManager.EnableDebugLogging(true);
		//#pragma warning restore CS0618 // Type or member is obsolete
		//#endif

		//			Current?.SendDisappearing();
		//			Current = page;

		//			// TODO MAUI
		//			//if (Platform != null)
		//			//{
		//			//	Platform.NavAnimationInProgress = true;
		//			//}

		//			FragmentTransaction transaction = FragmentManager.BeginTransactionEx();

		//			if (animated)
		//				SetupPageTransition(transaction, !removed);

		//			var fragmentsToRemove = new List<Fragment>();

		//			if (_fragmentStack.Count == 0)
		//			{
		//				transaction.AddEx(Resource.Id.nav_host, fragment);
		//				_fragmentStack.Add(fragment);
		//			}
		//			else
		//			{
		//				if (removed)
		//				{
		//					// pop only one page, or pop everything to the root
		//					var popPage = true;
		//					while (_fragmentStack.Count > 1 && popPage)
		//					{
		//						Fragment currentToRemove = _fragmentStack.Last();
		//						_fragmentStack.RemoveAt(_fragmentStack.Count - 1);
		//						transaction.RemoveEx(currentToRemove);
		//						fragmentsToRemove.Add(currentToRemove);
		//						popPage = popToRoot;
		//					}

		//					transaction.SetCustomAnimations(Resource.Animation.enterfromleft, Resource.Animation.exittoright);

		//					Fragment toShow = _fragmentStack.Last();
		//					// Execute pending transactions so that we can be sure the fragment list is accurate.
		//					// FragmentManager.ExecutePendingTransactionsEx();
		//					if (FragmentManager.Fragments.Contains(toShow))
		//						transaction.ShowEx(toShow);
		//					else
		//						transaction.AddEx(Resource.Id.nav_host, toShow);
		//				}
		//				else
		//				{
		//					transaction.SetCustomAnimations(Resource.Animation.enterfromright, Resource.Animation.exittoleft);
		//					// push
		//					Fragment currentToHide = _fragmentStack.Last();
		//					transaction.HideEx(currentToHide);
		//					transaction.AddEx(Resource.Id.nav_host, fragment);
		//					transaction.ShowEx(fragment);
		//					_fragmentStack.Add(fragment);
		//				}
		//			}

		//			// We don't currently support fragment restoration, so we don't need to worry about
		//			// whether the commit loses state
		//			transaction.CommitAllowingStateLossEx();

		//			// The fragment transitions don't really SUPPORT telling you when they end
		//			// There are some hacks you can do, but they actually are worse than just doing this:

		//			if (animated)
		//			{
		//				if (!removed)
		//				{
		//					UpdateToolbar();
		//					if (_drawerToggle != null && NavigationPageController.StackDepth == 2 && NavigationPage.GetHasBackButton(page))
		//						AnimateArrowIn();
		//				}
		//				else if (_drawerToggle != null && NavigationPageController.StackDepth == 2 && NavigationPage.GetHasBackButton(page))
		//					AnimateArrowOut();

		//				//AddTransitionTimer(tcs, fragment, FragmentManager, fragmentsToRemove, TransitionDuration, removed);
		//			}
		//			//else
		//			//	AddTransitionTimer(tcs, fragment, FragmentManager, fragmentsToRemove, 1, true);

		//			Context.HideKeyboard(this);

		//			// TODO MAUI
		//			//if (Platform != null)
		//			//{
		//			//	Platform.NavAnimationInProgress = false;
		//			//}

		//			tcs.SetResult(true);
		//			return tcs.Task;
		//		}


		void ToolbarTrackerOnCollectionChanged(object sender, EventArgs eventArgs)
		{
			UpdateMenu();
		}

		void UpdateMenu()
		{
			if (_currentMenuItems == null)
				return;

			_currentMenuItems.Clear();
			_currentMenuItems = new List<IMenuItem>();
			_toolbar.UpdateMenuItems(_toolbarTracker?.ToolbarItems, Element.FindMauiContext(), null, OnToolbarItemPropertyChanged, _currentMenuItems, _currentToolbarItems, UpdateMenuItemIcon);
		}

		protected virtual void OnToolbarItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var items = _toolbarTracker?.ToolbarItems?.ToList();
			_toolbar.OnToolbarItemPropertyChanged(e, (ToolbarItem)sender, items, Element.FindMauiContext(), null, OnToolbarItemPropertyChanged, _currentMenuItems, _currentToolbarItems, UpdateMenuItemIcon);
		}

		protected virtual void UpdateMenuItemIcon(Context context, IMenuItem menuItem, ToolbarItem toolBarItem)
		{
			ToolbarExtensions.UpdateMenuItemIcon(Element.FindMauiContext(), menuItem, toolBarItem, null);
		}

		void UpdateToolbar()
		{
			Context context = Context;
			AToolbar bar = _toolbar;
			ActionBarDrawerToggle toggle = _drawerToggle;

			if (bar == null)
				return;

			bool isNavigated = NavigationPageController.StackDepth > 1;
			bar.NavigationIcon = null;
			Page currentPage = Element.CurrentPage;

			if (isNavigated)
			{
				if (NavigationPage.GetHasBackButton(currentPage))
				{
					if (toggle != null)
					{
						toggle.DrawerIndicatorEnabled = false;
						toggle.SyncState();
					}

					
					var icon = new DrawerArrowDrawable(context.GetThemedContext());
					icon.Progress = 1;
					bar.NavigationIcon = icon;
					var prevPage = Element.Peek(1);
					var backButtonTitle = NavigationPage.GetBackButtonTitle(prevPage);
					_defaultNavigationContentDescription = backButtonTitle != null
						? bar.SetNavigationContentDescription(prevPage, backButtonTitle)
						: bar.SetNavigationContentDescription(prevPage, _defaultNavigationContentDescription);
				}
				else if (toggle != null && _flyoutPage != null)
				{
					toggle.DrawerIndicatorEnabled = _flyoutPage.ShouldShowToolbarButton();
					toggle.SyncState();
				}
			}
			else
			{
				if (toggle != null && _flyoutPage != null)
				{
					toggle.DrawerIndicatorEnabled = _flyoutPage.ShouldShowToolbarButton();
					toggle.SyncState();
				}
			}

			Color tintColor = Element.BarBackgroundColor;

			if (tintColor == null)
				bar.BackgroundTintMode = null;
			else
			{
				bar.BackgroundTintMode = PorterDuff.Mode.Src;
				bar.BackgroundTintList = ColorStateList.ValueOf(tintColor.ToNative());
			}

			Brush barBackground = Element.BarBackground;
			bar.UpdateBackground(barBackground);

			Color textColor = Element.BarTextColor;
			if (textColor != null)
				bar.SetTitleTextColor(textColor.ToNative().ToArgb());

			Color navIconColor = NavigationPage.GetIconColor(Current);
			if (navIconColor != null && bar.NavigationIcon != null)
				DrawableExtensions.SetColorFilter(bar.NavigationIcon, navIconColor, FilterMode.SrcAtop);

			bar.Title = currentPage?.Title ?? string.Empty;

			if (_toolbar.NavigationIcon != null && textColor != null)
			{
				var icon = _toolbar.NavigationIcon as DrawerArrowDrawable;
				if (icon != null)
					icon.Color = textColor.ToNative().ToArgb();
			}

			UpdateTitleIcon();

			UpdateTitleView();
		}

		void UpdateTitleIcon()
		{
			Page currentPage = Element.CurrentPage;

			if (currentPage == null)
				return;

			ImageSource source = NavigationPage.GetTitleIconImageSource(currentPage);

			if (source == null || source.IsEmpty)
			{
				_toolbar.RemoveView(_titleIconView);
				_titleIconView?.Dispose();
				_titleIconView = null;
				_imageSource = null;
				return;
			}

			if (_titleIconView == null)
			{
				_titleIconView = new Android.Widget.ImageView(Context);
				_toolbar.AddView(_titleIconView, 0);
			}

			if (_imageSource != source)
			{
				_imageSource = source;
				_titleIconView.SetImageResource(global::Android.Resource.Color.Transparent);

				ShellImagePart.LoadImage(source, MauiContext, (result) =>
				{
					_titleIconView.SetImageDrawable(result.Value);
					AutomationPropertiesProvider.AccessibilitySettingsChanged(_titleIconView, source);
				});
			}
		}

		void UpdateTitleView()
		{
			AToolbar bar = _toolbar;

			if (bar == null)
				return;

			Page currentPage = Element.CurrentPage;

			if (currentPage == null)
				return;

			VisualElement titleView = NavigationPage.GetTitleView(currentPage);
			if (_titleViewHandler != null)
			{
				var reflectableType = _titleViewHandler as System.Reflection.IReflectableType;
				var rendererType = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : _titleViewHandler.GetType();
				if (titleView == null || Internals.Registrar.Registered.GetHandlerTypeForObject(titleView) != rendererType)
				{
					if (_titleView != null)
						_titleView.Child = null;

					_titleViewHandler.VirtualView.Handler = null;
					_titleViewHandler = null;
				}
			}

			if (titleView == null)
				return;

			if (_titleViewHandler != null)
				_titleViewHandler.SetVirtualView(titleView);
			else
			{
				titleView.ToNative(MauiContext);
				_titleViewHandler = titleView.Handler;

				if (_titleView == null)
				{
					_titleView = new Container(Context);
					bar.AddView(_titleView);
				}

				_titleView.Child = (INativeViewHandler)_titleViewHandler;
			}
		}

		class ClickListener : Object, IOnClickListener
		{
			readonly NavigationPage _element;

			public ClickListener(NavigationPage element)
			{
				_element = element;
			}

			public void OnClick(AView v)
			{
				_element?.PopAsync();
			}
		}

		internal class Container : ViewGroup
		{
			INativeViewHandler _child;

			public Container(Context context) : base(context)
			{
			}

			public INativeViewHandler Child
			{
				set
				{
					if (_child != null)
						RemoveView(_child.NativeView);

					_child = value;

					if (value != null)
						AddView(value.NativeView);
				}
			}

			protected override void OnLayout(bool changed, int l, int t, int r, int b)
			{
				if (_child == null)
					return;

				_child.NativeView.Layout(l, t, r, b);
			}

			protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
			{
				if (_child == null)
				{
					SetMeasuredDimension(0, 0);
					return;
				}

				_child.NativeView.Measure(widthMeasureSpec, heightMeasureSpec);
				SetMeasuredDimension(_child.NativeView.MeasuredWidth, _child.NativeView.MeasuredHeight);
			}
		}

		class DrawerMultiplexedListener : Object, DrawerLayout.IDrawerListener
		{
			public List<DrawerLayout.IDrawerListener> Listeners { get; } = new List<DrawerLayout.IDrawerListener>(2);

			public void OnDrawerClosed(AView drawerView)
			{
				foreach (DrawerLayout.IDrawerListener listener in Listeners)
					listener.OnDrawerClosed(drawerView);
			}

			public void OnDrawerOpened(AView drawerView)
			{
				foreach (DrawerLayout.IDrawerListener listener in Listeners)
					listener.OnDrawerOpened(drawerView);
			}

			public void OnDrawerSlide(AView drawerView, float slideOffset)
			{
				foreach (DrawerLayout.IDrawerListener listener in Listeners)
					listener.OnDrawerSlide(drawerView, slideOffset);
			}

			public void OnDrawerStateChanged(int newState)
			{
				foreach (DrawerLayout.IDrawerListener listener in Listeners)
					listener.OnDrawerStateChanged(newState);
			}
		}
	}
}
