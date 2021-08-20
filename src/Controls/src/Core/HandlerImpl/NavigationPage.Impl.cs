using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	public partial class NavigationPage : INavigationViewInternal
	{
		Thickness IView.Margin => Thickness.Zero;

		partial void Init()
		{
			PushRequested += (_, args) =>
			{
				var request = new MauiNavigationRequestedEventArgs(args.Page, args.Animated);
				Handler?.Invoke(nameof(INavigationViewInternal.PushAsync), request);

			};

			PopRequested += (_, args) =>
			{
				var request = new MauiNavigationRequestedEventArgs(args.Page, args.Animated);
				Handler?.Invoke(nameof(INavigationViewInternal.PopAsync), request);
			};
		}

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			if (Content is IView view)
			{
				view.Measure(widthConstraint, heightConstraint);
			}

			return new Size(widthConstraint, heightConstraint);
		}

		protected override Size ArrangeOverride(Rectangle bounds)
		{
			Frame = this.ComputeFrame(bounds);

			if (Content is IView view)
			{
				_ = view.Arrange(Frame);
			}

			return Frame.Size;
		}

		void INavigationViewInternal.InsertPageBefore(IView page, IView before)
		{
			throw new NotImplementedException();
		}

		Task<IView> INavigationViewInternal.PopAsync() =>
			(this as INavigationViewInternal).PopAsync(true);

		async Task<IView> INavigationViewInternal.PopAsync(bool animated)
		{
			var thing = await this.PopAsync(animated);
			return thing;
		}

		Task<IView> INavigationViewInternal.PopModalAsync()
		{
			throw new NotImplementedException();
		}

		Task<IView> INavigationViewInternal.PopModalAsync(bool animated)
		{
			throw new NotImplementedException();
		}

		Task INavigationViewInternal.PushAsync(IView page) =>
			(this as INavigationViewInternal).PushAsync(page, true);

		Task INavigationViewInternal.PushAsync(IView page, bool animated)
		{
			return this.PushAsync((Page)page, animated);
		}

		Task INavigationViewInternal.PushModalAsync(IView page)
		{
			throw new NotImplementedException();
		}

		Task INavigationViewInternal.PushModalAsync(IView page, bool animated)
		{
			throw new NotImplementedException();
		}

		void INavigationViewInternal.RemovePage(IView page)
		{
			throw new NotImplementedException();
		}

		IView Content =>
			this.CurrentPage;

		IReadOnlyList<IView> INavigationViewInternal.ModalStack => throw new NotImplementedException();

		IReadOnlyList<IView> INavigationViewInternal.NavigationStack =>
			this.Navigation.NavigationStack;
	}

}
