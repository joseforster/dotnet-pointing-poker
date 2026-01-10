using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up!");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(new ExpressionTemplate(
            "[{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m}\n{@x}",
            theme: TemplateTheme.Code)));

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
            options.Cookie.Name = "PointingPokerAuth";
            options.LoginPath = "/Login";
            options.ExpireTimeSpan = TimeSpan.FromDays(3);
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

    app.UseSerilogRequestLogging();

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
    Log.Fatal(ex, "An unhandled exception occurred during bootstrapping");
}
finally
{
    Log.CloseAndFlush();
}

