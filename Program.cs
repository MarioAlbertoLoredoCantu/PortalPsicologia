using Microsoft.EntityFrameworkCore;
using PortalPsicologia.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ✅ Agrega esta línea para habilitar sesiones
builder.Services.AddSession();

builder.Services.AddDbContext<PsicometricoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PsicometricoContext")));

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

// ✅ Agrega esta línea para usar sesiones
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

