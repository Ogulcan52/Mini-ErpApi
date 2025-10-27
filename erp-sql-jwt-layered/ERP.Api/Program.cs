using ERP.Application.Interfaces;
using ERP.Infrastructure;
using ERP.Infrastructure.Identity;
using ERP.Infrastructure.Persistence;
using ERP.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHostedService<ReservationExpireService>();



// Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ERP API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer", // küçük harf olmalı
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
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

// Infrastructure (DB, Identity, JWT, DI)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    // Seed roles + admin
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    if (!await roleMgr.RoleExistsAsync("Admin"))
        await roleMgr.CreateAsync(new IdentityRole("Admin"));

    var admin = await userMgr.FindByEmailAsync("admin@erp.local");
    if (admin is null)
    {
        admin = new ApplicationUser
        {
            UserName = "admin@erp.local",
            Email = "admin@erp.local",
            EmailConfirmed = true
        };
        await userMgr.CreateAsync(admin, "Admin*12345");
        await userMgr.AddToRoleAsync(admin, "Admin");
    }

    // Seed sample data
    if (!await db.Products.AnyAsync())
    {
        db.Customers.Add(new ERP.Domain.Entities.Customer { Name = "Acme A.Ş.", Email = "info@acme.com" });
        db.Products.AddRange(
            new ERP.Domain.Entities.Product { Name = "Kalem", Sku = "PRD-001", UnitPrice = 10, Stock = 250 },
            new ERP.Domain.Entities.Product { Name = "Defter", Sku = "PRD-002", UnitPrice = 25, Stock = 150 }
        );
        await db.SaveChangesAsync();
    }
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ERP API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthentication(); // mutlaka önce bu
app.UseAuthorization();

app.MapControllers();

app.Run();
