using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace tpintegrador_psr2026.Api;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            foreach (var nombre in Enum.GetNames(context.Type))
            {
                schema.Enum.Add(new OpenApiString(nombre));
            }
        }
    }
}