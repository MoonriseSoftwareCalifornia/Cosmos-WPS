using AspNetCore.Identity.CosmosDb.Extensions;
using AspNetCore.Identity.Services.SendGrid;
using AspNetCore.Identity.Services.SendGrid.Extensions;
using Azure.Identity;
using Azure.ResourceManager;
using Cosmos.BlobService;
using Cosmos.Cms.Common.Services.Configurations;
using Cosmos.Cms.Data.Logic;
using Cosmos.Cms.Hubs;
using Cosmos.Cms.Services;
using Cosmos.Common.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using System;
using System.Threading.Tasks;

namespace Cosmos.Cms
{
    /// <summary>
    ///     Startup class for the website.
    /// </summary>
    public class Startup
    {

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        ///     Configuration for the website.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        ///     Method configures services for the website.
        /// </summary>
        /// <param name="services"></param>
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // The following line enables Application Insights telemetry collection.
            // See: https://learn.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core?tabs=netcore6
            services.AddApplicationInsightsTelemetry();

            // The Cosmos connection string
            var connectionString = Configuration.GetConnectionString("ApplicationDbContextConnection");

            // Name of the Cosmos database to use
            var cosmosIdentityDbName = Configuration.GetValue<string>("CosmosIdentityDbName");

            // If this is set, the Cosmos identity provider will:
            // 1. Create the database if it does not already exist.
            // 2. Create the required containers if they do not already exist.
            // IMPORTANT: Remove this variable if after first run. It will improve startup performance.
            var setupCosmosDb = Configuration.GetValue<bool?>("SetupCosmosDb");

            // If the following is set, it will create the Cosmos database and
            //  required containers.
            if (setupCosmosDb.HasValue && setupCosmosDb.Value)
            {
                var builder1 = new DbContextOptionsBuilder<ApplicationDbContext>();
                builder1.UseCosmos(connectionString, cosmosIdentityDbName);

                using (var dbContext = new ApplicationDbContext(builder1.Options))
                {
                    dbContext.Database.EnsureCreated();
                }
            }

            //
            // Add the Cosmos database context here
            //
            var cosmosRegionName = Configuration.GetValue<string>("CosmosRegionName");
            if (string.IsNullOrEmpty(cosmosRegionName))
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                  options.UseCosmos(connectionString: connectionString, databaseName: cosmosIdentityDbName));
            }
            else
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseCosmos(connectionString: connectionString, databaseName: cosmosIdentityDbName,
                        cosmosOps =>
                        {
                            cosmosOps.Region(cosmosRegionName);
                        });
                });
            }

            //
            // Add Cosmos Identity here
            //
            services.AddCosmosIdentity<ApplicationDbContext, IdentityUser, IdentityRole>(
                  options => options.SignIn.RequireConfirmedAccount = true
                )
                .AddDefaultUI() // Use this if Identity Scaffolding added
                .AddDefaultTokenProviders();

            // Add shared data protection here
            services.AddDataProtection().PersistKeysToDbContext<ApplicationDbContext>();

            // Add IDistributed cache using Cosmos DB
            // This enables the editor to run in a web farm without needing
            // the "sticky bit" set.
            // See: https://github.com/Azure/Microsoft.Extensions.Caching.Cosmos
            services.AddCosmosCache((CosmosCacheOptions cacheOptions) =>
            {
                cacheOptions.ContainerName = "EditorCache";
                cacheOptions.DatabaseName = cosmosIdentityDbName;
                cacheOptions.ClientBuilder = new CosmosClientBuilder(connectionString);
                cacheOptions.CreateIfNotExists = true;
            });
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(3600);
                options.Cookie.IsEssential = true;
            });

            // https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-3.1&tabs=visual-studio
            services.ConfigureApplicationCookie(o =>
            {
                o.Cookie.Name = "CosmosAuthCookie";
                o.ExpireTimeSpan = TimeSpan.FromDays(5);
                o.SlidingExpiration = true;
            });

            var azureSubscription = new AzureSubscription();

            try
            {
                var armClient = new ArmClient(new DefaultAzureCredential());
                azureSubscription.Subscription = armClient.GetDefaultSubscription();
            }
            catch
            {
                // Nothing to do right now.
            }

            services.AddSingleton(azureSubscription);

            //
            // Get the boot variables loaded, and
            // do some validation to make sure Cosmos can boot up
            // based on the values given.
            //
            var cosmosStartup = new CosmosStartup(Configuration);

            // Add Cosmos Options
            var option = cosmosStartup.Build();
            services.AddSingleton(option);

            //
            // Add services
            //

            //
            // Must have an Email sender when using Identity Framework.
            // You will need an IEmailProvider. Below uses a SendGrid EmailProvider. You can use another.
            // Below users NuGet package: AspNetCore.Identity.Services.SendGrid
            var sendGridApiKey = Configuration.GetValue<string>("CosmosSendGridApiKey");
            var adminEmail = Configuration.GetValue<string>("CosmosAdminEmail");
            var sendGridOptions = new SendGridEmailProviderOptions(sendGridApiKey, adminEmail);
            services.AddSendGridEmailProvider(sendGridOptions);

            // End add SendGrid

            // Add the BLOB and File Storage contexts for Cosmos WPS
            services.AddCosmosStorageContext(Configuration);

            // Add file share storage context
            var fileStorageCon = Configuration.GetValue<string>("AzureFileStorageConnectionString");
            if (string.IsNullOrEmpty(fileStorageCon))
            {
                // Connect using the blob storage connection string
                fileStorageCon = Configuration.GetValue<string>("AzureBlobStorageConnectionString");
            }

            var fileShare = Configuration.GetValue<string>("CosmosFileShare");
            if (string.IsNullOrEmpty(fileShare))
            {
                fileShare = "ccmsshare";
            }
            services.AddSingleton(new FileStorageContext(fileStorageCon, fileShare));

            services.AddTransient<ArticleEditLogic>();


            // This is used by the ViewRenderingService 
            // to export web pages for external editing.
            services.AddScoped<IViewRenderService, ViewRenderService>();

            services.AddCors(options =>
            {
                options.AddPolicy("AllCors",
                    policy =>
                    {
                        policy.AllowAnyOrigin().AllowAnyMethod();
                    });
            });

            //

            // Add this before identity
            // See also: https://learn.microsoft.com/en-us/aspnet/core/performance/caching/response?view=aspnetcore-7.0
            services.AddControllersWithViews();

            services.AddRazorPages();

            services.AddMvc()
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ContractResolver =
                        new DefaultContractResolver())
                .AddRazorPagesOptions(options =>
                {
                    // This section docs are here: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/scaffold-identity?view=aspnetcore-3.1&tabs=visual-studio#full
                    //options.AllowAreas = true;
                    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
                    options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
                });

            // https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-2.1&tabs=visual-studio#http-strict-transport-security-protocol-hsts
            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
                //options.ExcludedHosts.Add("example.com");
                //options.ExcludedHosts.Add("www.example.com");
            });

            services.ConfigureApplicationCookie(options =>
            {
                // This section docs are here: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/scaffold-identity?view=aspnetcore-3.1&tabs=visual-studio#full
                // The following is when using Docker container with a proxy like
                // Azure front door. It ensures relative paths for redirects
                // which is necessary when the public DNS at Front door is www.mycompany.com 
                // and the DNS of the App Service is something like myappservice.azurewebsites.net.
                options.Events.OnRedirectToLogin = x =>
                {
                    x.Response.Redirect("/Identity/Account/Login");
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToLogout = x =>
                {
                    x.Response.Redirect("/Identity/Account/Logout");
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = x =>
                {
                    x.Response.Redirect("/Identity/Account/AccessDenied");
                    return Task.CompletedTask;
                };
            });

            // BEGIN
            // When deploying to a Docker container, the OAuth redirect_url
            // parameter may have http instead of https.
            // Providers often do not allow http because it is not secure.
            // So authentication will fail.
            // Article below shows instructions for fixing this.
            //
            // NOTE: There is a companion secton below in the Configure method. Must have this
            // app.UseForwardedHeaders();
            //
            // https://seankilleen.com/2020/06/solved-net-core-azure-ad-in-docker-container-incorrectly-uses-an-non-https-redirect-uri/
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                           ForwardedHeaders.XForwardedProto;
                // Only loopback proxies are allowed by default.
                // Clear that restriction because forwarders are enabled by explicit
                // configuration.
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
            // END

            services.AddResponseCaching();

            // https://docs.microsoft.com/en-us/dotnet/core/compatibility/aspnet-core/5.0/middleware-database-error-page-obsolete
            //services.AddDatabaseDeveloperPageExceptionFilter();

            // Add the SignalR service.
            // If there is a DB connection, then use SQL backplane.
            // See: https://github.com/IntelliTect/IntelliTect.AspNetCore.SignalR.SqlServer
            var signalRConnection = Configuration.GetConnectionString("CosmosSignalRConnection");
            if (string.IsNullOrEmpty(signalRConnection))
            {
                services.AddSignalR();
            }
            else
            {
                services.AddSignalR().AddAzureSignalR(signalRConnection);
            }
        }

        /// <summary>
        ///     This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            // BEGIN
            // https://seankilleen.com/2020/06/solved-net-core-azure-ad-in-docker-container-incorrectly-uses-an-non-https-redirect-uri/
            app.UseForwardedHeaders();
            // END

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // app.UseHttpsRedirection(); // See: https://github.com/dotnet/aspnetcore/issues/18594
            app.UseStaticFiles();
            app.UseRouting();

            app.UseCors();

            app.UseResponseCaching(); //https://docs.microsoft.com/en-us/aspnet/core/performance/caching/middleware?view=aspnetcore-3.1


            //app.Use(async (context, next) =>
            //{
            //    //
            //    //context.Response.Headers[HeaderNames.CacheControl] = "no-store";
            //    context.Response.GetTypedHeaders().CacheControl =
            //        new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
            //        {
            //            NoStore = true,
            //            NoCache = false
            //        };
            //    //context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
            //    //    new string[] { "Accept-Encoding" };

            //    await next();
            //});

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // Point to the route that will return the SignalR Hub.
                endpoints.MapHub<LiveEditorHub>("/___cwps_hubs_live_editor");

                endpoints.MapControllerRoute(
                    "MsValidation",
                    ".well-known/microsoft-identity-association.json",
                    new { controller = "Home", action = "GetMicrosoftIdentityAssociation" });

                endpoints.MapControllerRoute(
                    "MyArea",
                    "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                        "default",
                        "{controller=Home}/{action=Index}/{id?}");

                // Deep path
                endpoints.MapFallbackToController("Index", "Home");

                endpoints.MapRazorPages();

            });
        }

    }
}