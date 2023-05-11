using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Common.Services.Configurations
{
    /// <summary>
    ///     Simple proxy service config
    /// </summary>
    public class SimpleProxyConfigs
    {
        /// <summary>
        ///     Array of configurations
        /// </summary>
        [Display(Name = "Proxy configuration(s)")]
        public ProxyConfig[] Configs { get; set; }
    }

    /// <summary>
    ///     Proxy configuration
    /// </summary>
    public class ProxyConfig
    {
        /// <summary>
        ///     Name of the connection
        /// </summary>
        [Display(Name = "Connection name")]
        public string Name { get; set; }

        /// <summary>
        ///     Method (i.e. GET or POST)
        /// </summary>
        [Display(Name = "Method (i.e. GET or POST)")]
        public string Method { get; set; }

        /// <summary>
        ///     URL end point
        /// </summary>
        [Display(Name = "URL end point")]
        public string UriEndpoint { get; set; }

        /// <summary>
        ///     GET string or POST data
        /// </summary>
        [Display(Name = "GET or POST data")]
        public string Data { get; set; }

        /// <summary>
        ///     User name to use when accessing end point.
        /// </summary>
        [Display(Name = "User name")]
        public string UserName { get; set; } = "";

        /// <summary>
        ///     Password to use when accessing end point.
        /// </summary>
        [Display(Name = "Password")]
        public string Password { get; set; } = "";

        /// <summary>
        ///     Content type
        /// </summary>
        [Display(Name = "Content type")]
        public string ContentType { get; set; } = "application/x-www-form-urlencoded";

        /// <summary>
        ///     RBAC roles allowed to use end point
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Anonymous role enables anyone to use end point. Authenticated role allows any authenticated user access.
        ///         Otherwise the specifc roles who have access are listed here.
        ///     </para>
        /// </remarks>
        [Display(Name = "Roles")]
        public string[] Roles { get; set; }
    }
}