using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	public class MauiNavigationRequestedEventArgs : EventArgs
	{
		readonly IList<IView> _newNavigationStack;

		public MauiNavigationRequestedEventArgs(IList<IView> newNavigationStack, bool animated)
		{
			_newNavigationStack = newNavigationStack;
			Animated = animated;
		}

		public bool Animated { get; set; }

		public Task<bool>? Task { get; set; }
	}
}