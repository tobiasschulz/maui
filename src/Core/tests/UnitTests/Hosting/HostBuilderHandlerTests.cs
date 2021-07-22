using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.UnitTests.Hosting
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class HostBuilderHandlerTests
	{
		[Fact]
		public void CanBuildAHost()
		{
			var services = MauiAppBuilder.CreateBuilder()
				.Build();

			Assert.NotNull(services);
		}

		[Fact]
		public void CanGetIMauiHandlersServiceProviderFromServices()
		{
			var services = MauiAppBuilder.CreateBuilder()
				.Build();

			Assert.NotNull(services);
			var handlers = services.GetRequiredService<IMauiHandlersServiceProvider>();
			Assert.NotNull(handlers);
			Assert.IsType<Maui.Hosting.Internal.MauiHandlersServiceProvider>(handlers);
		}

		[Fact]
		public void CanRegisterAndGetHandlerUsingType()
		{
			var services = MauiAppBuilder.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<IViewStub, ViewHandlerStub>())
				.Build();

			var handler = services.GetRequiredService<IMauiHandlersServiceProvider>().GetHandler(typeof(IViewStub));

			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void CanRegisterAndGetHandler()
		{
			var services = MauiAppBuilder.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<IViewStub, ViewHandlerStub>())
				.Build();

			var handler = services.GetRequiredService<IMauiHandlersServiceProvider>().GetHandler<IViewStub>();

			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void CanRegisterAndGetHandlerWithType()
		{
			var services = MauiAppBuilder.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler(typeof(IViewStub), typeof(ViewHandlerStub)))
				.Build();

			var handler = services.GetRequiredService<IMauiHandlersServiceProvider>().GetHandler(typeof(IViewStub));

			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void CanRegisterAndGetHandlerWithDictionary()
		{
			var dic = new Dictionary<Type, Type>
			{
				{ typeof(IViewStub), typeof(ViewHandlerStub) }
			};

			var services = MauiAppBuilder.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandlers(dic))
				.Build();

			var handler = services.GetRequiredService<IMauiHandlersServiceProvider>().GetHandler(typeof(IViewStub));

			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void CanRegisterAndGetHandlerForConcreteType()
		{
			var services = MauiAppBuilder.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<IViewStub, ViewHandlerStub>())
				.Build();

			var handler = services.GetRequiredService<IMauiHandlersServiceProvider>().GetHandler(typeof(ViewStub));

			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void CanChangeHandlerRegistration()
		{
			var services = MauiAppBuilder.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ButtonStub, ButtonHandlerStub>())
				.Build();

			var specificHandler = services.GetRequiredService<IMauiHandlersServiceProvider>().GetHandler(typeof(ButtonStub));
			Assert.IsType<ButtonHandlerStub>(specificHandler);

			services.GetRequiredService<IMauiHandlersServiceProvider>().GetCollection().AddHandler<ButtonStub, AlternateButtonHandlerStub>();

			var alternateHandler = services.GetRequiredService<IMauiHandlersServiceProvider>().GetHandler(typeof(ButtonStub));
			Assert.IsType<AlternateButtonHandlerStub>(alternateHandler);
		}
	}
}