using System;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace tpintegrador_psr2026.Api;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum && schema.Enum != null)
        {
            schema.Enum.Clear();
            foreach (var nombre in Enum.GetNames(context.Type))
            {
                schema.Enum.Add(JsonValue.Create(nombre));
            }
        }
    }
}