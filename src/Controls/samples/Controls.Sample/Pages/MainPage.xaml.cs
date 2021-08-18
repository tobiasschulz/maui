using System;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class MainPage
	{
		public MainPage(IServiceProvider services, MainViewModel viewModel)
		{
			InitializeComponent();

			BindingContext = viewModel;
		}

		private void OnToolbarItemClicked(object sender, EventArgs e)
		{
			if (FlowDirection != FlowDirection.RightToLeft)
				FlowDirection = FlowDirection.RightToLeft;
			else
				FlowDirection = FlowDirection.LeftToRight;

			XamlApp.GlobalFlowDirection = FlowDirection;
		}
	}
}