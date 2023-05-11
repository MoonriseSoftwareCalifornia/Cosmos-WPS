using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Common.Services.Configurations
{
    /// <summary>
    ///     Authentication provider configurations
    /// </summary>
    public class AuthenticationConfig
    {
        /// <summary>
        ///     Microsoft authentication configuration
        /// </summary>
        public Microsoft Microsoft { get; set; }

        /// <summary>
        ///     Google authentication configuration
        /// </summary>
        public Google Google { get; set; }

        /// <summary>
        ///     Facebook authentication configuration
        /// </summary>
        public Facebook Facebook { get; set; }

        /// <summary>
        ///     Allow registration locally on the editor site?
        /// </summary>
        [Display(Name = "Allow C/CMS accounts")]
        public bool? AllowLocalRegistration { get; set; } = true;
    }

    /// <summary>
    ///     Microsoft configuration
    /// </summary>
    public class Microsoft
    {
        /// <summary>
        ///     Client Id
        /// </summary>
        [Display(Name = "Client Id")]
        public string ClientId { get; set; }

        /// <summary>
        ///     Client Secret
        /// </summary>
        [Display(Name = "Client secret")]
        public string ClientSecret { get; set; }
    }

    /// <summary>
    ///     Google configuration
    /// </summary>
    public class Google
    {
        /// <summary>
        ///     Client Id
        /// </summary>
        [Display(Name = "Client Id")]
        public string ClientId { get; set; }

        /// <summary>
        ///     Client Secret
        /// </summary>
        [Display(Name = "Client secret")]
        public string ClientSecret { get; set; }
    }

    /// <summary>
    ///     Facebook configuration
    /// </summary>
    public class Facebook
    {
        /// <summary>
        ///     Application Id
        /// </summary>
        [Display(Name = "Application Id")]
        public string AppId { get; set; }

        /// <summary>
        ///     Application Secret
        /// </summary>
        [Display(Name = "Application secret")]
        public string AppSecret { get; set; }
    }
}