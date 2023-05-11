using Cosmos.Common.Data;
using Cosmos.Common.Models;
using Cosmos.Cms.Common.Services.Configurations;
using Cosmos.Cms.Data.Logic;
using Cosmos.Cms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.BlobService;

namespace Cosmos.Cms.Controllers
{
    /// <summary>
    /// Home page controller
    /// </summary>
    [Authorize]
    [ResponseCache(NoStore = true)]
    public class HomeController : Controller
    {
        private readonly ArticleEditLogic _articleLogic;
        private readonly IOptions<CosmosConfig> _options;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly StorageContext _storageContext;
        //private readonly SignInManager<IdentityUser> _signInManager;

        #region SETUP TESTS

        /// <summary>
        /// Insures there is an administrator
        /// </summary>
        /// <returns></returns>
        private async Task<bool> EnsureAdminSetup()
        {
            await _dbContext.Database.EnsureCreatedAsync();
            return await _dbContext.Users.CosmosAnyAsync() && (await _userManager.GetUsersInRoleAsync("Administrators")).Any();
        }

        /// <summary>
        /// Ensures there is a Layout
        /// </summary>
        /// <returns></returns>
        private async Task<bool> EnsureLayoutExists()
        {
            return await _dbContext.Layouts.CosmosAnyAsync();
        }

        /// <summary>
        /// Ensures there is at least one article
        /// </summary>
        /// <returns></returns>
        private async Task<bool> EnsureArticleExists()
        {
            return await _dbContext.Articles.CosmosAnyAsync();
        }


        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cosmosConfig"></param>
        /// <param name="dbContext"></param>
        /// <param name="articleLogic"></param>
        /// <param name="userManager"></param>
        /// <param name="storageContext"></param>
        public HomeController(ILogger<HomeController> logger,
            IOptions<CosmosConfig> cosmosConfig,
            ApplicationDbContext dbContext,
            ArticleEditLogic articleLogic,
            UserManager<IdentityUser> userManager,
            StorageContext storageContext
            )
        {
            _logger = logger;
            _options = cosmosConfig;
            _articleLogic = articleLogic;
            _dbContext = dbContext;
            _userManager = userManager;
            _storageContext = storageContext;
        }

        /// <summary>
        /// Editor home index method
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> CcmsContentIndex(string target)
        {
            var article = await _articleLogic.GetByUrl(target);

            return View(article);
        }

        /// <summary>
        /// Get edit list
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditList(string target)
        {
            var article = await _articleLogic.GetByUrl(target);

            var data = await _dbContext.Articles.OrderByDescending(o => o.VersionNumber)
                .Where(a => a.ArticleNumber == article.ArticleNumber).Select(s => new ArticleEditMenuItem
                {
                    Id = s.Id,
                    ArticleNumber = s.ArticleNumber,
                    Published = s.Published,
                    VersionNumber = s.VersionNumber,
                    UsesHtmlEditor = s.Content.ToLower().Contains(" editable=") || s.Content.ToLower().Contains(" data-ccms-ceid=")
                }).OrderByDescending(o => o.VersionNumber).Take(10).ToListAsync();

            return Json(data);
        }

        /// <summary>
        /// Index page
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            //if (_options.Value.SiteSettings.AllowSetup ?? false)
            //{
            //    if (!await EnsureAdminSetup())
            //    {
            //        return RedirectToAction("Index", "Setup");
            //    }
            //}
            try
            {
                if (User.Identity?.IsAuthenticated == false)
                {
                    //
                    // See if we need to register a new user.
                    //
                    if (await _dbContext.Users.CosmosAnyAsync()) return Redirect("~/Identity/Account/Login");
                    return Redirect("~/Identity/Account/Register");
                }
                else
                {
                    // Make sure the user's claims identity has an account here.
                    var user = await _userManager.GetUserAsync(User);

                    if (user == null)
                    {
                        Response.Cookies.Delete("CosmosAuthCookie");
                        return Redirect("~/Identity/Account/Logout");
                    }


                    if (!User.IsInRole("Reviewers") && !User.IsInRole("Authors") && !User.IsInRole("Editors") &&
                        !User.IsInRole("Administrators")) return RedirectToAction("AccessPending");
                }

                // Enable static website for Azure BLOB storage
                await _storageContext.EnableAzureStaticWebsite();

                // If we do not yet have a layout, go to a page where we can select one.
                if (!await EnsureLayoutExists()) return RedirectToAction("Index", "Layouts");

                // If there are not web pages yet, let's go create a new home page.
                if (!await EnsureArticleExists()) return RedirectToAction("Index", "Editor");

                

                //
                // If yes, do NOT include headers that allow caching. 
                //
                Response.Headers[HeaderNames.CacheControl] = "no-store";
                //Response.Headers[HeaderNames.Pragma] = "no-cache"; This conflicts with Azure Frontdoor premium with private links and affinity set.

                var article = await _articleLogic.GetByUrl(HttpContext.Request.Path, HttpContext.Request.Query["lang"]); // ?? await _articleLogic.GetByUrl(id, langCookie);

                // Article not found?
                // try getting a version not published.

                if (article == null)
                {
                    //
                    // Create your own not found page for a graceful page for users.
                    //
                    article = await _articleLogic.GetByUrl("/not_found", HttpContext.Request.Query["lang"]);

                    HttpContext.Response.StatusCode = 404;

                    if (article == null) return NotFound();
                }

                article.EditModeOn = false;
                article.ReadWriteMode = true;

                return View(article);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        /// <summary>
        ///     Gets an article by its ID (or row key).
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Preview(string id)
        {
            try
            {
                if (Guid.TryParse(id, out var pageId))
                {
                    ViewData["EditModeOn"] = false;
                    var article = await _articleLogic.Get(pageId, EnumControllerName.Home, User.Identity.Name);

                    // Check base header
                    //article.UrlPath = $"/home/preview/{id}";
                    //_articleLogic.UpdateHeadBaseTag(article);

                    // Home/Preview/154

                    if (article != null)
                    {
                        article.ReadWriteMode = false;
                        article.EditModeOn = false;

                        return View("Preview", article);
                    }
                }

                return NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        /// <summary>
        /// Error page
        /// </summary>
        /// <returns></returns>
        public IActionResult Error()
        {
            //Response.Headers[HeaderNames.CacheControl] = "no-store";
            //Response.Headers[HeaderNames.Pragma] = "no-cache";
            ViewData["EditModeOn"] = false;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Gets the application validation for Microsoft
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult GetMicrosoftIdentityAssociation()
        {
            var model = new MicrosoftValidationObject();
            var appIds = _options.Value.MicrosoftAppId.Split(',');

            foreach (var id in appIds)
            {
                model.associatedApplications.Add(new AssociatedApplication() { applicationId = id });
            }

            var data = Newtonsoft.Json.JsonConvert.SerializeObject(model);

            return File(Encoding.UTF8.GetBytes(data), "application/json", fileDownloadName: "microsoft-identity-association.json");
        }

        /// <summary>
        /// Returns a health check
        /// </summary>
        /// <returns></returns>
        /// 
        [AllowAnonymous]
        public async Task<IActionResult> CWPS_UTILITIES_NET_PING_HEALTH_CHECK()
        {
            try
            {
                var t = await _dbContext.Users.CountAsync();
                return Ok();
            }
            catch
            {
            }

            return StatusCode(500);
        }

        #region STATIC WEB PAGES

        /// <summary>
        /// Returns if a user has not been granted access yet.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public IActionResult AccessPending()
        {
            var model = new ArticleViewModel
            {
                Id = Guid.NewGuid(),
                ArticleNumber = 0,
                UrlPath = null,
                VersionNumber = 0,
                Published = null,
                Title = "Access Pending",
                Content = null,
                Updated = default,
                HeadJavaScript = null,
                FooterJavaScript = null,
                Layout = null,
                ReadWriteMode = false,
                PreviewMode = false,
                EditModeOn = false
            };
            return View(model);
        }

        #endregion

        #region API

        /// <summary>
        /// Gets the children of a given page path.
        /// </summary>
        /// <param name="page">UrlPath</param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderByPub"></param>
        /// <returns></returns>
        [EnableCors("AllCors")]
        public async Task<IActionResult> GetTOC(
            string page,
            bool? orderByPub,
            int? pageNo,
            int? pageSize)
        {
            var result = await _articleLogic.GetTOC(page, pageNo ?? 0, pageSize ?? 10, orderByPub ?? false);
            return Json(result);
        }

        ///// <summary>
        ///// Returns a proxy result as a <see cref="JsonResult"/>.
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public async Task<IActionResult> SimpleProxyJson(string id)
        //{
        //    if (_proxyConfigs.Value == null) return Json(string.Empty);
        //    var proxy = new SimpleProxyService(_proxyConfigs);
        //    return Json(await proxy.CallEndpoint(id, new UserIdentityInfo(User)));
        //}

        ///// <summary>
        ///// Returns a proxy as a simple string.
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public async Task<string> SimpleProxy(string id)
        //{
        //    if (_proxyConfigs.Value == null) return string.Empty;
        //    var proxy = new SimpleProxyService(_proxyConfigs);
        //    return await proxy.CallEndpoint(id, new UserIdentityInfo(User));
        //}

        #endregion
    }
}