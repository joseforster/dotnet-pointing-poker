using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.SignalR;
using PointingPoker.Hubs;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

Log.Information("Starting up!");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

// Add services to the container.
    builder.Services.AddRazorPages();
    builder.Services.AddSignalR(s =>
    {
        if (builder.Environment.IsDevelopment())
        {
            s.EnableDetailedErrors = true;
        }

        s.AddFilter<RateLimitFilter>();
    });

    // Add background services
    builder.Services.AddHostedService<PointingPoker.Services.SessionCleanupService>();

    builder.Services
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.Cookie.Name = "PointingPokerAuth";
            options.LoginPath = "/Login";
            options.ExpireTimeSpan = TimeSpan.FromHours(12);
            options.SlidingExpiration = true;
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

    await app.RunAsync();

    Log.Information("Stopped cleanly");
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unhandled exception occurred during startup.");
}
finally
{
    Log.CloseAndFlush();
}

