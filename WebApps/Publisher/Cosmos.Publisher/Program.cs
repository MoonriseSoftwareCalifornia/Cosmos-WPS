using AspNetCore.Identity.CosmosDb.Extensions;
using AspNetCore.Identity.Services.SendGrid;
using AspNetCore.Identity.Services.SendGrid.Extensions;
using Cosmos.Common.Data;
using Cosmos.Common.Data.Logic;
using Cosmos.Cms.Common.Services.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();


// Add CORS
// See: https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-6.0

var corsOrigins = builder.Configuration.GetValue<string>("CorsAllowedOrigins");
if (string.IsNullOrEmpty(corsOrigins))
{
    builder.Services.AddCors();
}
else
{
    var origins = corsOrigins.Split(',');
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: "AllowedOrigPolicy",
                          policy =>
                          {
                              policy.WithOrigins(origins);
                          });
    });
}

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection");
// Name of the Cosmos database to use
var cosmosIdentityDbName = builder.Configuration.GetValue<string>("CosmosIdentityDbName");

//
// Add the Cosmos database context here
//
var cosmosRegionName = builder.Configuration.GetValue<string>("CosmosRegionName");
if (string.IsNullOrEmpty(cosmosRegionName))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
      options.UseCosmos(connectionString: connectionString, databaseName: cosmosIdentityDbName));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseCosmos(connectionString: connectionString, databaseName: cosmosIdentityDbName,
            cosmosOps =>
            {
                cosmosOps.Region(cosmosRegionName);
            });
    });
}
#pragma warning restore CS8604 // Possible null reference argument.

builder.Services.AddMvc()
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ContractResolver =
                        new DefaultContractResolver());

//
// Add Cosmos Identity here
//
builder.Services.AddCosmosIdentity<ApplicationDbContext, IdentityUser, IdentityRole>(
      options =>
      {
          options.SignIn.RequireConfirmedAccount = true;
      }
    )
    .AddDefaultUI() // Use this if Identity Scaffolding added
    .AddDefaultTokenProviders();

// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-3.1&tabs=visual-studio
builder.Services.ConfigureApplicationCookie(o =>
{
    o.Cookie.Name = "CosmosAuthCookie";
    o.ExpireTimeSpan = TimeSpan.FromDays(5);
    o.SlidingExpiration = true;
});

//
// Get the boot variables loaded, and
// do some validation to make sure Cosmos can boot up
// based on the values given.
//
var cosmosStartup = new CosmosStartup(builder.Configuration);

// Add Cosmos Options
var option = cosmosStartup.Build();

builder.Services.AddSingleton(option);
builder.Services.AddTransient<ArticleLogic>();

builder.Services.AddControllersWithViews();

// Email provider
var sendGridApiKey = builder.Configuration.GetValue<string>("CosmosSendGridApiKey");
var adminEmail = builder.Configuration.GetValue<string>("CosmosAdminEmail");
if (!string.IsNullOrEmpty(sendGridApiKey) && !string.IsNullOrEmpty(adminEmail))
{
    var sendGridOptions = new SendGridEmailProviderOptions(sendGridApiKey, adminEmail);
    builder.Services.AddSendGridEmailProvider(sendGridOptions);
}

var app = builder.Build();

// Middle-ware proper order:
// See: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0#middleware-order
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();

app.UseRouting();

if (string.IsNullOrEmpty(corsOrigins))
{
    // See: https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-6.0
    app.UseCors();
}
else
{
    app.UseCors("AllowedOrigPolicy");
}

//app.UseOutputCache();

app.UseAuthentication();
app.UseAuthorization();

//app.UseResponseCompression();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
            "MsValidation",
            ".well-known/microsoft-identity-association.json",
            new { controller = "Home", action = "GetMicrosoftIdentityAssociation" });

app.MapControllerRoute(
            "MyArea",
            "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapFallbackToController("Index", "Home");

app.MapRazorPages();

app.Run();
