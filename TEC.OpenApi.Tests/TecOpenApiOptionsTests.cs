using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using TEC.OpenApi;

namespace TEC.OpenApi.Tests;

public class TecOpenApiOptionsTests
{
    [Test]
    public async Task Default_options_have_expected_values()
    {
        var options = new TecOpenApiOptions();

        await Assert.That(options.Title).IsEqualTo("API Tudo em Código");
        await Assert.That(options.Version).IsEqualTo("v1");
        await Assert.That(options.DocumentName).IsEqualTo("v1");
        await Assert.That(options.ScalarRoutePrefix).IsEqualTo("docs");
        await Assert.That(options.EnableJwtBearer).IsTrue();
        await Assert.That(options.JwtBearerSchemeName).IsEqualTo("Bearer");
        await Assert.That(options.ScalarTheme).IsEqualTo(ScalarTheme.Mars);
        await Assert.That(options.ContactName).IsEqualTo("Suporte Tudo em Código");
        await Assert.That(options.LicenseName).IsEqualTo("Apache 2.0");
    }

    [Test]
    public async Task AddTecOpenApi_registers_configured_options_in_di()
    {
        var services = new ServiceCollection();
        services.AddTecOpenApi(o =>
        {
            o.Title = "Minha API Corporativa";
            o.Version = "v2";
            o.DocumentName = "v2";
            o.EnableJwtBearer = false;
            o.ScalarTheme = ScalarTheme.Moon;
        });

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<TecOpenApiOptions>>().Value;

        await Assert.That(options.Title).IsEqualTo("Minha API Corporativa");
        await Assert.That(options.Version).IsEqualTo("v2");
        await Assert.That(options.DocumentName).IsEqualTo("v2");
        await Assert.That(options.EnableJwtBearer).IsFalse();
        await Assert.That(options.ScalarTheme).IsEqualTo(ScalarTheme.Moon);
    }

    [Test]
    public async Task AddTecOpenApi_with_null_services_throws_ArgumentNullException()
    {
        IServiceCollection? services = null;
        Assert.Throws<ArgumentNullException>(() => services!.AddTecOpenApi());
        await Task.CompletedTask;
    }
}
