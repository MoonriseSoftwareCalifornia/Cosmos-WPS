using Cosmos.Cms.Publisher.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Cosmos.Cms.Publisher.Controllers
{
    /// <summary>
    /// This is a controller that demonstrates how to add your functionality for uses
    /// with Cosmos WPS.
    /// </summary>
    public class CosmosWpsPluginsController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEmailSender _emailSender;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="emailSender"></param>
        public CosmosWpsPluginsController(ILogger<HomeController> logger, IEmailSender emailSender)
        {
            _logger = logger;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> NuGetSearch(string searchTerm)
        {
            SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            PackageSearchResource resource = await repository.GetResourceAsync<PackageSearchResource>();
            SearchFilter searchFilter = new SearchFilter(includePrerelease: false);
            CancellationToken cancellationToken = CancellationToken.None;
            NuGet.Common.ILogger logger = NullLogger.Instance;

            IEnumerable<IPackageSearchMetadata> results = await resource.SearchAsync(
                searchTerm,
                searchFilter,
                skip: 0,
            take: 20,
                logger,
                cancellationToken);

            var data = new List<NuGetPkgItem>();

            foreach (IPackageSearchMetadata result in results)
            {
                data.Add(new NuGetPkgItem()
                {
                    Authors = result.Authors,
                    Description = result.Description,
                    DownloadCount = result.DownloadCount,
                    Id = result.Identity.Id,
                    ProjectUrl = result.ProjectUrl == null ? "" : result.ProjectUrl.ToString(),
                    Version = result.Identity.Version.ToString(),
                    Owners = result.Owners
                });
            }
            return Json(data);
        }
    }
}
