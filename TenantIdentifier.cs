using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Linq;
using System.Threading.Tasks;
using TentiaCloud.UtilitiesAPI.Entities;
using TentiaCloud.UtilitiesAPI.Models.Db;

namespace TentiaCloud.UtilitiesAPI.Middleware
{
    public class TenantIdentifier
    {
        private readonly RequestDelegate _next;
        public IConfiguration _configuration { get; }

        public TenantIdentifier(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext httpContext, GlobalDBContext dbContext)
        {
            var tenantID = httpContext.Request.Headers["tenantID"].FirstOrDefault();
            if (!string.IsNullOrEmpty(tenantID))
            {
                var tenant = dbContext.customer.FirstOrDefault(p => p.instancia == tenantID);

                var connectionStringBuilder = new MySqlConnectionStringBuilder
                {
                    Server = tenant.nombre_servidor,
                    UserID = tenant.usuario_db,
                    Password = tenant.pwd_db,
                    Database = tenant.nombre_db
                };

                Tenant connectionTenant = new()
                {
                    Id = tenant.id,
                    Nombre = tenant.instancia,
                    CadenaConexion = connectionStringBuilder.ConnectionString
                };
                httpContext.Items["TENANT"] = connectionTenant;
            }

            await _next.Invoke(httpContext);
        }
    }

    public static class TenantIdentifierExtension
    {
        public static IApplicationBuilder UseTenantIdentifier(this IApplicationBuilder app)
        {
            app.UseMiddleware<TenantIdentifier>();
            return app;
        }
    }
}
