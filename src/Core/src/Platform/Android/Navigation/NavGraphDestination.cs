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
			// If they've removed all the pages except one then we process the navigation
			// to update the app bar
			if (pages[pages.Count - 1] == NavigationStack[NavigationStack.Count - 1] &&
				pages.Count > 1 &&
				NavigationStack.Count > 1)
			{
				NavigationStack = new List<IView>(pages);
				return;
			}

			NavOptions? navOptions = null;
			

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
			bool isPop = pages.Count < NavigationStack.Count;

			if (fragmentNavDestinations.Count < pages.Count)
			{
				if (animated)
				{
					// natively we're doing the opposite
					if (isPop)
					{
						navOptions = new NavOptions.Builder()
							 .SetPopEnterAnim(Resource.Animation.enterfromright)
							 .SetPopExitAnim(Resource.Animation.exittoleft)
							 .SetEnterAnim(Resource.Animation.enterfromleft)
							 .SetExitAnim(Resource.Animation.exittoright)
							 .Build();
					}
					else
					{
						navOptions = new NavOptions.Builder()
								.SetEnterAnim(Resource.Animation.enterfromright)
								.SetExitAnim(Resource.Animation.exittoleft)
								.SetPopEnterAnim(Resource.Animation.enterfromleft)
								.SetPopExitAnim(Resource.Animation.exittoright)
								.Build();
					}
				}

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
							navController.Navigate(dest.Id);
						}
						else
						{
							navController.Navigate(dest.Id, null, navOptions);
						}
					}
				}
			}
			// user is popping to root
			else if (pages.Count == 1)
			{
				// TODO MAUI work with cleaning up fragments before actually firing navigation
				Pages.Add(pages[0], fragmentNavDestinations[0].Id);
				fragmentNavDestinations[0].Page = pages[0];

				//navOptions = new NavOptions.Builder()
				//				.SetEnterAnim(Resource.Animation.enterfromright)
				//				.SetExitAnim(Resource.Animation.exittoleft)
				//				.SetPopEnterAnim(Resource.Animation.enterfromleft)
				//				.SetPopExitAnim(Resource.Animation.exittoright)
				//				.SetPopUpTo(fragmentNavDestinations[0].Id, false)
				//				.Build();

				navController.PopBackStack(fragmentNavDestinations[0].Id, false);
				// navController.Navigate(fragmentNavDestinations[0].Id, null, navOptions);
			}
			else if (pages[pages.Count - 1] == NavigationStack[NavigationStack.Count - 1])
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
			}
			else if (pages.Count == fragmentNavDestinations.Count)
			{
				int popToId = fragmentNavDestinations[fragmentNavDestinations.Count - 2].Id;
				for (int i = 0; i < pages.Count; i++)
				{
					Pages.Add(pages[i], fragmentNavDestinations[i].Id);

					if (fragmentNavDestinations[i].Page != pages[i])
						fragmentNavDestinations[i].Page = pages[i];
				}

				// last page on the stack
				var lastPage = pages.Last();

				// last fragment on the stack
				var lastFrag = fragmentNavDestinations.Last();

				if (lastFrag.Page != lastPage)
					lastFrag.Page = lastPage;

				Console.Write($"lastFrag ID: {lastFrag.Id}");
				Console.Write($"PopToID: {popToId}");
				Console.WriteLine($"{navController.PopBackStack(popToId, false)}");
				navController.Navigate(lastFrag.Id);
			}
			else
			{
				int popToId = 0;
				for (int i = 0; i < pages.Count; i++)
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
				navOptions = new NavOptions.Builder()
								.SetEnterAnim(Resource.Animation.enterfromright)
								.SetExitAnim(Resource.Animation.exittoleft)
								.SetPopEnterAnim(Resource.Animation.enterfromleft)
								.SetPopExitAnim(Resource.Animation.exittoright)
								.SetPopUpTo(popToId, false)
								.Build();

				Console.Write($"PopToID: {popToId}");
				//Console.WriteLine($"{navController.PopBackStack(popToId, false)}");
				navController.Navigate(popToId, null, navOptions);
			}


			foreach (var thing in fragmentNavDestinations)
			{
				if (!Pages.Values.ToList().Contains(thing.Id))
				{
					this.Remove(thing);
					Console.WriteLine($"Removing Destination {(thing.Page as ITitledElement)?.Title}");
				}
			}

			NavigationStack = new List<IView>(pages);
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
