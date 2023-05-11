﻿using Cosmos.Cms.Common.Services.Configurations;
using Cosmos.Cms.Data.Logic;
using Cosmos.Cms.Models;
using Cosmos.Cms.Services;
using Cosmos.Common.Data;
using Cosmos.Common.Data.Logic;
using Cosmos.Common.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Cms.Controllers
{
    /// <summary>
    /// Editor controller
    /// </summary>
    //[ResponseCache(NoStore = true)]
    [Authorize(Roles = "Reviewers, Administrators, Editors, Authors")]
    public class EditorController : BaseController
    {
        private readonly ArticleEditLogic _articleLogic;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<EditorController> _logger;
        private readonly IOptions<CosmosConfig> _options;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly Uri _blobPublicAbsoluteUrl;
        private readonly IViewRenderService _viewRenderService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dbContext"></param>
        /// <param name="userManager"></param>
        /// <param name="articleLogic"></param>
        /// <param name="options"></param>
        /// <param name="viewRenderService"></param>
        public EditorController(ILogger<EditorController> logger,
            ApplicationDbContext dbContext,
            UserManager<IdentityUser> userManager,
            ArticleEditLogic articleLogic,
            IOptions<CosmosConfig> options,
            IViewRenderService viewRenderService
        ) :
             base(dbContext, userManager, articleLogic, options)
        {
            _logger = logger;
            _dbContext = dbContext;
            _options = options;
            _userManager = userManager;
            _articleLogic = articleLogic;


            var htmlUtilities = new HtmlUtilities();

            if (htmlUtilities.IsAbsoluteUri(options.Value.SiteSettings.BlobPublicUrl))
            {
                _blobPublicAbsoluteUrl = new Uri(options.Value.SiteSettings.BlobPublicUrl);
            }
            else
            {
                _blobPublicAbsoluteUrl = new Uri(options.Value.SiteSettings.PublisherUrl.TrimEnd('/') + "/" + options.Value.SiteSettings.BlobPublicUrl.TrimStart('/'));
            }

            _viewRenderService = viewRenderService;
        }

        /// <summary>
        ///     Disposes of resources for this controller.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #region LIST METHODS

        /// <summary>
        /// Catalog of web pages on this website.
        /// </summary>
        /// <param name="sortOrder"></param>
        /// <param name="currentSort"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<IActionResult> Index(string sortOrder, string currentSort, int pageNo = 0, int pageSize = 10, string filter = "")
        {
            ViewData["HomePageArticleNumber"] = await _dbContext.Pages.Where(f => f.UrlPath == "root").Select(s => s.ArticleNumber).FirstOrDefaultAsync();
            ViewData["PublisherUrl"] = _options.Value.SiteSettings.PublisherUrl;
            ViewData["ShowFirstPageBtn"] = await _dbContext.Articles.CosmosAnyAsync() == false;
            ViewData["ShowNotFoundBtn"] = await _dbContext.ArticleCatalog.Where(w => w.UrlPath == "not_found").CosmosAnyAsync() == false;

            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.TrimStart('/');
            }

            ViewData["Filter"] = filter;

            ViewData["sortOrder"] = sortOrder;
            ViewData["currentSort"] = currentSort;
            ViewData["pageNo"] = pageNo;
            ViewData["pageSize"] = pageSize;


            var query = _dbContext.ArticleCatalog.AsQueryable();

            ViewData["RowCount"] = await query.CountAsync();

            if (!string.IsNullOrEmpty(filter))
            {
                var f = filter.ToLower();
                query = query.Where(w => w.Title.ToLower().Contains(f));
            }

            if (sortOrder == "desc")
            {
                if (!string.IsNullOrEmpty(currentSort))
                {
                    switch (currentSort)
                    {
                        case "ArticleNumber":
                            query = query.OrderByDescending(o => o.ArticleNumber);
                            break;
                        case "Title":
                            query = query.OrderByDescending(o => o.Title);
                            break;
                        case "LastPublished":
                            query = query.OrderByDescending(o => o.Published);
                            break;
                        case "UrlPath":
                            query = query.OrderByDescending(o => o.UrlPath);
                            break;
                        case "Status":
                            query = query.OrderByDescending(o => o.Status);
                            break;
                        case "Updated":
                            query = query.OrderByDescending(o => o.Updated);
                            break;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(currentSort))
                {
                    switch (currentSort)
                    {
                        case "ArticleNumber":
                            query = query.OrderBy(o => o.ArticleNumber);
                            break;
                        case "Title":
                            query = query.OrderBy(o => o.Title);
                            break;
                        case "LastPublished":
                            query = query.OrderBy(o => o.Published);
                            break;
                        case "UrlPath":
                            query = query.OrderBy(o => o.UrlPath);
                            break;
                        case "Status":
                            query = query.OrderBy(o => o.Status);
                            break;
                        case "Updated":
                            query = query.OrderBy(o => o.Updated);
                            break;
                    }
                }
                else
                {
                    // Default sort order
                    query = query.OrderBy(o => o.Title);
                }
            }

            var model = query.Select(s => new ArticleListItem()
            {
                ArticleNumber = s.ArticleNumber,
                Title = s.Title,
                IsDefault = s.UrlPath == "root",
                LastPublished = s.Published,
                UrlPath = s.UrlPath,
                Status = s.Status,
                Updated = s.Updated
            }).Skip(pageNo * pageSize).Take(pageSize);

            var data = await model.ToListAsync();

            return View(data);
        }

        ///<summary>
        ///     Gets all the versions for an article
        /// </summary>
        /// <param name="id">Article number</param>
        /// <param name="sortOrder"></param>
        /// <param name="currentSort"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<IActionResult> Versions(int? id, string sortOrder = "desc", string currentSort = "VersionNumber", int pageNo = 0, int pageSize = 10)
        {
            if (id == null)
                return RedirectToAction("Index");

            ViewData["sortOrder"] = sortOrder;
            ViewData["currentSort"] = currentSort;
            ViewData["pageNo"] = pageNo;
            ViewData["pageSize"] = pageSize;
            ViewData["articleNumber"] = id;

            var query = _dbContext.Articles.Where(w => w.ArticleNumber == id)
                .Select(s => new ArticleVersionViewModel()
                {
                    Id = s.Id,
                    Published = s.Published,
                    Title = s.Title,
                    Updated = s.Updated,
                    VersionNumber = s.VersionNumber,
                    Expires = s.Expires,
                    UsesHtmlEditor = s.Content.ToLower().Contains(" contenteditable=") || s.Content.ToLower().Contains(" data-ccms-ceid=")
                }).AsQueryable();

            ViewData["RowCount"] = await query.CountAsync();


            if (sortOrder == "desc")
            {
                if (!string.IsNullOrEmpty(currentSort))
                {
                    switch (currentSort)
                    {
                        case "Published":
                            query = query.OrderByDescending(o => o.Published);
                            break;
                        case "Updated":
                            query = query.OrderByDescending(o => o.Updated);
                            break;
                        case "VersionNumber":
                            query = query.OrderByDescending(o => o.VersionNumber);
                            break;
                        case "Expires":
                            query = query.OrderByDescending(o => o.Expires);
                            break;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(currentSort))
                {
                    switch (currentSort)
                    {
                        case "Published":
                            query = query.OrderBy(o => o.Published);
                            break;
                        case "Updated":
                            query = query.OrderBy(o => o.Updated);
                            break;
                        case "VersionNumber":
                            query = query.OrderBy(o => o.VersionNumber);
                            break;
                        case "Expires":
                            query = query.OrderBy(o => o.Expires);
                            break;
                    }
                }
            }

            var article = await _dbContext.Articles.Where(a => a.ArticleNumber == id.Value)
                .Select(s => new { s.Title, s.VersionNumber }).FirstOrDefaultAsync();

            ViewData["ArticleTitle"] = article.Title;
            ViewData["ArticleId"] = id.Value;

            return View(await query.Skip(pageNo * pageSize).Take(pageSize).ToListAsync());
        }

        /// <summary>
        /// Open trash
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrators, Editors, Authors")]
        public async Task<IActionResult> Trash(string sortOrder, string currentSort, int pageNo = 0, int pageSize = 10)
        {
            ViewData["sortOrder"] = sortOrder;
            ViewData["currentSort"] = currentSort;
            ViewData["pageNo"] = pageNo;
            ViewData["pageSize"] = pageSize;

            var data = await _articleLogic.GetArticleTrashList();
            var query = data.AsQueryable();

            ViewData["RowCount"] = query.Count();


            if (sortOrder == "desc")
            {
                if (!string.IsNullOrEmpty(currentSort))
                {
                    switch (currentSort)
                    {
                        case "ArticleNumber":
                            query = query.OrderByDescending(o => o.ArticleNumber);
                            break;
                        case "Title":
                            query = query.OrderByDescending(o => o.Title);
                            break;
                        case "LastPublished":
                            query = query.OrderByDescending(o => o.LastPublished);
                            break;
                        case "UrlPath":
                            query = query.OrderByDescending(o => o.UrlPath);
                            break;
                        case "Status":
                            query = query.OrderByDescending(o => o.Status);
                            break;
                        case "Updated":
                            query = query.OrderByDescending(o => o.Updated);
                            break;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(currentSort))
                {
                    switch (currentSort)
                    {
                        case "ArticleNumber":
                            query = query.OrderBy(o => o.ArticleNumber);
                            break;
                        case "Title":
                            query = query.OrderBy(o => o.Title);
                            break;
                        case "LastPublished":
                            query = query.OrderBy(o => o.LastPublished);
                            break;
                        case "UrlPath":
                            query = query.OrderBy(o => o.UrlPath);
                            break;
                        case "Status":
                            query = query.OrderBy(o => o.Status);
                            break;
                        case "Updated":
                            query = query.OrderBy(o => o.Updated);
                            break;
                    }
                }
            }

            return View(query.Skip(pageNo * pageSize).Take(pageSize).ToList());
        }
        #endregion

        /// <summary>
        /// Compare two versions.
        /// </summary>
        /// <param name="leftId"></param>
        /// <param name="rightId"></param>
        /// <returns></returns>
        public async Task<IActionResult> Compare(Guid leftId, Guid rightId)
        {

            var left = await _articleLogic.Get(leftId, EnumControllerName.Edit, await GetUserId());
            var right = await _articleLogic.Get(rightId, EnumControllerName.Edit, await GetUserId());
            @ViewData["PageTitle"] = left.Title;

            ViewData["LeftVersion"] = left.VersionNumber;
            ViewData["RightVersion"] = right.VersionNumber;

            var model = new CompareCodeViewModel()
            {
                EditorTitle = left.Title,
                EditorFields = new[]
                {
                    new EditorField
                    {
                        FieldId = "HeadJavaScript",
                        FieldName = "Head Block",
                        EditorMode = EditorMode.Html,
                        IconUrl = "/images/seti-ui/icons/html.svg",
                        ToolTip = "Content to appear at the bottom of the <head> tag."
                    },
                    new EditorField
                    {
                        FieldId = "Content",
                        FieldName = "Html Content",
                        EditorMode = EditorMode.Html,
                        IconUrl = "~/images/seti-ui/icons/html.svg",
                        ToolTip = "Content to appear in the <body>."
                    },
                    new EditorField
                    {
                        FieldId = "FooterJavaScript",
                        FieldName = "Footer Block",
                        EditorMode = EditorMode.Html,
                        IconUrl = "~/images/seti-ui/icons/html.svg",
                        ToolTip = "Content to appear at the bottom of the <body> tag."
                    }
                },
                Articles = new ArticleViewModel[] { left, right }
            };
            return View(model);
        }

        /// <summary>
        /// Gets template page information.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrators, Editors, Authors, Team Members")]
        public async Task<IActionResult> GetTemplateInfo(Guid? Id)
        {
            if (Id == null)
                return Json("");

            var model = await _dbContext.Templates.FirstOrDefaultAsync(f => f.Id == Id.Value);

            return Json(model);
        }

        /// <summary>
        ///     Creates a <see cref="CreatePageViewModel" /> used to create a new article.
        /// </summary>
        /// <param name="title">Name of new page if known</param>
        /// <param name="sortOrder"></param>
        /// <param name="currentSort"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrators, Editors, Authors, Team Members")]
        public async Task<IActionResult> Create(string title = "", string sortOrder = "asc", string currentSort = "Title", int pageNo = 0, int pageSize = 20)
        {
            var defautLayout = await _dbContext.Layouts.FirstOrDefaultAsync(l => l.IsDefault);

            ViewData["Layouts"] = await BaseGetLayoutListItems();

            ViewData["sortOrder"] = sortOrder;
            ViewData["currentSort"] = currentSort;
            ViewData["pageNo"] = pageNo;
            ViewData["pageSize"] = pageSize;

            var query = _dbContext.Templates.OrderBy(t => t.Title)
                .Where(w => w.LayoutId == defautLayout.Id)
                .Select(s => new TemplateIndexViewModel
                {
                    Id = s.Id,
                    LayoutName = defautLayout.LayoutName,
                    Description = s.Description,
                    Title = s.Title,
                    UsesHtmlEditor = s.Content.ToLower().Contains(" contenteditable=") || s.Content.ToLower().Contains(" data-ccms-ceid=")
                }).AsQueryable();

            ViewData["RowCount"] = await query.CountAsync();


            if (sortOrder == "desc")
            {
                if (!string.IsNullOrEmpty(currentSort))
                {
                    switch (currentSort)
                    {
                        case "LayoutName":
                            query = query.OrderByDescending(o => o.LayoutName);
                            break;
                        case "Description":
                            query = query.OrderByDescending(o => o.Description);
                            break;
                        case "Title":
                            query = query.OrderByDescending(o => o.Title);
                            break;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(currentSort))
                {
                    switch (currentSort)
                    {
                        case "LayoutName":
                            query = query.OrderBy(o => o.LayoutName);
                            break;
                        case "Title":
                            query = query.OrderBy(o => o.Title);
                            break;
                        case "Description":
                            query = query.OrderBy(o => o.Description);
                            break;
                    }
                }
            }

            ViewData["TemplateList"] = await query.Skip(pageNo * pageSize).Take(pageSize).ToListAsync();

            return View(new CreatePageViewModel()
            {
                Id = Guid.NewGuid(),
                Title = title
            });
        }

        /// <summary>
        ///     Uses <see cref="ArticleEditLogic.Create(string, string, Guid?)" /> to create an <see cref="ArticleViewModel" /> that is
        ///     saved to
        ///     the database with <see cref="ArticleEditLogic.Save(ArticleViewModel, string)" /> ready for editing.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrators, Editors, Authors, Team Members")]
        [HttpPost]
        public async Task<IActionResult> Create(CreatePageViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model == null) return NotFound();

                model.Title = model.Title.TrimStart('/');


                var validTitle = await _articleLogic.ValidateTitle(model.Title, null);

                if (!validTitle)
                {
                    ModelState.AddModelError("Title", $"Title: {model.Title} conflicts with another article title or reserved word.");
                    return View(model);
                }

                var article = await _articleLogic.Create(model.Title, await GetUserId(), model.TemplateId);

                return RedirectToAction("Versions", "Editor", new { Id = article.ArticleNumber });
            }

            var defautLayout = await _dbContext.Layouts.FirstOrDefaultAsync(l => l.IsDefault);

            ViewData["Layouts"] = await BaseGetLayoutListItems();

            ViewData["sortOrder"] = "asc";
            ViewData["currentSort"] = "title";
            ViewData["pageNo"] = 0;
            ViewData["pageSize"] = 20;

            var query = _dbContext.Templates.OrderBy(t => t.Title)
               .Where(w => w.LayoutId == defautLayout.Id)
               .Select(s => new TemplateIndexViewModel
               {
                   Id = s.Id,
                   LayoutName = defautLayout.LayoutName,
                   Description = s.Description,
                   Title = s.Title,
                   UsesHtmlEditor = s.Content.ToLower().Contains(" contenteditable=") || s.Content.ToLower().Contains(" data-ccms-ceid=")
               }).AsQueryable();

            ViewData["RowCount"] = await query.CountAsync();
            ViewData["TemplateList"] = await query.Skip(0 * 20).Take(20).ToListAsync();

            return View(model);
        }

        /// <summary>
        ///     Creates a new version for an article and redirects to editor.
        /// </summary>
        /// <param name="id">Article ID</param>
        /// <param name="entityId">Entity Id to use as new version</param>
        /// <returns></returns>
        [Authorize(Roles = "Administrators, Editors, Authors")]
        public async Task<IActionResult> CreateVersion(int id, Guid? entityId = null)
        {
            IQueryable<Article> query;

            //
            // Are we basing this on an existing entity?
            //
            if (entityId == null)
            {
                //
                // If here, we are not. Clone the new version from the last version.
                //
                // Find the last version here
                var maxVersion = await _dbContext.Articles.Where(a => a.ArticleNumber == id)
                    .MaxAsync(m => m.VersionNumber);

                //
                // Now find that version.
                //
                query = _dbContext.Articles.Where(f =>
                    f.ArticleNumber == id &&
                    f.VersionNumber == maxVersion);
            }
            else
            {
                //
                // We are here because the new version is being based on a
                // specific older version, not the latest version.
                //
                //
                // Create a new version based on a specific version
                //
                query = _dbContext.Articles.Where(f =>
                    f.Id == entityId.Value);
            }

            var article = await query.FirstOrDefaultAsync();

            var model = new ArticleViewModel
            {
                Id = article.Id, // This is the article we are going to clone as a new version.
                StatusCode = StatusCodeEnum.Active,
                ArticleNumber = article.ArticleNumber,
                UrlPath = article.UrlPath,
                VersionNumber = 0,
                Published = null,
                Title = article.Title,
                Content = article.Content,
                Updated = article.Updated,
                HeadJavaScript = article.HeaderJavaScript,
                FooterJavaScript = article.FooterJavaScript,
                ReadWriteMode = false,
                PreviewMode = false,
                EditModeOn = false,
                CacheKey = null,
                CacheDuration = 0
            };

            var userId = _userManager.GetUserId(User);

            var result = await _articleLogic.Save(model, userId);

            return RedirectToAction("EditCode", "Editor", new { result.Model.Id });
        }

        /// <summary>
        /// Create a duplicate page from a specified page.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Clone(int id)
        {
            var lastVersion = await _dbContext.Articles.Where(a => a.ArticleNumber == id).MaxAsync(m => m.VersionNumber);

            var articleViewModel = await _articleLogic.Get(id, lastVersion);

            ViewData["Original"] = articleViewModel;

            if (articleViewModel == null)
            {
                return NotFound();
            }

            var model = new DuplicateViewModel()
            {
                Id = articleViewModel.Id,
                Published = articleViewModel.Published,
                Title = articleViewModel.Title,
                ArticleId = articleViewModel.ArticleNumber,
                ArticleVersion = articleViewModel.VersionNumber
            };

            return View(model);
        }

        /// <summary>
        /// Creates a duplicate page from a specified page and version.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators, Editors, Authors")]
        public async Task<IActionResult> Clone(DuplicateViewModel model)
        {
            string title = "";

            if (string.IsNullOrEmpty(model.ParentPageTitle))
            {
                title = model.Title;
            }
            else
            {
                title = $"{model.ParentPageTitle.Trim('/')}/{model.Title.Trim('/')} ";
            }

            if (await _dbContext.Articles.Where(a => a.Title.ToLower() == title.ToLower()).CosmosAnyAsync())
            {
                if (string.IsNullOrEmpty(model.ParentPageTitle))
                {
                    ModelState.AddModelError("Title", "Page title already taken.");
                }
                else
                {
                    ModelState.AddModelError("Title", "Sub-page title already taken.");
                }
            }

            var userId = await GetUserId();

            var articleViewModel = await _articleLogic.Get(model.Id, EnumControllerName.Edit, userId);

            if (ModelState.IsValid)
            {
                articleViewModel.ArticleNumber = 0;
                articleViewModel.Id = Guid.NewGuid();
                articleViewModel.Published = model.Published;
                articleViewModel.Title = title;

                try
                {
                    var clone = await _articleLogic.Create(articleViewModel.Title, userId);
                    clone.RoleList = articleViewModel.RoleList;
                    clone.StatusCode = articleViewModel.StatusCode;
                    clone.CacheDuration = articleViewModel.CacheDuration;
                    clone.Content = articleViewModel.Content;
                    clone.FooterJavaScript = articleViewModel.FooterJavaScript;
                    clone.HeadJavaScript = articleViewModel.HeadJavaScript;
                    clone.LanguageCode = articleViewModel.LanguageCode;

                    var result = await _articleLogic.Save(clone, userId);


                    // Open the live editor if there are editable regions on the page.
                    if (result.Model.Content.Contains("editable", StringComparison.InvariantCultureIgnoreCase) ||
                        result.Model.Content.Contains("data-ccms-ceid", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return RedirectToAction("Edit", new { result.Model.Id });
                    }

                    // Otherwise, open in the Monaco code editor
                    return RedirectToAction("EditCode", new { result.Model.Id });
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.Message);
                }
            }

            ViewData["Original"] = articleViewModel;

            return View(model);
        }

        /// <summary>
        ///     Creates a <see cref="CreatePageViewModel" /> used to create a new article.
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrators, Editors")]
        public async Task<IActionResult> NewHome(int id)
        {
            var page = await _dbContext.Articles.FirstOrDefaultAsync(f => f.ArticleNumber == id);
            return View(new NewHomeViewModel
            {
                Id = page.Id,
                ArticleNumber = page.ArticleNumber,
                Title = page.Title,
                IsNewHomePage = false,
                UrlPath = page.UrlPath
            });
        }

        /// <summary>
        /// Make a web page the new home page
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrators, Editors")]
        public async Task<IActionResult> NewHome(NewHomeViewModel model)
        {
            if (model == null) return NotFound();
            await _articleLogic.NewHomePage(model, _userManager.GetUserId(User));

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Recovers an article from trash
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrators, Editors, Authors")]
        public async Task<IActionResult> Recover(int Id)
        {
            await _articleLogic.RetrieveFromTrash(Id, await GetUserId());

            return RedirectToAction("Trash");
        }

        /// <summary>
        ///     Publishes a website.
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrators, Editors")]
        public IActionResult Publish()
        {
            return View();
        }

        /// <summary>
        /// Open Cosmos logs
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrators, Editors")]
        public async Task<IActionResult> Logs()
        {
            var data = await _dbContext.ArticleLogs
                .OrderByDescending(o => o.DateTimeStamp)
                .Select(s => new
                {
                    s.Id,
                    s.ActivityNotes,
                    s.DateTimeStamp,
                    s.IdentityUserId
                }).ToListAsync();

            var model = data.Select(s => new ArticleLogJsonModel
            {
                Id = s.Id,
                ActivityNotes = s.ActivityNotes,
                DateTimeStamp = s.DateTimeStamp.ToUniversalTime(),
                IdentityUserId = s.IdentityUserId
            }).AsQueryable();

            return View(model);
        }

        #region RESERVED PATH MANAGEMENT

        /// <summary>
        /// Gets a reserved path list
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ReservedPaths(string sortOrder, string currentSort, int pageNo = 0, int pageSize = 10, string filter = "")
        {

            var paths = await _articleLogic.GetReservedPaths();

            ViewData["RowCount"] = paths.Count();

            var query = paths.AsQueryable();

            ViewData["Filter"] = filter;
            ViewData["sortOrder"] = sortOrder;
            ViewData["currentSort"] = currentSort;
            ViewData["pageNo"] = pageNo;
            ViewData["pageSize"] = pageSize;


            if (!string.IsNullOrEmpty(filter))
            {
                var f = filter.ToLower();
                query = query.Where(w => w.Path.ToLower().Contains(f));
            }

            if (sortOrder == "desc")
            {
                if (!string.IsNullOrEmpty(currentSort))
                {
                    switch (currentSort)
                    {
                        case "Path":
                            query = query.OrderByDescending(o => o.Path);
                            break;
                        case "CosmosRequired":
                            query = query.OrderByDescending(o => o.CosmosRequired);
                            break;
                        case "Notes":
                            query = query.OrderByDescending(o => o.Notes);
                            break;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(currentSort))
                {
                    switch (currentSort)
                    {
                        case "Path":
                            query = query.OrderBy(o => o.Path);
                            break;
                        case "CosmosRequired":
                            query = query.OrderBy(o => o.CosmosRequired);
                            break;
                        case "Notes":
                            query = query.OrderBy(o => o.Notes);
                            break;
                    }
                }
                else
                {
                    // Default sort order
                    query = query.OrderBy(o => o.Path);
                }
            }

            query = query.Skip(pageNo * pageSize).Take(pageSize);

            return View(query.ToList());
        }

        /// <summary>
        /// Creates a new reserved path
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CreateReservedPath()
        {
            ViewData["Title"] = "Create a Reserved Path";

            return View("~/Views/Editor/EditReservedPath.cshtml", new ReservedPath());
        }

        /// <summary>
        /// Creates a new reserved path
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateReservedPath(ReservedPath model)
        {
            ViewData["Title"] = "Create a Reserved Path";

            if (ModelState.IsValid)
            {
                try
                {
                    await _articleLogic.AddOrUpdateReservedPath(model);
                    return RedirectToAction("ReservedPaths");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("Path", e.Message);
                }
            }
            return View("~/Views/Editor/EditReservedPath.cshtml", model);
        }

        /// <summary>
        /// Edit a reserved path
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditReservedPath(Guid Id)
        {
            ViewData["Title"] = "Edit Reserved Path";

            var paths = await _articleLogic.GetReservedPaths();

            var path = paths.FirstOrDefault(f => f.Id == Id);

            if (path == null)
            {
                return NotFound();
            }

            return View(path);
        }

        /// <summary>
        /// Edit an existing reserved path
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditReservedPath(ReservedPath model)
        {
            ViewData["Title"] = "Edit Reserved Path";

            if (ModelState.IsValid)
            {
                try
                {
                    await _articleLogic.AddOrUpdateReservedPath(model);
                    return RedirectToAction("ReservedPaths");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("Path", e.Message);
                }
            }
            return View(model);
        }

        /// <summary>
        /// Removes a reerved path
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<IActionResult> RemoveReservedPath(Guid Id)
        {
            try
            {
                await _articleLogic.RemoveReservedPath(Id);
                return RedirectToAction("ReservedPaths");
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Path", e.Message);
            }

            return RedirectToAction("ReservedPaths");
        }

        #endregion

        #region SAVING CONTENT METHODS

        #endregion

        #region EDIT ARTICLE FUNCTIONS


        /// <summary>
        /// Editor page
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<IActionResult> CcmsContent(Guid Id)
        {
            var article = await _articleLogic.Get(Id, EnumControllerName.Edit, await GetUserId());

            return View(article);
        }

        #region HTML AND CODE EDITOR METHODS

        /// <summary>
        /// CKEditor collaboration token maker
        /// </summary>
        /// <param name="environmentId"></param>
        /// <param name="accessKey"></param>
        /// <returns></returns>
        /// <remarks><see href="https://ckeditor.com/docs/cs/latest/examples/token-endpoints/dotnet.html"/></remarks>
        private async Task<string> CreateCSToken(string environmentId, string accessKey)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(accessKey));

            var signingCredentials = new SigningCredentials(securityKey, "HS256");
            var header = new JwtHeader(signingCredentials);

            var dateTimeOffset = new DateTimeOffset(DateTime.UtcNow);

            var payload = new JwtPayload
            {
                { "aud", environmentId },
                { "iat", dateTimeOffset.ToUnixTimeSeconds() },
                { "sub", await GetUserId() },
                { "user", new Dictionary<string, string> {
                    { "email", await GetUserEmail() }
                } },
                { "auth", new Dictionary<string, object> {
                    { "collaboration", new Dictionary<string, object> {
                        { "*", new Dictionary<string, string> {
                            { "role", "writer" }
                        } }
                    } }
                } }
            };

            var securityToken = new JwtSecurityToken(header, payload);
            var handler = new JwtSecurityTokenHandler();

            return handler.WriteToken(securityToken);
        }

        /// <summary>
        ///     Gets an article to edit by ID for the HTML (WYSIWYG) Editor.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrators, Editors, Authors, Team Members")]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                // Web browser may ask for favicon.ico, so if the ID is not a number, just skip the response.
                if (Guid.TryParse(id, out var pageId))
                {
                    ViewData["BlobEndpointUrl"] = _options.Value.SiteSettings.BlobPublicUrl;

                    //
                    // Get an article, or a template based on the controller name.
                    //
                    var model = await _articleLogic.Get(pageId, EnumControllerName.Edit, await GetUserId());
                    ViewData["LastPubDateTime"] = await GetLastPublishingDate(model.ArticleNumber);

                    ViewData["PageTitle"] = model.Title;
                    ViewData["Published"] = model.Published;

                    // Override defaults
                    model.EditModeOn = true;

                    // Authors cannot edit published articles
                    if (model.Published.HasValue && User.IsInRole("Authors"))
                        return Unauthorized();

                    return View(new HtmlEditorViewModel(model));

                }

                return NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                throw;
            }
        }

        /// <summary>
        /// Updates editable regions
        /// </summary>
        /// <param name="model"></param>
        /// <remarks>FromBody is used because the jQuery call puts the JSON in the body, not the "Form" as this is a JSON content type.</remarks>
        /// <returns></returns>
        //[HttpPost]
        //[Authorize(Roles = "Administrators, Editors, Authors, Team Members")]
        //public async Task<IActionResult> PostRegions([FromBody] HtmlEditorPost model)
        //{
        //    var saveError = new StringBuilder();

        //    try
        //    {
        //        // Next pull the original. This is a view model, not tracked by DbContext.
        //        var article = await _articleLogic.Get(model.Id, EnumControllerName.Edit, await GetUserId());

        //        if (article == null)
        //        {
        //            return NotFound();
        //        }

        //        // The Live editor edits the title and Content fields.
        //        // Next two lines detect any HTML errors with each.
        //        // Errors are saved in ModelState.
        //        model.Title = BaseValidateHtml("Title", model.Title);

        //        if (ModelState.IsValid)
        //        {
        //            // Get the editable regions from the original document.
        //            var originalHtmlDoc = new HtmlDocument();
        //            originalHtmlDoc.LoadHtml(article.Content);
        //            var originalEditableDivs = originalHtmlDoc.DocumentNode.SelectNodes("//*[@data-ccms-ceid]");

        //            foreach (var region in model.Regions)
        //            {
        //                var target = originalEditableDivs.FirstOrDefault(w => w.Attributes["data-ccms-ceid"].Value == region.Id);
        //                if (target != null)
        //                {
        //                    target.InnerHtml = region.Html;
        //                }
        //            }

        //            // Now carry over what's being updated to the original.
        //            article.Content = originalHtmlDoc.DocumentNode.OuterHtml;
        //            article.Title = model.Title;
        //            article.Published = model.Published;
        //            article.BannerImage = model.BannerImage;

        //            // Make sure we are setting to the orignal updated date/time
        //            // This is validated to make sure that someone else hasn't already edited this
        //            // entity
        //            article.Updated = model.Updated.Value;

        //            var result = await _articleLogic.Save(article, await GetUserId());

        //            var data = new HtmlEditorPost()
        //            {
        //                Title = result.Model.Title,
        //                Published = result.Model.Published,
        //                Updated = result.Model.Updated,
        //                ArticleNumber = result.Model.ArticleNumber,
        //                Content = result.Model.Content,
        //                Id = result.Model.Id,
        //                Regions = model.Regions,
        //                RoleList = result.Model.RoleList,
        //                UpdateExisting = model.UpdateExisting,
        //                UrlPath = result.Model.UrlPath,
        //                VersionNumber = result.Model.VersionNumber,
        //                BannerImage = result.Model.BannerImage
        //            };

        //            return Json(data);
        //        }

        //        saveError.AppendLine("Error(s):");
        //        saveError.AppendLine("<ul>");

        //        var errors = ModelState.Values.Where(w => w.ValidationState == ModelValidationState.Invalid).ToList();

        //        foreach (var error in errors)
        //        {
        //            foreach (var e in error.Errors)
        //            {
        //                saveError.AppendLine("<li>" + e.ErrorMessage + "</li>");
        //            }
        //        }

        //        saveError.AppendLine("</ul>");
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError(e.Message, e);
        //        saveError.AppendLine("<ul><li>An error occurred while saving.</li></ul>");
        //    }

        //    return StatusCode(StatusCodes.Status500InternalServerError, saveError.ToString());
        //}

        /// <summary>
        /// Edit web page code with Monaco editor.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrators, Editors, Authors, Team Members")]
        public async Task<IActionResult> EditCode(Guid id)
        {
            var article = await _articleLogic.Get(id, EnumControllerName.Edit, await GetUserId());
            if (article == null) return NotFound();

            // Validate security for authors before going further
            if (article.Published.HasValue && User.IsInRole("Authors"))
                return Unauthorized();

            ViewData["Version"] = article.VersionNumber;

            ViewData["PageTitle"] = article.Title;
            ViewData["Published"] = article.Published;
            ViewData["LastPubDateTime"] = await GetLastPublishingDate(article.ArticleNumber);

            return View(new EditCodePostModel
            {
                Id = article.Id,
                ArticleNumber = article.ArticleNumber,
                Title = article.Title,
                Published = article.Published,
                RoleList = article.RoleList,
                EditorTitle = article.Title,
                UrlPath = article.UrlPath,
                BannerImage = article.BannerImage,
                Updated = article.Updated,
                EditorFields = new[]
                {
                    new EditorField
                    {
                        FieldId = "HeadJavaScript",
                        FieldName = "Head Block",
                        EditorMode = EditorMode.Html,
                        IconUrl = "/images/seti-ui/icons/html.svg",
                        ToolTip = "Content to appear at the bottom of the <head> tag."
                    },
                    new EditorField
                    {
                        FieldId = "Content",
                        FieldName = "Html Content",
                        EditorMode = EditorMode.Html,
                        IconUrl = "~/images/seti-ui/icons/html.svg",
                        ToolTip = "Content to appear in the <body>."
                    },
                    new EditorField
                    {
                        FieldId = "FooterJavaScript",
                        FieldName = "Footer Block",
                        EditorMode = EditorMode.Html,
                        IconUrl = "~/images/seti-ui/icons/html.svg",
                        ToolTip = "Content to appear at the bottom of the <body> tag."
                    }
                },
                HeadJavaScript = article.HeadJavaScript,
                FooterJavaScript = article.FooterJavaScript,
                Content = article.Content,
                EditingField = "HeadJavaScript",
                CustomButtons = new[] { "Preview", "Html", "Export", "Import" }
            });
        }

        /// <summary>
        ///     Saves the code and html of the page.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        ///     This method saves page code to the database.  <see cref="EditCodePostModel.Content" /> is validated using method
        ///     <see cref="BaseController.BaseValidateHtml" />.
        ///     HTML formatting errors that could not be automatically fixed are logged with
        ///     <see cref="ControllerBase.ModelState" /> and
        ///     the code is not saved in the database.
        /// </remarks>
        /// <exception cref="NotFoundResult"></exception>
        /// <exception cref="UnauthorizedResult"></exception>
        [HttpPost]
        [Authorize(Roles = "Administrators, Editors, Authors, Team Members")]
        public async Task<IActionResult> EditCode(EditCodePostModel model)
        {
            var saveError = new StringBuilder();

            if (model == null) return NotFound();

            // Get the user's ID for logging.
            var userId = await GetUserId();

            var article = await _dbContext.Articles.FirstOrDefaultAsync(f => f.Id == model.Id);

            if (article == null) return NotFound();

            _dbContext.Entry(article).State = EntityState.Detached;

            ViewData["Version"] = article.VersionNumber;

            var jsonModel = new SaveCodeResultJsonModel();

            if (ModelState.IsValid)
            {

                try
                {
                    var result = await _articleLogic.Save(new ArticleViewModel()
                    {
                        Id = model.Id,
                        ArticleNumber = article.ArticleNumber,
                        BannerImage = article.BannerImage,
                        Content = model.Content,
                        Title = model.Title,
                        RoleList = model.RoleList,
                        Published = model.Published,
                        Expires = article.Expires,
                        FooterJavaScript = model.FooterJavaScript,
                        HeadJavaScript = model.HeadJavaScript,
                        StatusCode = (StatusCodeEnum)article.StatusCode,
                        UrlPath = article.UrlPath,
                        VersionNumber = article.VersionNumber,
                        Updated = model.Updated.Value,
                    }, userId);

                    jsonModel.Model = new EditCodePostModel()
                    {
                        ArticleNumber = result.Model.ArticleNumber,
                        BannerImage = result.Model.BannerImage,
                        Content = result.Model.Content,
                        EditingField = model.EditingField,
                        CustomButtons = model.CustomButtons,
                        EditorMode = model.EditorMode,
                        EditorFields = model.EditorFields,
                        EditorTitle = model.EditorTitle,
                        EditorType = model.EditorType,
                        FooterJavaScript = result.Model.FooterJavaScript,
                        HeadJavaScript = result.Model.HeadJavaScript,
                        UrlPath = result.Model.UrlPath,
                        Id = result.Model.Id,
                        Published = result.Model.Published,
                        RoleList = result.Model.RoleList,
                        Title = result.Model.Title,
                        Updated = result.Model.Updated
                    };

                }
                catch (Exception e)
                {
                    var provider = new EmptyModelMetadataProvider();
                    ModelState.AddModelError("Save", e, provider.GetMetadataForType(typeof(string)));
                    _logger.LogError(e.Message, e);
                }

                //
                jsonModel.ErrorCount = ModelState.ErrorCount;
                jsonModel.IsValid = ModelState.IsValid;

                jsonModel.Errors.AddRange(ModelState.Values
                    .Where(w => w.ValidationState == ModelValidationState.Invalid)
                    .ToList());
                jsonModel.ValidationState = ModelState.ValidationState;

                return Json(jsonModel);
            }

            saveError.AppendLine("Error(s):");
            saveError.AppendLine("<ul>");

            var errors = ModelState.Values.Where(w => w.ValidationState == ModelValidationState.Invalid).ToList();

            foreach (var error in errors)
            {
                foreach (var e in error.Errors)
                {
                    saveError.AppendLine("<li>" + e.ErrorMessage + "</li>");
                }
            }

            saveError.AppendLine("</ul>");

            return StatusCode(StatusCodes.Status500InternalServerError, saveError.ToString());
        }

        /// <summary>
        /// Gets the last date this article was published.
        /// </summary>
        /// <param name="articleNumber"></param>
        /// <returns></returns>
        private async Task<DateTimeOffset?> GetLastPublishingDate(int articleNumber)
        {
            return await _dbContext.Articles.Where(a => a.ArticleNumber == articleNumber).MaxAsync(m => m.Published);
        }

        /// <summary>
        /// Search and replace for all published pages.
        /// </summary>
        /// <returns></returns>
        public IActionResult SearchAndReplace()
        {
            return View();
        }

        /// <summary>
        /// Performs a query to see what pages will have changes.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SearchAndReplaceQuery(SearchAndReplaceViewModel model)
        {
            if (model.ArticleNumber.HasValue)
            {
                var articleCount = await _dbContext.Articles.Where(c => c.ArticleNumber == model.ArticleNumber && c.Content.Contains(model.FindValue)).CountAsync();

                ViewData["SearchAndReplacePrequery"] = $"{articleCount} versions will be modified.";
            }
            else
            {
                var articleCount = await _dbContext.Articles.Where(c => c.Published != null && c.Content.Contains(model.FindValue)).CountAsync();

                ViewData["SearchAndReplacePrequery"] = $"{articleCount} published articles will be modified.";
            }

            return View(model);
        }

        #endregion

        /// <summary>
        /// Exports a page as a file
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrators, Editors, Authors, Team Members")]
        public async Task<IActionResult> ExportPage(Guid? id)
        {
            ArticleViewModel article;
            var userId = await GetUserId();
            if (id.HasValue)
            {
                article = await _articleLogic.Get(id.Value, EnumControllerName.Edit, userId);
            }
            else
            {
                // Get the user's ID for logging.
                article = await _articleLogic.Create("Blank Page", userId);
            }

            var html = await _articleLogic.ExportArticle(article, _blobPublicAbsoluteUrl, _viewRenderService);

            var exportName = $"pageid-{article.ArticleNumber}-version-{article.VersionNumber}.html";

            var bytes = Encoding.UTF8.GetBytes(html);

            return File(bytes, "application/octet-stream", exportName);
        }

        /// <summary>
        /// Pre-load the website (useful if CDN configured).
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrators")]
        public IActionResult Preload()
        {
            return View(new PreloadViewModel());
        }

        ///// <summary>
        ///// Execute preload.
        ///// </summary>
        ///// <param name="model"></param>
        ///// <param name="primaryOnly"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[Authorize(Roles = "Administrators")]
        //public async Task<IActionResult> Preload(PreloadViewModel model, bool primaryOnly = false)
        //{
        //    var activeCode = (int)StatusCodeEnum.Active;
        //    var query = _dbContext.Articles.Where(p => p.Published != null && p.StatusCode == activeCode);
        //    var articleList = await _articleLogic.GetArticleList(query);
        //    var publicUrl = _options.Value.SiteSettings.PublisherUrl.TrimEnd('/');

        //    model.PageCount = 0;

        //    var client = new HttpClient();

        //    // Get a list of editors that are outside the current cloud.
        //    var otherEditors = _options.Value.EditorUrls.Where(w => w.CloudName.Equals(_options.Value.PrimaryCloud, StringComparison.CurrentCultureIgnoreCase) == false).ToList();

        //    model.EditorCount++;

        //    //
        //    // If we are preloading CDN
        //    if (model.PreloadCdn)
        //    {
        //        foreach (var article in articleList)
        //        {
        //            try
        //            {
        //                var response = await client.GetAsync($"{publicUrl}/{(article.UrlPath == "root" ? "" : article.UrlPath)}");
        //                response.EnsureSuccessStatusCode();
        //                _ = await response.Content.ReadAsStringAsync();
        //                //model.PageCount++;
        //            }
        //            catch (Exception e)
        //            {
        //                _logger.LogError(e.Message, e);
        //            }
        //        }
        //    }


        //    return View(model);
        //}

        #endregion

        #region Data Services

        /// <summary>
        /// Check to see if a page title is already taken.
        /// </summary>
        /// <param name="articleNumber"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> CheckTitle(int articleNumber, string title)
        {
            var result = await _articleLogic.ValidateTitle(title, articleNumber);

            if (result) return Json(true);

            return Json($"Title '{title}' is already taken.");
        }

        /// <summary>
        /// Gets a list of articles (web pages)
        /// </summary>
        /// <param name="term">search text value (optional)</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetArticleList(string term = "")
        {
            var query = _dbContext.ArticleCatalog.Select(s => new ArticleListItem()
            {
                ArticleNumber = s.ArticleNumber,
                Title = s.Title,
                IsDefault = s.UrlPath == "root",
                LastPublished = s.Published,
                UrlPath = s.UrlPath,
                Status = s.Status,
                Updated = s.Updated
            }).OrderBy(o => o.Title);

            var data = new List<ArticleListItem>();
            if (string.IsNullOrEmpty(term))
            {
                data.AddRange(await query.Take(10).ToListAsync());
            }
            else
            {
                term = term.TrimStart('/').Trim().ToLower();
                data.AddRange(await query.Where(w => w.Title.ToLower().Contains(term)).Take(10).ToListAsync());
            }

            return Json(data);

        }

        /// <summary>
        /// Gets a list of articles (pages) on this website.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <remarks>Returns published and non-published links</remarks>
        public async Task<IActionResult> List_Articles(string text)
        {
            IQueryable<Article> query = _dbContext.Articles
            .OrderBy(o => o.Title)
            .Where(w => w.StatusCode == (int)StatusCodeEnum.Active || w.StatusCode == (int)StatusCodeEnum.Inactive);

            if (!string.IsNullOrEmpty(text))
            {
                query = query.Where(x => x.Title.ToLower().Contains(text.ToLower()));
            }

            var model = await query.Select(s => new
            {
                s.Title,
                s.UrlPath
            }).Distinct().Take(10).ToListAsync();

            return Json(model);
        }

        /// <summary>
        /// Sends an article (or page) to trash bin.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> TrashArticle(int Id)
        {
            await _articleLogic.TrashArticle(Id);
            return RedirectToAction("Index", "Editor");
        }

        /// <summary>
        ///     Gets a role list, and allows for filtering
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get_RoleList(string text)
        {
            var query = _dbContext.Roles.Select(s => new RoleItemViewModel
            {
                Id = s.Id,
                RoleName = s.Name,
                RoleNormalizedName = s.NormalizedName
            });

            if (!string.IsNullOrEmpty(text)) query = query.Where(w => w.RoleName.StartsWith(text));

            return Json(await query.OrderBy(r => r.RoleName).ToListAsync());
        }

        #region REDIRECT MANAGEMENT

        /// <summary>
        /// Redirect manager page
        /// </summary>
        /// <param name="sortOrder"></param>
        /// <param name="currentSort"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrators, Editors")]
        public async Task<IActionResult> Redirects(string sortOrder, string currentSort, int pageNo = 0, int pageSize = 10)
        {

            ViewData["sortOrder"] = sortOrder;
            ViewData["currentSort"] = currentSort;
            ViewData["pageNo"] = pageNo;
            ViewData["pageSize"] = pageSize;

            var query = _articleLogic.GetArticleRedirects();

            ViewData["RowCount"] = await query.CountAsync();

            if (sortOrder == "desc")
            {
                if (!string.IsNullOrEmpty(currentSort))
                {
                    switch (currentSort)
                    {
                        case "FromUrl":
                            query = query.OrderByDescending(o => o.FromUrl);
                            break;
                        case "Title":
                            query = query.OrderByDescending(o => o.Id);
                            break;
                        case "ToUrl":
                            query = query.OrderByDescending(o => o.ToUrl);
                            break;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(currentSort))
                {
                    switch (currentSort)
                    {
                        case "FromUrl":
                            query = query.OrderBy(o => o.FromUrl);
                            break;
                        case "Id":
                            query = query.OrderBy(o => o.Id);
                            break;
                        case "ToUrl":
                            query = query.OrderBy(o => o.ToUrl);
                            break;
                    }
                }
            }

            var model = await query.Skip(pageNo * pageSize).Take(pageSize).ToListAsync();

            return View(model);
        }

        /// <summary>
        /// Sends an article (or page) to trash bin.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> RedirectDelete(Guid Id)
        {
            var article = await _dbContext.Articles.FirstOrDefaultAsync(f => f.Id == Id);

            await _articleLogic.TrashArticle(article.ArticleNumber);

            return RedirectToAction("Redirects");
        }

        /// <summary>
        /// Updates a redirect
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="FromUrl"></param>
        /// <param name="ToUrl"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrators, Editors")]
        public async Task<IActionResult> RedirectEdit([FromForm] Guid Id, string FromUrl, string ToUrl)
        {
            var redirect = await _dbContext.Articles.FirstOrDefaultAsync(f => f.Id == Id && f.StatusCode == (int)StatusCodeEnum.Redirect);
            if (redirect == null)
                return NotFound();

            redirect.UrlPath = FromUrl;
            redirect.Content = ToUrl;

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Redirects");
        }

        #endregion

        #endregion

    }
}
