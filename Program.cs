using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShiftPlanner;
using ShiftPlanner.Interfaces;
using ShiftPlanner.Repositary;

var builder = WebApplication.CreateBuilder(args);

//add db context
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("VogueAusDB"));
});
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IShiftRepositary, ShiftRepositary>();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Shift}/{action=Index}/{id?}");

app.Run();
