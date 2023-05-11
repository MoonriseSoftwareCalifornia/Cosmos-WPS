using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;

namespace Cosmos.Cms.Common.Services.Configurations
{
    /// <summary>
    ///     Cosmos startup configuration
    /// </summary>
    public class CosmosStartup
    {
        #region CONSTRUCTORS

        /// <summary>
        /// Parameterless constructor. No validation.
        /// </summary>
        public CosmosStartup()
        {
        }

        /// <summary>
        /// Builds boot configuration and performs validation.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="services"></param>
        /// <remarks>
        /// Validates the boot configuration.
        /// </remarks>
        public CosmosStartup(IConfiguration configuration)
        {
            _configuration = configuration;

            //
            // Add Azure Key Vault to config?
            //
            var useAzureVault = GetValue<bool>("CosmosUseAzureVault");
            if (useAzureVault)
            {

                var builder = new ConfigurationBuilder();

                builder.AddConfiguration(_configuration);

                var useDefaultCredential = GetValue<bool>("CosmosUseDefaultCredential");

                if (useDefaultCredential)
                {
                    builder.AddAzureKeyVault(new Uri(GetValue<string>("CosmosAzureVaultUrl")),
                        new DefaultAzureCredential());
                }
                else
                {
                    builder.AddAzureKeyVault(new Uri(GetValue<string>("CosmosAzureVaultUrl")),
                        new ClientSecretCredential(
                                GetValue<string>("CosmosAzureVaultTenantId"),
                                GetValue<string>("CosmosAzureVaultClientId"),
                                GetValue<string>("CosmosAzureVaultClientSecret")));
                }

                _configuration = builder.Build();
            }

        }


        #endregion

        private readonly IConfiguration _configuration;

        #region PRIVATE METHODS

        /// <summary>
        /// Gets a value from the configuration, and records what was found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueName"></param>
        /// <returns></returns>
        private T GetValue<T>(string valueName)
        {
            var val = _configuration[valueName];

            object outputValue;

            if (typeof(T) == typeof(bool))
            {
                // default to false
                if (string.IsNullOrEmpty(val))
                {
                    outputValue = false;
                }
                else if (bool.TryParse(val, out bool parsedValue))
                {
                    outputValue = parsedValue;
                }
                else
                {
                    outputValue = false;
                }
            }
            else if (typeof(T) == typeof(bool?))
            {
                if (bool.TryParse(val, out bool parsedValue))
                {
                    outputValue = parsedValue;
                }
                else
                {
                    outputValue = null;
                }
            }
            else if (typeof(T) == typeof(Uri))
            {
                if (Uri.TryCreate(val, UriKind.Absolute, out Uri uri))
                {
                    outputValue = uri;
                }
                else
                {
                    outputValue = null;
                }
            }
            else
            {
                outputValue = val;
            }

            return (T)outputValue;
        }

        #endregion

        /// <summary>
        /// Attempts to run Cosmos Startup.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method tries to run Cosmos and collects diagnostics in the process.
        /// </para>
        /// <para>
        /// Check <see cref="HasErrors"/> to see if there are any errors detected.
        /// </para>
        /// <para>
        /// If boot time value CosmosAllowSetup is set to true, then diagnostic tests are 
        /// run to determine if cloud resource can be connected to. This can significantly delay boot
        /// up time for Cosmos.  Once Cosmos is setup, set CosmosAllowSetup to false.
        /// </para>
        /// </remarks>
        public IOptions<CosmosConfig> Build()
        {
            // Read the boot configuration values and check them
            //ReadBootConfig();

            var cosmosConfig = new CosmosConfig();

            // SETUP VALUES
            cosmosConfig.SiteSettings.AllowSetup = GetValue<bool?>("CosmosAllowSetup");
            cosmosConfig.SiteSettings.AllowConfigEdit = GetValue<bool>("CosmosAllowConfigEdit");
            cosmosConfig.SiteSettings.AllowReset = GetValue<bool>("CosmosAllowReset");

            // Microsoft App ID
            cosmosConfig.SecretName = GetValue<string>("CosmosSecretName");
            cosmosConfig.MicrosoftAppId = GetValue<string>("MicrosoftAppId");
            cosmosConfig.PrimaryCloud = GetValue<string>("CosmosPrimaryCloud");
            cosmosConfig.SendGridConfig.EmailFrom = GetValue<string>("CosmosAdminEmail");
            cosmosConfig.SendGridConfig.SendGridKey = GetValue<string>("CosmosSendGridApiKey");

            // Cosmos Endpoints
            cosmosConfig.SiteSettings.PublisherUrl = GetValue<string>("CosmosPublisherUrl");
            cosmosConfig.SiteSettings.FileShare = GetValue<string>("CosmosFileShare");
            cosmosConfig.SiteSettings.BlobPublicUrl = GetValue<string>("CosmosStorageUrl");
            cosmosConfig.SiteSettings.BlobPublicUrl = cosmosConfig.SiteSettings.BlobPublicUrl?.TrimEnd('/');
            var editorUrl = GetValue<string>("CosmosEditorUrl");
            if (!string.IsNullOrEmpty(editorUrl))
            {
                cosmosConfig.EditorUrls.Add(new EditorUrl() { CloudName = cosmosConfig.PrimaryCloud, Url = editorUrl });
            };

            return Options.Create(cosmosConfig);

        }

    }
}