using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TEC.OpenApi;

namespace TEC.OpenApi.Tests;

public class TecOpenApiIntegrationTests
{
    private const string UserHeader = "X-Test-User";
    private const string RoleHeader = "X-Test-Role";

    /// <summary>Autentica quando há <c>X-Test-User</c>; o papel vem de <c>X-Test-Role</c>. Sem header, desafio responde 401.</summary>
    private sealed class HeaderAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(UserHeader, out var user))
                return Task.FromResult(AuthenticateResult.NoResult());

            var claims = new List<Claim> { new("sub", user.ToString()) };
            if (Request.Headers.TryGetValue(RoleHeader, out var role))
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
        }
    }

    private static async Task<WebApplication> StartAsync(
        Action<TecOpenApiOptions>? configure = null,
        string environment = "Development",
        Func<IEndpointRouteBuilder, IEndpointConventionBuilder>? map = null)
    {
        var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions { EnvironmentName = environment });
        builder.WebHost.UseTestServer();
        builder.Services.AddLogging();
        builder.Services.AddRouting();
        builder.Services.AddAuthentication("Teste").AddScheme<AuthenticationSchemeOptions, HeaderAuthHandler>("Teste", _ => { });
        builder.Services.AddAuthorization(o => o.AddPolicy("docs", p => p.RequireRole("documentacao")));
        builder.Services.AddTecOpenApi(o =>
        {
            o.Title = "Integration Test API";
            configure?.Invoke(o);
        });

        var app = builder.Build();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet("/protegido", () => "ok").RequireAuthorization();
        app.MapGet("/publico", () => "ok");
        var conventions = (map ?? (e => e.MapTecApiDocs()))(app);
        await Assert.That(conventions).IsNotNull();

        await app.StartAsync();
        return app;
    }

    private static async Task<HttpStatusCode> StatusAsync(HttpClient client, string path, string? user = null, string? role = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (user is not null) request.Headers.Add(UserHeader, user);
        if (role is not null) request.Headers.Add(RoleHeader, role);
        using var response = await client.SendAsync(request);
        return response.StatusCode;
    }

    [Test]
    public async Task Documento_real_tem_metadados_esquema_bearer_e_requisito_nas_rotas_protegidas()
    {
        await using var app = await StartAsync();
        var client = app.GetTestClient();

        using var response = await client.GetAsync("/openapi/v1.json");
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;

        var info = root.GetProperty("info");
        await Assert.That(info.GetProperty("title").GetString()).IsEqualTo("Integration Test API");
        // Sem contato nem licença por padrão: nada da Tudo em Código entra no documento do consumidor
        await Assert.That(info.TryGetProperty("contact", out _)).IsFalse();
        await Assert.That(info.TryGetProperty("license", out _)).IsFalse();

        var bearer = root.GetProperty("components").GetProperty("securitySchemes").GetProperty("Bearer");
        await Assert.That(bearer.GetProperty("scheme").GetString()).IsEqualTo("bearer");

        // O requisito precisa citar o esquema; "security": [{}] seria um documento que não pede token
        var protegido = root.GetProperty("paths").GetProperty("/protegido").GetProperty("get");
        var requisito = protegido.GetProperty("security")[0];
        await Assert.That(requisito.TryGetProperty("Bearer", out _)).IsTrue();
        await Assert.That(protegido.GetProperty("responses").TryGetProperty("401", out _)).IsTrue();

        var publico = root.GetProperty("paths").GetProperty("/publico").GetProperty("get");
        await Assert.That(publico.TryGetProperty("security", out _)).IsFalse();
    }

    [Test]
    public async Task Scalar_aponta_para_o_documento_registrado()
    {
        await using var app = await StartAsync();
        using var response = await app.GetTestClient().GetAsync("/docs/v1");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(response.Content.Headers.ContentType?.MediaType).IsEqualTo("text/html");
        await Assert.That(await response.Content.ReadAsStringAsync()).Contains("openapi/v1.json");
    }

    [Test]
    public async Task DocumentName_customizado_e_servido_e_usado_pelo_Scalar()
    {
        await using var app = await StartAsync(o => o.DocumentName = "interno");
        var client = app.GetTestClient();

        using var documento = await client.GetAsync("/openapi/interno.json");
        await Assert.That(documento.StatusCode).IsEqualTo(HttpStatusCode.OK);
        using var json = JsonDocument.Parse(await documento.Content.ReadAsStringAsync());
        await Assert.That(json.RootElement.GetProperty("info").GetProperty("title").GetString()).IsEqualTo("Integration Test API");

        await Assert.That(await StatusAsync(client, "/openapi/v1.json")).IsEqualTo(HttpStatusCode.NotFound);

        using var scalar = await client.GetAsync("/docs/interno");
        var html = await scalar.Content.ReadAsStringAsync();
        await Assert.That(html).Contains("openapi/interno.json");
        await Assert.That(html.Contains("openapi/v1.json", StringComparison.Ordinal)).IsFalse();
    }

    [Test]
    public async Task Contato_e_licenca_entram_quando_o_consumidor_define()
    {
        await using var app = await StartAsync(o =>
        {
            o.ContactName = "Time Vendas";
            o.ContactEmail = "vendas@example.com";
            o.LicenseName = "MIT";
        });
        using var response = await app.GetTestClient().GetAsync("/openapi/v1.json");
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var info = json.RootElement.GetProperty("info");

        await Assert.That(info.GetProperty("contact").GetProperty("email").GetString()).IsEqualTo("vendas@example.com");
        await Assert.That(info.GetProperty("license").GetProperty("name").GetString()).IsEqualTo("MIT");
    }

    [Test]
    public async Task Documentacao_protegida_responde_401_sem_credencial_e_200_com_credencial()
    {
        await using var app = await StartAsync(o => o.RequireDocumentationAuthorization = true);
        var client = app.GetTestClient();

        await Assert.That(await StatusAsync(client, "/openapi/v1.json")).IsEqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(await StatusAsync(client, "/docs/v1")).IsEqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(await StatusAsync(client, "/openapi/v1.json", user: "alice")).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(await StatusAsync(client, "/docs/v1", user: "alice")).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Politica_nomeada_recusa_autenticado_sem_permissao()
    {
        await using var app = await StartAsync(o => o.DocumentationAuthorizationPolicy = "docs");
        var client = app.GetTestClient();

        await Assert.That(await StatusAsync(client, "/openapi/v1.json")).IsEqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(await StatusAsync(client, "/openapi/v1.json", user: "alice")).IsEqualTo(HttpStatusCode.Forbidden);
        await Assert.That(await StatusAsync(client, "/docs/v1", user: "alice")).IsEqualTo(HttpStatusCode.Forbidden);
        await Assert.That(await StatusAsync(client, "/openapi/v1.json", user: "alice", role: "documentacao")).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Retorno_de_MapTecApiDocs_aceita_RequireAuthorization_nos_dois_endpoints()
    {
        await using var app = await StartAsync(map: e => e.MapTecApiDocs().RequireAuthorization());
        var client = app.GetTestClient();

        await Assert.That(await StatusAsync(client, "/openapi/v1.json")).IsEqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(await StatusAsync(client, "/docs/v1")).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Retorno_de_MapTecOpenApi_e_MapTecScalar_aceita_convencoes_separadas()
    {
        await using var app = await StartAsync(map: e =>
        {
            e.MapTecOpenApi().RequireAuthorization();
            return e.MapTecScalar();
        });
        var client = app.GetTestClient();

        await Assert.That(await StatusAsync(client, "/openapi/v1.json")).IsEqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(await StatusAsync(client, "/docs/v1")).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    [Arguments("Production")]
    [Arguments("Staging")]
    public async Task Fora_de_Development_sem_opt_in_nada_e_mapeado(string ambiente)
    {
        await using var app = await StartAsync(environment: ambiente);
        var client = app.GetTestClient();

        await Assert.That(await StatusAsync(client, "/openapi/v1.json")).IsEqualTo(HttpStatusCode.NotFound);
        await Assert.That(await StatusAsync(client, "/docs/v1")).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Fora_de_Development_sem_opt_in_convencoes_no_retorno_nao_quebram()
    {
        await using var app = await StartAsync(environment: "Production", map: e => e.MapTecApiDocs().RequireAuthorization());
        await Assert.That(await StatusAsync(app.GetTestClient(), "/openapi/v1.json")).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Fora_de_Development_com_opt_in_e_mapeado_e_respeita_a_autorizacao()
    {
        await using var app = await StartAsync(o =>
        {
            o.ExposeOutsideDevelopment = true;
            o.RequireDocumentationAuthorization = true;
        }, environment: "Production");
        var client = app.GetTestClient();

        await Assert.That(await StatusAsync(client, "/openapi/v1.json")).IsEqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(await StatusAsync(client, "/openapi/v1.json", user: "alice")).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Mapear_sem_AddTecOpenApi_derruba_a_subida()
    {
        var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
        builder.WebHost.UseTestServer();
        builder.Services.AddRouting();
        await using var app = builder.Build();

        Assert.Throws<InvalidOperationException>(() => app.MapTecApiDocs());
    }
}
