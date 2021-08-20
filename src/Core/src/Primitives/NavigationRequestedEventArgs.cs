using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	public class MauiNavigationEventArgs : EventArgs
	{
		public MauiNavigationEventArgs(IView page)
		{
			if (page == null)
				throw new ArgumentNullException("page");

			Page = page;
		}

		public IView Page { get; }
	}

	public class MauiNavigationRequestedEventArgs : MauiNavigationEventArgs
	{
		public MauiNavigationRequestedEventArgs(IView page, bool animated) : base(page)
		{
			Animated = animated;
		}

		public bool Animated { get; set; }

		public Task<bool>? Task { get; set; }
	}
}