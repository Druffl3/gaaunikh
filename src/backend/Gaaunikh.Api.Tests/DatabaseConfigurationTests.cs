using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Gaaunikh.Api.Configuration;
using Gaaunikh.Api.Data;

namespace Gaaunikh.Api.Tests;

public sealed class DatabaseConfigurationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DatabaseConfigurationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ApplicationBoots_WithCommerceDatabaseConfigurationPresent()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/health");

        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var dbContext = scope.ServiceProvider.GetRequiredService<CommerceDbContext>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(dbContext);
        Assert.Equal(
            "Host=postgres;Port=5432;Database=gaaunikh;Username=gaaunikh;Password=gaaunikh",
            configuration.GetConnectionString("CommerceDatabase"));
    }

    [Fact]
    public void CommerceProviderOptions_BindSuccessfully()
    {
        using var scope = _factory.Services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<CommerceOptions>>().Value;

        Assert.Equal("Razorpay", options.Payments.Provider);
        Assert.Equal("Shiprocket", options.Shipping.Provider);
        Assert.Equal("Resend", options.Notifications.Email.Provider);
        Assert.Equal("Twilio", options.Notifications.WhatsApp.Provider);
        Assert.Equal(string.Empty, options.Payments.Razorpay.KeyId);
        Assert.Equal(string.Empty, options.Shipping.Shiprocket.Email);
        Assert.Equal(string.Empty, options.Notifications.Email.Resend.ApiKey);
        Assert.Equal(string.Empty, options.Notifications.WhatsApp.Twilio.AccountSid);
    }
}
