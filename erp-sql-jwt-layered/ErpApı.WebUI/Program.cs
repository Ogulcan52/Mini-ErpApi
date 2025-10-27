using ERP.Application.Interfaces;
using ERP.Infrastructure;
using ERP.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddInfrastructure(builder.Configuration);

// Eðer Identity veya JWT kullanýyorsan ekle
// builder.Services.AddAuthentication(...);
// builder.Services.AddAuthorization(...);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Eðer Authentication kullanýyorsan:
app.UseAuthentication();
app.UseAuthorization();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

app.Run();
