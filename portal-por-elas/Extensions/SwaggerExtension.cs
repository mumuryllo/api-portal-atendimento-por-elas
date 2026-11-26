using Microsoft.OpenApi.Models;

namespace portal_por_elas.Extensions
{
    public static class SwaggerExtension
    {
        public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                if (File.Exists(xmlPath))
                    c.IncludeXmlComments(xmlPath);

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Portal Por Elas",
                    Version = "v1",
                    Description = "API de Portal de atendimento de serviços a mulheres"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Digite: Bearer {seu token}"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerWithGatewaySupport(this IApplicationBuilder app)
        {
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    var envPrefix = httpReq.Headers["X-Environment"].FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(envPrefix))
                    {
                        var prefix = envPrefix.Equals("Production", StringComparison.OrdinalIgnoreCase)
                            ? "/portal-por-elas/prod"
                            : "/portal-por-elas/dev";

                        swaggerDoc.Servers = new List<OpenApiServer>
                        {
                            new() { Url = prefix }
                        };
                    }
                });
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "portal-por-elas v1");
            });

            return app;
        }
    }
}