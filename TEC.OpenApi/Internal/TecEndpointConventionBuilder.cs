using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;

namespace TEC.OpenApi.Internal;

/// <summary>
/// Repassa cada convenção a vários construtores (documento e interface). Sem construtores, é o retorno dos <c>MapTec*</c>
/// quando a guarda de ambiente bloqueia o mapeamento: as convenções são descartadas porque não há endpoint.
/// </summary>
internal sealed class TecEndpointConventionBuilder(IReadOnlyList<IEndpointConventionBuilder> builders) : IEndpointConventionBuilder
{
    public static readonly TecEndpointConventionBuilder Empty = new([]);

    public void Add(Action<EndpointBuilder> convention)
    {
        ArgumentNullException.ThrowIfNull(convention);
        foreach (var builder in builders)
            builder.Add(convention);
    }

    public void Finally(Action<EndpointBuilder> finallyConvention)
    {
        ArgumentNullException.ThrowIfNull(finallyConvention);
        foreach (var builder in builders)
            builder.Finally(finallyConvention);
    }
}
