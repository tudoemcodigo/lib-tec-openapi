using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TEC.OpenApi;

namespace TEC.OpenApi.Tests;

public class TecOpenApiIntegrationTests
{
    [Test]
    public async Task Scalar_endpoint_is_accessible_and_returns_html()
    {
        var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions());
        builder.WebHost.UseTestServer();
        builder.Services.AddRouting();
        builder.Services.AddTecOpenApi(o =>
        {
            o.Title = "Integration Test API";
            o.Version = "v1";
        });

        var app = builder.Build();
        app.UseRouting();
        app.MapTecApiDocs();

        await app.StartAsync();

        var client = app.GetTestClient();
        var response = await client.GetAsync("/docs/v1");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        await Assert.That(content.Contains("Integration Test API") || content.Contains("scalar")).IsTrue();

#if NET9_0_OR_GREATER || NET10_0_OR_GREATER
        var openApiResponse = await client.GetAsync("/openapi/v1.json");
        await Assert.That(openApiResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var openApiContent = await openApiResponse.Content.ReadAsStringAsync();
        await Assert.That(openApiContent.Contains("Integration Test API")).IsTrue();
#endif

        await app.StopAsync();
    }
}
