using Microsoft.Extensions.Logging;

namespace TEC.OpenApi.Internal;

internal static partial class TecOpenApiLog
{
    [LoggerMessage(EventId = 5001, Level = LogLevel.Warning,
        Message = "Documentação OpenAPI/Scalar não mapeada: o ambiente '{Environment}' não é Development e " +
                  "TecOpenApiOptions.ExposeOutsideDevelopment está desligado.")]
    public static partial void DocumentationNotMapped(ILogger logger, string environment);
}
