using System.Text;
using ERP.Application.Interfaces;
using ERP.Application.Mapping;
using ERP.Application.Sepicifications;
using ERP.Infrastructure.Auth;
using ERP.Infrastructure.Identity;
using ERP.Infrastructure.Persistence;
using ERP.Infrastructure.Services;
using ERP.Infrastructure.Specifications;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ERP.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            // SQL Server
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlServer(config.GetConnectionString("DefaultConnection")));

            // Identity
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // JWT
            // JWT Settings al
            var jwtSection = config.GetSection("Jwt");
            var settings = jwtSection.Get<JwtSettings>() ?? throw new InvalidOperationException("JWT configuration is missing!");
            services.AddSingleton(settings);

            // Authentication ve JWT Bearer ayarlarý
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // development/test için
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero, // token expire süresini tam uygular
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = settings.Issuer,
                    ValidAudience = settings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key))
                };

                // Opsiyonel: token hatasýný daha net almak için event
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        Console.WriteLine("Authentication failed: " + ctx.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = ctx =>
                    {
                        Console.WriteLine("Token validated for: " + ctx.Principal?.Identity?.Name);
                        return Task.CompletedTask;
                    }
                };
            });


            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdmin", p => p.RequireRole("Admin"));
            });

            // AutoMapper
            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            // App services
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IReservationService, ReservationService>();
            services.AddSingleton<IJwtTokenService, JwtTokenService>();
            services.AddScoped(typeof(IDataShaper<>), typeof(DataShaper<>));



            return services;
        }
    }
}
