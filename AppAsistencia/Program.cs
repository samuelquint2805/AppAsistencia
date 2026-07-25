using AppAsistencia;
using AppAsistencia.Data;
using AppAsistencia.Data.DBSET;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddCustomConfiguration();
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<RoleRouteSeeder>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Invocar e inicializar el Seeder dentro de un ámbito de servicios
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Se obtiene el DbContext o el servicio de Seeder
    var context = services.GetRequiredService<DataContextAsistencia>();
    
}
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=LoginSelection}/{id?}")
    .WithStaticAssets();

app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var seeder = scope.ServiceProvider.GetRequiredService<RoleRouteSeeder>();
    var context2 = services.GetRequiredService<DataContextAsistencia>();
    await seeder.SeedAsync(context2);
}


app.Run();
