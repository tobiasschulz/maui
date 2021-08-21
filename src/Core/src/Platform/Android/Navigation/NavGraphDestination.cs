using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndroidX.Navigation;
using AndroidX.Navigation.Fragment;
using AndroidX.Navigation.UI;

namespace Microsoft.Maui
{

	// TODO MAUI MAKE PRIVATE or make it proxy and probably rename
	public class NavGraphDestination : NavGraph
	{
		public IReadOnlyList<IView> NavigationStack { get; private set; } = new List<IView>();

		public Dictionary<IView, int> Pages = new Dictionary<IView, int>();

		public NavGraphDestination(Navigator navGraphNavigator) : base(navGraphNavigator)
		{
			Id = global::Android.Views.View.GenerateViewId();
		}



		// all of this weirdness is because AFAICT you can't remove things from the navigation stack
		public void ReShuffleDestinations(
			IReadOnlyList<IView> pages,
			bool animated,
			NavigationLayout navigationLayout)
		{
			var navController = navigationLayout.NavHost.NavController;

			// this means the currently visible page hasn't changed so don't do anything
			// TODO MAUI test remove page on root
			//if (pages[pages.Count - 1] == NavigationStack[NavigationStack.Count - 1])
			//{
			//	NavigationStack = new List<IView>(pages);
			//	return;
			//}

			NavOptions? navOptions = null;
			if (animated)
			{
				navOptions = new NavOptions.Builder()
					 .SetEnterAnim(Resource.Animation.enterfromright)
					 .SetExitAnim(Resource.Animation.exittoleft)
					 .SetPopEnterAnim(Resource.Animation.enterfromleft)
					 .SetPopExitAnim(Resource.Animation.exittoright)
					 .Build();
			}

			var iterator = navigationLayout.NavHost.NavController.BackStack.Iterator();
			var fragmentNavDestinations = new List<FragmentNavDestination>();
			var bsEntry = new List<NavBackStackEntry>();

			Console.WriteLine($"Output Stack start");
			while (iterator.HasNext)
			{
				if (iterator.Next() is NavBackStackEntry nbse &&
					nbse.Destination is FragmentNavDestination nvd)
				{
					Console.WriteLine($"In Stack {(nvd.Page as ITitledElement)?.Title}");
					fragmentNavDestinations.Add(nvd);
					bsEntry.Add(nbse);
				}
			}
			Console.WriteLine($"Output Stack end");

			Pages.Clear();
			if (pages.Count > NavigationStack.Count)
			{
				for (int i = 0; i < pages.Count; i++)
				{
					// TODO cleanup into method
					if (fragmentNavDestinations.Count > i)
					{
						Pages.Add(pages[i], fragmentNavDestinations[i].Id);
						fragmentNavDestinations[i].Page = pages[i];
					}
					else
					{
						var dest = AddDestination(pages[i], navigationLayout);

						if (i < (pages.Count - 1))
						{
							//NavAction navAction = new NavAction(dest.Id, navOptions);
							//dest.PutAction(global::Android.Views.View.GenerateViewId(), navAction);
							navController.Navigate(dest.Id);
						}
						else
						{
							navController.Navigate(dest.Id, null, navOptions);
						}
					}
				}
			}
			else
			{
				if (pages.Count > 0 &&
					pages[pages.Count - 1] == NavigationStack[NavigationStack.Count - 1])
				{
					int popToId = 0;
					for (int i = 0; i < pages.Count - 1; i++)
					{
						Pages.Add(pages[i], fragmentNavDestinations[i].Id);

						if (fragmentNavDestinations[i].Page != pages[i])
							fragmentNavDestinations[i].Page = pages[i];

						popToId = fragmentNavDestinations[i].Id;
					}

					// last page on the stack
					var lastPage = pages.Last();

					// last fragment on the stack
					var lastFrag = fragmentNavDestinations.Last();


					Pages.Add(lastPage, lastFrag.Id);

					if (lastFrag.Page != lastPage)
						lastFrag.Page = lastPage;

					Console.Write($"lastFrag ID: {lastFrag.Id}");

					bool inclusive = false;
					if (popToId == 0)
					{
						popToId = fragmentNavDestinations[0].Id;
						Console.Write($"PopToID: {popToId}");
						navOptions = new NavOptions.Builder()
							 //.SetEnterAnim(Resource.Animation.enterfromright)
							 //.SetExitAnim(Resource.Animation.exittoleft)
							 //.SetPopEnterAnim(Resource.Animation.enterfromleft)
							 //.SetPopExitAnim(Resource.Animation.exittoright)
							 .SetPopUpTo(popToId, true)
							 .Build();

						this.StartDestination = lastFrag.Id;
						var actionId = Android.Views.View.GenerateViewId();
						lastFrag.PutAction(actionId, new NavAction(lastFrag.Id, navOptions));
						Console.WriteLine($"Inclusive Push to {(lastFrag.Page as ITitledElement)?.Title}");
						navController.Navigate(actionId);
					}
					else
					{
						Console.Write($"PopToID: {popToId}");
						Console.WriteLine($"{navController.PopBackStack(popToId, inclusive)}");
						navController.Navigate(lastFrag.Id);
					}

					


					foreach(var thing in fragmentNavDestinations)
					{
						if (!Pages.Values.ToList().Contains(thing.Id))
						{
							this.Remove(thing);

							Console.WriteLine($"Removing Destination {(thing.Page as ITitledElement)?.Title}");
						}
					}
				}
				else
				{
					navController.PopBackStack();
				}
			}

			//for (var i = NativeNavigationStackCount; i < pages.Count; i++)
			//{
			//	var dest = destinations[i];
			//	navController.Navigate(dest);
			//}

			NavigationStack = new List<IView>(pages);

			//Pages.Clear();

			//// push operation / right now we interpret matching stack counts as a push
			//// at some later point users can customize animations and adjust it themselves
			//if (pages.Count >= NavigationStack.Count)
			//{
			//	// We just make what we can match up
			//	int pageCount = pages.Count;
			//	for (int i = fragmentNavDestinations.Count - 1; i >= 0 && pageCount >= 0; i--)
			//	{
			//		// TODO cleanup into method
			//		Pages.Add(pages[pageCount], fragmentNavDestinations[i].Id);
			//		fragmentNavDestinations[i].Page = pages[pageCount];
			//	}

			//	NavigationStack = new List<IView>(pages);

			//	var destination =
			//		AddDestination(NavigationStack.Last(), navigationLayout);

			//	navigationLayout.NavHost.NavController.Navigate(destination.Id, null, navOptions);
			//}
			//else
			//{
			//	int pageCount = pages.Count;
			//	for (int i = fragmentNavDestinations.Count - 2; i >= 0 && pageCount >= 0; i--)
			//	{
			//		// TODO cleanup into method
			//		Pages.Add(pages[pageCount], fragmentNavDestinations[i].Id);
			//		fragmentNavDestinations[i].Page = pages[pageCount];
			//	}
			//}


			//// TODO MORE COMMENTS
			////Pages.Add(pages[0], fragmentNavDestinations[0].Id);

			//if (fragmentNavDestinations.Count > pages.Count)
			//{

			//}

			// ensure that the root of the stack has an element
		}

		public FragmentNavDestination AddDestination(
			IView page,
			NavigationLayout navigationLayout)
		{
			var destination = new FragmentNavDestination(page, navigationLayout, this);
			AddDestination(destination);
			return destination;
		}

		internal List<int> ApplyPagesToGraph(
			IReadOnlyList<IView> pages,
			NavigationLayout navigationLayout)
		{
			var navController = navigationLayout.NavHost.NavController;

			// We are subtracting one because the navgraph itself is the first item on the stack
			int NativeNavigationStackCount = navController.BackStack.Size() - 1;

			// set this to one because when the graph is first attached to the controller
			// it will add the graph and the first destination
			if (NativeNavigationStackCount < 0)
				NativeNavigationStackCount = 1;

			List<int> destinations = new List<int>();

			NavDestination navDestination;

			foreach (var page in pages)
			{
				navDestination =
						AddDestination(
							page,
							navigationLayout);

				destinations.Add(navDestination.Id);
			}

			StartDestination = destinations[0];
			navController.SetGraph(this, null);

			for (var i = NativeNavigationStackCount; i < pages.Count; i++)
			{
				var dest = destinations[i];
				navController.Navigate(dest);
			}

			NavigationStack = new List<IView>(pages);
			return destinations;
		}
	}
}
