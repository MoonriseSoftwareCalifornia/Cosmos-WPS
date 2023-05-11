﻿using Azure.ResourceManager.Cdn;
using Azure.ResourceManager.Cdn.Models;
using Azure.ResourceManager.Resources;
using Cosmos.Common.Data;
using Cosmos.Common.Models;
using Cosmos.Cms.Common.Services.Configurations;
using Cosmos.Cms.Data.Logic;
using Cosmos.Cms.Models;
using Cosmos.Cms.Services;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Cosmos.Cms.Controllers
{
    /// <summary>
    /// Layouts controller
    /// </summary>
    //[ResponseCache(NoStore = true)]
    [Authorize(Roles = "Administrators, Editors")]
    public class LayoutsController : BaseController
    {
        private readonly ArticleEditLogic _articleLogic;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<LayoutsController> _logger;
        private readonly Uri _blobPublicAbsoluteUrl;
        private readonly IViewRenderService _viewRenderService;
        private readonly AzureSubscription _azureSubscription;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="userManager"></param>
        /// <param name="articleLogic"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        /// <param name="viewRenderService"></param>
        /// <param name="azureSubscription"></param>
        public LayoutsController(ApplicationDbContext dbContext,
            UserManager<IdentityUser> userManager,
            ArticleEditLogic articleLogic,
            IOptions<CosmosConfig> options,
            ILogger<LayoutsController> logger,
            IViewRenderService viewRenderService,
            AzureSubscription azureSubscription) : base(dbContext, userManager, articleLogic, options)
        {
            _dbContext = dbContext;
            _articleLogic = articleLogic;
            _logger = logger;
            _azureSubscription = azureSubscription;

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

        private bool LayoutExists(Guid id)
        {
            return _dbContext.Layouts.Where(e => e.Id == id).CosmosAnyAsync().Result;
        }


        private async Task PurgeCdn()
        {
            if (_azureSubscription.Subscription == null)
            {
                _logger.LogError("Tried to purge CDN but DefaultAzureCredential did not return a subscription in Startup.cs.");
                return; // Not authenticated
            }

            // Check for CDN
            var cdnSetting = await _dbContext.Settings.FirstOrDefaultAsync(f => f.Name == Cosmos_Admin_CdnController.CDNSERVICENAME);

            if (cdnSetting != null)
            {
                try
                {
                    var azureCdnEndpoint = JsonConvert.DeserializeObject<AzureCdnEndpoint>(cdnSetting.Value);

                    SubscriptionResource subscription = _azureSubscription.Subscription;

                    var group = await subscription.GetResourceGroupAsync(azureCdnEndpoint.ResourceGroupName);

                    var profile = await group.Value.GetProfileAsync(azureCdnEndpoint.CdnProfileName);

                    var endPoint = await profile.Value.GetCdnEndpointAsync(azureCdnEndpoint.EndpointName);

                    if (azureCdnEndpoint.SkuName.Contains("akamai", StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Akamami does not support wildcard so iterate throw URLs in batches of 100

                        var purgeUrls = await _dbContext.Pages.Select(s => s.UrlPath).Distinct().ToListAsync();
                        var urls = new List<string>();

                        var count = 0;

                        foreach (var url in purgeUrls)
                        {
                            if (url.Equals("root"))
                            {
                                urls.Add("/");
                            }
                            else
                            {
                                urls.Add("/" + url.Trim('/'));
                            }
                            count++;

                            if (count == 100)
                            {
                                var purgeContent = new PurgeContent(urls);
                                endPoint.Value.PurgeContent(Azure.WaitUntil.Started, purgeContent);
                                count = 0;
                                urls.Clear();
                            }
                        }

                        if (urls.Any())
                        {
                            var purgeContent = new PurgeContent(urls);
                            endPoint.Value.PurgeContent(Azure.WaitUntil.Started, purgeContent);
                        }
                    }
                    else
                    {
                        endPoint.Value.PurgeContent(Azure.WaitUntil.Started, new PurgeContent(new string[] { "/*" }));
                    }


                }
                catch (Exception e)
                {
                    _logger.LogError($"Failed to refresh CDN for a layout change due the following reason:", e);
                }

            }
        }



        /// <summary>
        /// Gets a list of layouts
        /// </summary>
        /// <param name="includeDefault">Default = false</param>
        /// <returns></returns>
        public async Task<IActionResult> GetLayoutList(bool includeDefault = false)
        {
            if (includeDefault)
            {
                return Json(await _dbContext.Layouts.OrderBy(o => o.LayoutName).Select(s => new { LayoutId = s.Id, s.LayoutName, s.Notes }).ToListAsync());
            }

            return Json(await _dbContext.Layouts.Where(w => w.IsDefault == false).OrderBy(o => o.LayoutName).Select(s => new { LayoutId = s.Id, s.LayoutName, s.Notes }).ToListAsync());
        }

        /// <summary>
        /// Gets the home page with the specified layout (may not be the default layout)
        /// </summary>
        /// <param name="id">Layout Id (default layout if null)</param>
        /// <returns>ViewResult with <see cref="ArticleViewModel"/></returns>
        private async Task<IActionResult> GetLayoutWithHomePage(Guid? id)
        {
            // Get the home page
            var model = await _articleLogic.GetByUrl("");

            // Specify layout if given.
            if (id.HasValue)
            {
                var layout = await _dbContext.Layouts.FirstOrDefaultAsync(i => i.Id == id.Value);
                model.Layout = new LayoutViewModel(layout);
            }

            // Make its editable
            model.Layout.HtmlHeader = model.Layout.HtmlHeader.Replace(" crx=\"", " contenteditable=\"", StringComparison.CurrentCultureIgnoreCase);
            model.Layout.FooterHtmlContent = model.Layout.FooterHtmlContent.Replace(" crx=\"", " contenteditable=\"", StringComparison.CurrentCultureIgnoreCase);

            return View(model);
        }

        /// <summary>
        /// Gets a list of layouts
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index(string sortOrder = "asc", string currentSort = "LayoutName", int pageNo = 0, int pageSize = 10)
        {

            ViewData["ShowCreateFirstLayout"] = (!await _dbContext.Layouts.CosmosAnyAsync());

            ViewData["ShowFirstPageBtn"] = (!await _dbContext.Articles.CosmosAnyAsync());

            ViewData["sortOrder"] = sortOrder;
            ViewData["currentSort"] = currentSort;
            ViewData["pageNo"] = pageNo;
            ViewData["pageSize"] = pageSize;

            var query = _dbContext.Layouts.AsQueryable();

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
                    }
                }
            }

            var model = _dbContext.Layouts.Select(s => new LayoutIndexViewModel
            {
                Id = s.Id,
                IsDefault = s.IsDefault,
                LayoutName = s.LayoutName,
                Notes = s.Notes
            });

            return View(await model.Skip(pageNo * pageSize).Take(pageSize).ToListAsync());
        }

        /// <summary>
        /// Page returns a list of community layouts.
        /// </summary>
        /// <returns></returns>
        public IActionResult CommunityLayouts(string sortOrder = "asc", string currentSort = "Name", int pageNo = 0, int pageSize = 10)
        {

            ViewData["sortOrder"] = sortOrder;
            ViewData["currentSort"] = currentSort;
            ViewData["pageNo"] = pageNo;
            ViewData["pageSize"] = pageSize;

            var utilities = new LayoutUtilities();

            var query = utilities.CommunityCatalog.LayoutCatalog.AsQueryable();

            ViewData["RowCount"] = query.Count();


            if (sortOrder == "desc")
            {
                if (!string.IsNullOrEmpty(currentSort))
                {
                    switch (currentSort)
                    {
                        case "ArticleNumber":
                            query = query.OrderByDescending(o => o.License);
                            break;
                        case "Name":
                            query = query.OrderByDescending(o => o.Name);
                            break;
                        case "Description":
                            query = query.OrderByDescending(o => o.Description);
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
                            query = query.OrderBy(o => o.License);
                            break;
                        case "Name":
                            query = query.OrderBy(o => o.Name);
                            break;
                        case "Description":
                            query = query.OrderBy(o => o.Description);
                            break;
                    }
                }
            }


            return View(query.Skip(pageNo * pageSize).Take(pageSize).ToList());
        }

        /// <summary>
        /// Create a new layout
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Create()
        {
            var layout = new Layout();
            layout.IsDefault = false;
            layout.LayoutName = "New Layout " + await _dbContext.Layouts.CountAsync();
            layout.Notes = "New layout created. Please customize using code editor.";
            _dbContext.Layouts.Add(layout);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("EditCode", new { layout.Id });
        }

        /// <summary>
        /// Deletes a layout that is not the default layout
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(Guid Id)
        {
            var entity = await _dbContext.Layouts.FindAsync(Id);

            if (!entity.IsDefault)
            {
                // also remove pages that go with this layout.
                var pages = await _dbContext.Templates.Where(t => t.LayoutId == Id).ToListAsync();
                _dbContext.Templates.RemoveRange(pages);
                _dbContext.Layouts.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                return BadRequest("Cannot delete the default layout.");
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Edit the page header and footer of a layout.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="header"></param>
        /// <param name="footer"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Edit([Bind("id,header,footer")] Guid id, string header, string footer)
        {
            var layout = await _dbContext.Layouts.FirstOrDefaultAsync(i => i.Id == id);

            // Make editable
            //header = header.Replace(" contenteditable=\"", " crx=\"", StringComparison.CurrentCultureIgnoreCase);
            //footer = footer.Replace(" contenteditable=\"", " crx=\"", StringComparison.CurrentCultureIgnoreCase);

            layout.HtmlHeader = header;
            layout.FooterHtmlContent = footer;

            await _dbContext.SaveChangesAsync();

            return await GetLayoutWithHomePage(id);
        }

        /// <summary>
        /// Gets a layout to edit it's notes
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditNotes(Guid? id)
        {
            if (id == null)
                return RedirectToAction("Index");

            var model = await _dbContext.Layouts.Select(s => new LayoutIndexViewModel
            {
                Id = s.Id,
                IsDefault = s.IsDefault,
                LayoutName = s.LayoutName,
                Notes = s.Notes
            }).FirstOrDefaultAsync(f => f.Id == id.Value);

            if (model == null) return NotFound();

            return View(model);
        }

        /// <summary>
        /// Edit layout notes
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditNotes([Bind(include: "Id,IsDefault,LayoutName,Notes")] LayoutIndexViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model != null)
            {
                var layout = await _dbContext.Layouts.FindAsync(model.Id);
                layout.LayoutName = model.LayoutName;
                var contentHtmlDocument = new HtmlDocument();
                contentHtmlDocument.LoadHtml(HttpUtility.HtmlDecode(model.Notes));
                if (contentHtmlDocument.ParseErrors.Any())
                    foreach (var error in contentHtmlDocument.ParseErrors)
                        ModelState.AddModelError("Notes", error.Reason);

                var remove = "<div style=\"display:none;\"></div>";
                layout.Notes = contentHtmlDocument.ParsedText.Replace(remove, "").Trim();
                //layout.IsDefault = model.IsDefault;
                if (model.IsDefault)
                {
                    var layouts = await _dbContext.Layouts.Where(w => w.Id != model.Id).ToListAsync();
                    foreach (var layout1 in layouts) layout1.IsDefault = false;
                }

                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Edit code for a layout
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditCode(Guid? id)
        {
            if (id == null) return NotFound();

            var layout = await _dbContext.Layouts.FirstOrDefaultAsync(f => f.Id == id.Value);
            if (layout == null) return NotFound();

            var model = new LayoutCodeViewModel
            {
                Id = layout.Id,
                EditorTitle = layout.LayoutName,
                EditorFields = new List<EditorField>
                {
                    new()
                    {
                        FieldId = "Head",
                        FieldName = "Head",
                        EditorMode = EditorMode.Html,
                        ToolTip = "Layout content to appear in the HEAD of every page."
                    },
                    new()
                    {
                        FieldId = "HtmlHeader",
                        FieldName = "Header Content",
                        EditorMode = EditorMode.Html,
                        ToolTip = "Layout body header content to appear on every page."
                    },
                    new()
                    {
                        FieldId = "FooterHtmlContent",
                        FieldName = "Footer Content",
                        EditorMode = EditorMode.Html,
                        ToolTip = "Layout footer content to appear at the bottom of the body on every page."
                    }
                },
                CustomButtons = new List<string> { "Preview", "Layouts" },
                Head = layout.Head,
                HtmlHeader = layout.HtmlHeader,
                BodyHtmlAttributes = layout.BodyHtmlAttributes,
                FooterHtmlContent = layout.FooterHtmlContent,
                EditingField = "Head"
            };
            return View(model);
        }

        /// <summary>
        ///     Saves the code and html of the page.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="layout"></param>
        /// <returns></returns>
        /// <remarks>
        ///     <para>
        ///         This method saves page code to the database. The following properties are validated with method
        ///         <see cref="BaseController.BaseValidateHtml" />:
        ///     </para>
        ///     <list type="bullet">
        ///         <item>
        ///             <see cref="LayoutCodeViewModel.Head" />
        ///         </item>
        ///         <item>
        ///             <see cref="LayoutCodeViewModel.HtmlHeader" />
        ///         </item>
        ///         <item>
        ///             <see cref="LayoutCodeViewModel.FooterHtmlContent" />
        ///         </item>
        ///     </list>
        ///     <para>
        ///         HTML formatting errors that could not be automatically fixed by <see cref="BaseController.BaseValidateHtml" />
        ///         are logged with <see cref="ControllerBase.ModelState" />.
        ///     </para>
        /// </remarks>
        /// <exception cref="NotFoundResult"></exception>
        /// <exception cref="UnauthorizedResult"></exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCode(Guid id, LayoutCodeViewModel layout)
        {
            if (id != layout.Id) return NotFound();

            if (ModelState.IsValid)
                try
                {
                    // Strip out BOM
                    layout.Head = StripBOM(layout.Head);
                    layout.HtmlHeader = StripBOM(layout.HtmlHeader);
                    layout.FooterHtmlContent = StripBOM(layout.FooterHtmlContent);
                    layout.BodyHtmlAttributes = StripBOM(layout.BodyHtmlAttributes);

                    //
                    // This layout now is the default, make sure the others are set to "false."

                    var entity = await _dbContext.Layouts.FindAsync(layout.Id);
                    entity.FooterHtmlContent =
                        BaseValidateHtml("FooterHtmlContent", layout.FooterHtmlContent);
                    entity.Head = BaseValidateHtml("Head", layout.Head);
                    entity.HtmlHeader = BaseValidateHtml("HtmlHeader", layout.HtmlHeader);
                    entity.BodyHtmlAttributes = layout.BodyHtmlAttributes;

                    // Check validation again after validation of HTML
                    await _dbContext.SaveChangesAsync();

                    var jsonModel = new SaveCodeResultJsonModel
                    {
                        ErrorCount = ModelState.ErrorCount,
                        IsValid = ModelState.IsValid
                    };
                    jsonModel.Errors.AddRange(ModelState.Values
                        .Where(w => w.ValidationState == ModelValidationState.Invalid)
                        .ToList());
                    jsonModel.ValidationState = ModelState.ValidationState;

                    await PurgeCdn();

                    return Json(jsonModel);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LayoutExists(layout.Id)) return NotFound();
                    throw;
                }
                catch (Exception)
                {
                    throw;
                }

            return View(layout);
        }

        /// <summary>
        /// Preview 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Preview(Guid id)
        {
            var layout = await _dbContext.Layouts.FindAsync(id);
            if (layout == null) return NotFound();

            var model = await _articleLogic.GetByUrl("");
            model.Layout = new LayoutViewModel(layout);
            model.EditModeOn = false;
            model.ReadWriteMode = false;
            model.PreviewMode = true;

            return View("~/Views/Home/Preview.cshtml", model);
        }

        /// <summary>
        /// Preview how a layout will look in edit mode.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditPreview(int id)
        {
            var layout = await _dbContext.Layouts.FindAsync(id);
            var model = await _articleLogic.GetByUrl("");
            model.Layout = new LayoutViewModel(layout);
            model.EditModeOn = true;
            model.ReadWriteMode = true;
            model.PreviewMode = true;
            return View("~/Views/Home/Index.cshtml", model);
        }

        /// <summary>
        /// Exports a layout with a blank page
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrators, Editors, Authors, Team Members")]
        public async Task<IActionResult> ExportLayout(int? id)
        {
            var article = await _articleLogic.GetByUrl("");

            var view = "~/Views/Layouts/ExportLayout.cshtml";
            var exportName = $"layout-{article.Layout.Id}.html";

            if (id.HasValue)
            {
                if (id.Value < 0)
                {
                    // Blank layout
                    view = "~/Views/Layouts/ExportBlank.cshtml";
                    exportName = "blank-layout.html";
                }
                else
                {
                    var layout = await _dbContext.Layouts.FindAsync(id.Value);
                    article.Layout = new LayoutViewModel(layout);
                }
            }

            var htmlUtilities = new HtmlUtilities();

            article.Layout.Head = htmlUtilities.RelativeToAbsoluteUrls(article.Layout.Head, _blobPublicAbsoluteUrl, false);

            // Layout body elements
            article.Layout.HtmlHeader = htmlUtilities.RelativeToAbsoluteUrls(article.Layout.HtmlHeader, _blobPublicAbsoluteUrl, true);
            article.Layout.FooterHtmlContent = htmlUtilities.RelativeToAbsoluteUrls(article.Layout.FooterHtmlContent, _blobPublicAbsoluteUrl, true);

            article.HeadJavaScript = htmlUtilities.RelativeToAbsoluteUrls(article.HeadJavaScript, _blobPublicAbsoluteUrl, false);
            article.Content = htmlUtilities.RelativeToAbsoluteUrls(article.Content, _blobPublicAbsoluteUrl, false);
            article.FooterJavaScript = htmlUtilities.RelativeToAbsoluteUrls(article.HeadJavaScript, _blobPublicAbsoluteUrl, false);

            var html = await _viewRenderService.RenderToStringAsync(view, article);



            var bytes = Encoding.UTF8.GetBytes(html);

            return File(bytes, "application/octet-stream", exportName);
        }

        /// <summary>
        /// Set a layout as the default layout.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> SetLayoutAsDefault(Guid? id)
        {
            if (id == null)
                return RedirectToAction("Index");

            var model = await _dbContext.Layouts.Select(s => new LayoutIndexViewModel
            {
                Id = s.Id,
                IsDefault = s.IsDefault,
                LayoutName = s.LayoutName,
                Notes = s.Notes
            }).FirstOrDefaultAsync(f => f.Id == id.Value);

            if (model == null) return NotFound();

            return View(model);
        }

        /// <summary>
        ///     Sets a layout as the default layout
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SetLayoutAsDefault(LayoutIndexViewModel model)
        {

            var layout = await _dbContext.Layouts.FirstOrDefaultAsync(f => f.Id == model.Id);
            layout.IsDefault = true;

            if (layout == null)
                return RedirectToAction("Index", "Layouts");

            await _dbContext.SaveChangesAsync();
            var items = await _dbContext.Layouts.Where(w => w.Id != model.Id).ToListAsync();
            foreach (var item in items)
            {
                item.IsDefault = false;
            }

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Layouts");
        }

        /// <summary>
        /// Gets a community layout
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> ImportCommunityLayout(string id)
        {
            try
            {
                if (await _dbContext.Layouts.Where(c => c.CommunityLayoutId == id).CosmosAnyAsync())
                {
                    throw new Exception("Layout already loaded.");
                }

                var utilities = new LayoutUtilities();
                var layout = await utilities.GetCommunityLayout(id, false);
                var communityPages = await utilities.GetCommunityTemplatePages(id);
                layout.IsDefault = (await _dbContext.Layouts.Where(a => a.IsDefault).CosmosAnyAsync()) == false;
                _dbContext.Layouts.Add(layout);
                await _dbContext.SaveChangesAsync();

                if (communityPages != null && communityPages.Any())
                {
                    var pages = communityPages.Select(p => new Template()
                    {
                        CommunityLayoutId = p.CommunityLayoutId,
                        Content = p.Content,
                        Description = p.Description,
                        LayoutId = layout.Id,
                        Title = p.Title
                    });

                    _dbContext.Templates.AddRange(pages);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Id", ex.Message);
            }

            return RedirectToAction("Index");
        }

    }
}
