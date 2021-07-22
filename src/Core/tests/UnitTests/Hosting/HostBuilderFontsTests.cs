using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.UnitTests.Hosting
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class HostBuilderFontsTests
	{
		[Fact]
		public void ConfigureFontsRegistersTheCorrectServices()
		{
			var builder = MauiAppBuilder
				.CreateBuilder()
				.ConfigureFonts();
			var services = builder.Build();

			var manager = services.GetRequiredService<IFontManager>();
			Assert.NotNull(manager);

			var registrar = services.GetRequiredService<IFontRegistrar>();
			Assert.NotNull(registrar);

			var loader = services.GetRequiredService<IEmbeddedFontLoader>();
			Assert.NotNull(loader);
		}

		[Theory]
		[InlineData("Dokdo-Regular.ttf", "Dokdo")]
		[InlineData("Dokdo-Regular.ttf", null)]
		public void ConfigureFontsRegistersFonts(string filename, string alias)
		{
			var root = Path.Combine(Path.GetTempPath(), "Microsoft.Maui.UnitTests", "ConfigureFontsRegistersFonts", Guid.NewGuid().ToString());

			var builder = MauiAppBuilder
				.CreateBuilder()
				.ConfigureFonts(fonts => fonts.AddEmeddedResourceFont(GetType().Assembly, filename, alias));
			builder.Services.AddSingleton<IEmbeddedFontLoader>(_ => new FileSystemEmbeddedFontLoader(root));
			var services = builder.Build();

			var registrar = services.GetRequiredService<IFontRegistrar>();

			var path = registrar.GetFont(filename);
			Assert.NotNull(path);
			Assert.StartsWith(root, path);

			if (alias != null)
			{
				path = registrar.GetFont(alias);
				Assert.NotNull(path);
				Assert.StartsWith(root, path);
			}

			Assert.True(File.Exists(Path.Combine(root, filename)));

			Directory.Delete(root, true);
		}

		[Fact]
		public void NullAssemblyForEmbeddedFontThrows()
		{
			var builder = MauiAppBuilder
				.CreateBuilder()
				.ConfigureFonts(fonts => fonts.AddEmeddedResourceFont(null, "test.ttf"));

			var ex = Assert.Throws<ArgumentNullException>(() => builder.Build());
			Assert.Equal("assembly", ex.ParamName);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		public void BadFileNameForEmbeddedFontThrows(string filename)
		{
			var builder = MauiAppBuilder
				.CreateBuilder()
				.ConfigureFonts(fonts => fonts.AddEmeddedResourceFont(GetType().Assembly, filename));

			var ex = Assert.ThrowsAny<ArgumentException>(() => builder.Build());
			Assert.Equal("filename", ex.ParamName);

			if (filename == null)
				Assert.IsType<ArgumentNullException>(ex);
		}

		[Fact]
		public void NullAliasForEmbeddedFontDoesNotThrow()
		{
			var builder = MauiAppBuilder
				.CreateBuilder()
				.ConfigureFonts(fonts => fonts.AddEmeddedResourceFont(GetType().Assembly, "test.ttf", null));

			_ = builder.Build();
		}
	}
}