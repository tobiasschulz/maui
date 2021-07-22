using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.UnitTests.Hosting
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class HostBuilderImageSourceTests
	{
		[Theory]
		[InlineData(typeof(IFileImageSource))]
		[InlineData(typeof(FileImageSourceStub))]
		public void CanRetrieveFileUsingInterfaceImageSource(Type type)
		{
			var builder = MauiAppBuilder
				.CreateBuilder()
				.ConfigureImageSources();
			var services = builder.Build();

			var images = services.GetRequiredService<IImageSourceServiceProvider>();
			Assert.NotNull(images);

			var imageSourceService = images.GetRequiredImageSourceService(type);
			Assert.NotNull(imageSourceService);
			Assert.IsType<FileImageSourceService>(imageSourceService);
		}

		[Fact]
		public void CanRetrieveFontUsingInterfaceImageSource()
		{
			var builder = MauiAppBuilder
				.CreateBuilder()
				.ConfigureFonts()
				.ConfigureImageSources();
			var services = builder.Build();

			var manager = services.GetRequiredService<IFontManager>();
			Assert.NotNull(manager);

			var images = services.GetRequiredService<IImageSourceServiceProvider>();
			Assert.NotNull(images);

			var imageSourceService = images.GetRequiredImageSourceService<IFontImageSource>();
			Assert.NotNull(imageSourceService);
			var fontService = Assert.IsType<FontImageSourceService>(imageSourceService);

			Assert.Equal(manager, fontService.FontManager);
		}

		[Fact]
		public void CanRetrieveFontUsingConcreteImageSource()
		{
			var builder = MauiAppBuilder
				.CreateBuilder()
				.ConfigureFonts()
				.ConfigureImageSources();
			var services = builder.Build();

			var manager = services.GetRequiredService<IFontManager>();
			Assert.NotNull(manager);

			var images = services.GetRequiredService<IImageSourceServiceProvider>();
			Assert.NotNull(images);

			var imageSourceService = images.GetRequiredImageSourceService<FontImageSourceStub>();
			Assert.NotNull(imageSourceService);
			var fontService = Assert.IsType<FontImageSourceService>(imageSourceService);

			Assert.Equal(manager, fontService.FontManager);
		}
	}
}