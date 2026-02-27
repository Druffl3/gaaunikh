using Microsoft.AspNetCore.Http;

namespace Gaaunikh.Api.Tests;

public sealed class FrontendStaticPageResolverTests
{
    [Fact]
    public void Resolve_ReturnsNestedIndexFile_ForShopRouteWithoutTrailingSlash()
    {
        var rootPath = CreateTempWebRoot();
        var shopDirectory = Path.Combine(rootPath, "shop");
        Directory.CreateDirectory(shopDirectory);
        File.WriteAllText(Path.Combine(shopDirectory, "index.html"), "<html>shop</html>");

        var resolvedPath = FrontendStaticPageResolver.Resolve(rootPath, new PathString("/shop"));

        Assert.NotNull(resolvedPath);
        Assert.EndsWith(Path.Combine("shop", "index.html"), resolvedPath, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Resolve_ReturnsNull_WhenNoMatchingFileExists()
    {
        var rootPath = CreateTempWebRoot();

        var resolvedPath = FrontendStaticPageResolver.Resolve(rootPath, new PathString("/non-existent"));

        Assert.Null(resolvedPath);
    }

    private static string CreateTempWebRoot()
    {
        var rootPath = Path.Combine(Path.GetTempPath(), $"gaaunikh-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootPath);
        return rootPath;
    }
}
