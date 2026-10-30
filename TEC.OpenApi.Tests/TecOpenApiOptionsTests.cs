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
        // Contato e licença só entram no documento se o consumidor definir
        await Assert.That(options.ContactName).IsNull();
        await Assert.That(options.ContactEmail).IsNull();
        await Assert.That(options.LicenseName).IsNull();
        await Assert.That(options.LicenseUrl).IsNull();
        // Fechado por padrão: fora de Development nada é mapeado sem opt-in
        await Assert.That(options.ExposeOutsideDevelopment).IsFalse();
        await Assert.That(options.RequireDocumentationAuthorization).IsFalse();
        await Assert.That(options.DocumentationAuthorizationPolicy).IsNull();
    }

    [Test]
    [Arguments("http://auth.example.com/realms/x/.well-known/openid-configuration")]
    [Arguments("ftp://auth.example.com/x")]
    [Arguments("nao-e-url")]
    [Arguments("/realms/x/.well-known/openid-configuration")]
    [Arguments("https://usuario:senha@auth.example.com/realms/x/.well-known/openid-configuration")]
    public async Task OpenIdConnectUrl_invalida_derruba_a_subida(string url)
    {
        var services = new ServiceCollection();
        var error = Assert.Throws<InvalidOperationException>(() => services.AddTecOpenApi(o => o.OpenIdConnectUrl = url));

        // A mensagem não ecoa o valor (pode carregar credencial)
        await Assert.That(error.Message.Contains(url, StringComparison.Ordinal)).IsFalse();
    }

    [Test]
    public async Task OpenIdConnectUrl_https_absoluta_e_aceita()
    {
        var services = new ServiceCollection();
        services.AddTecOpenApi(o => o.OpenIdConnectUrl = "https://auth.example.com/realms/x/.well-known/openid-configuration");

        var options = services.BuildServiceProvider().GetRequiredService<TecOpenApiOptions>();
        await Assert.That(options.OpenIdConnectUrl).IsNotNull();
    }

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    public async Task DocumentName_vazio_derruba_a_subida(string nome)
    {
        var services = new ServiceCollection();
        Assert.Throws<InvalidOperationException>(() => services.AddTecOpenApi(o => o.DocumentName = nome));
        await Task.CompletedTask;
    }

    [Test]
    public async Task Politica_de_autorizacao_so_com_espacos_derruba_a_subida()
    {
        var services = new ServiceCollection();
        Assert.Throws<InvalidOperationException>(() => services.AddTecOpenApi(o => o.DocumentationAuthorizationPolicy = "  "));
        await Task.CompletedTask;
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
