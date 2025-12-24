using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR(s =>
{
    if (builder.Environment.IsDevelopment())
    {
        s.EnableDetailedErrors = true;
    }
});


builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "PointingPokerAuthentication";
        options.LoginPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(3);
    });

builder.Host.UseSerilog((context, services, config) =>
{
    config
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services);
});

var app = builder.Build();

var cookiePolicyOptions = new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
};

app.UseCookiePolicy(cookiePolicyOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.MapHub<PointingPokerHub>("/pointingpokerhub");

app.Run();