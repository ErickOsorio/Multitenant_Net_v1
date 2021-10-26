using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Threading.Tasks;
using TentiaCloud.UtilitiesAPI.Hubs;
using TentiaCloud.UtilitiesAPI.Interface;
using TentiaCloud.UtilitiesAPI.Middleware;
using TentiaCloud.UtilitiesAPI.Models.Db;
using TentiaCloud.UtilitiesAPI.Repository;
using TentiaCloud.UtilitiesAPI.Repository.IRepository;

namespace TentiaCloud.UtilitiesAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ITerminalRepository, TerminalRepository>();
            services.AddScoped<IUsersRepository, UsersRepository>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // To access HttpContext
            services.AddDbContext<GlobalDBContext>((serviceProvider, dbContextBuilder) =>
            {
                var connectionString = Configuration.GetConnectionString("DefaultConnection");
                var serverVersion = new MySqlServerVersion(new Version(5, 7, 32));
                dbContextBuilder.UseMySql(connectionString, serverVersion);
            });
            services.AddDbContext<TenantDbContext>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["UtilitiesApi:SecretKey"]))
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };

            });

            services.AddSignalR()
                    .AddAzureSignalR();

            services.AddSingleton<IUserConnectionManager, UserConnectionManager>();

            services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
            {
                builder.AllowAnyMethod().AllowAnyHeader()
                .WithOrigins(Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
            }));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);

            services.AddControllers().AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("CorsPolicy");

            app.UseAuthorization();

            app.UseTenantIdentifier(); // Middleware

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ScalesHub>("Hub/scalesHub");
                endpoints.MapHub<BelugaHub>("Hub/bPayHub");
            });

        }
    }
}
