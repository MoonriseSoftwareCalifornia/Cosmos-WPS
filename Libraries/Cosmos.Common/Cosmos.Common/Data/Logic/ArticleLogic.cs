using Cosmos.Common.Models;
using Cosmos.Cms.Common.Services.Configurations;
using Cosmos.Common.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Cosmos.Common.Data.Logic
{
    /// <summary>
    ///     Main logic behind getting and maintaining web site articles.
    /// </summary>
    /// <remarks>An article is the "content" behind a web page.</remarks>
    public class ArticleLogic
    {
        private readonly bool _isEditor;
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        ///     Publisher Constructor
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="distributedCache"></param>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        /// <param name="memoryCache">Memory cache used only by Publishers</param>
        /// <param name="isEditor">Is in edit mode or not (by passess redis if set to true)</param>
        /// <param name="memoryCacheMaxSeconds">Maximum seconds to store item in memory cache.</param>
        public ArticleLogic(ApplicationDbContext dbContext,
            IOptions<CosmosConfig> config,
            IMemoryCache memoryCache,
            bool isEditor = false)
        {
            _memoryCache = memoryCache;
            DbContext = dbContext;
            CosmosOptions = config;
            _isEditor = isEditor;
        }

        //private readonly bool _editorMode;

        /// <summary>
        ///     Database Content
        /// </summary>
        protected ApplicationDbContext DbContext { get; }

        /// <summary>
        ///     Site customization config
        /// </summary>
        protected IOptions<CosmosConfig> CosmosOptions { get; }

        /// <summary>
        /// Provides cache hit information
        /// </summary>
        public string[] CacheResult { get; internal set; }

        /// <summary>
        /// Gets the list of child pages for a given page URL
        /// </summary>
        /// <param name="prefix">Page url</param>
        /// <param name="pageNo">Zero based index (page 1 is index 0)</param>
        /// <param name="pageSize">Number of records in a page.</param>
        /// <param name="orderByPublishedDate">Order by when was published (most recent on top)</param>
        /// <returns></returns>
        public async Task<TableOfContents> GetTOC(string prefix, int pageNo = 0, int pageSize = 10, bool orderByPublishedDate = false)
        {
            if (string.IsNullOrEmpty(prefix) || string.IsNullOrWhiteSpace(prefix) || prefix.Equals("/"))
            {
                prefix = "";
            }
            else
            {
                prefix = "/" + (System.Web.HttpUtility.UrlDecode(prefix.ToLower().Replace("%20", "_").Replace(" ", "_")) + "/").Trim('/');
            }
            var skip = pageNo * pageSize;

            //var query = DbContext.Articles.Select(s =>
            //new TOCItem { UrlPath = s.UrlPath, Title = s.Title, Published = s.Published.Value, Updated = s.Updated })
            //    .Where(a => a.Published <= DateTime.UtcNow &&
            //            EF.Functions.Like(a.Title, prefix + "%") &&
            //            (EF.Functions.Like(a.Title, prefix + "%/%") == false)).Distinct();


            // Regex example (?i)(^[cosmos]*)(\/[^\/]*){1}$

            var dcount = "{" + (prefix.Count(c => c == '/')) + "}";
            var epath = prefix.TrimStart('/').Replace("/", "\\/");
            var pattern = $"(?i)(^[{epath}]*)(\\/[^\\/]*){dcount}$";

            var query = (from t in DbContext.Pages
                         where t.Published <= DateTimeOffset.UtcNow &&
                         t.StatusCode != (int)StatusCodeEnum.Redirect
                         && Regex.IsMatch(t.UrlPath, pattern)
                         select new TableOfContentsItem
                         {
                             UrlPath = t.UrlPath,
                             Title = t.Title,
                             Published = t.Published.Value,
                             Updated = t.Updated,
                             BannerImage = t.BannerImage,
                             AuthorInfo = t.AuthorInfo
                         });


            if (orderByPublishedDate)
            {
                query = query.OrderByDescending(o => o.Published);
            }
            else
            {
                query = query.OrderBy(o => o.Title);
            }

            var results = await query.ToListAsync();

            var model = new TableOfContents();
            model.TotalCount = results.Count;
            model.PageNo = pageNo;
            model.PageSize = pageSize;
            model.Items = results.Skip(skip).Take(pageSize).ToList();

            return model;
        }

        #region GET ARTICLE METHODS

        /// <summary>
        ///     Gets the current *published* version of an article.  Gets the home page if ID is null.
        /// </summary>
        /// <param name="urlPath">URL Encoded path</param>
        /// <param name="lang">Language to return content as.</param>
        /// <param name="publishedOnly">Only retrieve latest published version</param>
        /// <param name="onlyActive">Only retrieve active status</param>
        /// <param name="forceUseRedis">Force use of distributed cache</param>
        /// <returns>
        ///     <see cref="ArticleViewModel" />
        /// </returns>
        /// <remarks>
        ///     <para>
        ///     Retrieves an article from the following sources in order:
        ///     </para>
        ///    <list type="number">
        ///       <item>Short term (5 second) Entity Framework Cache</item>
        ///       <item>SQL Database</item>
        ///     </list>
        ///     <para>
        ///         Returns <see cref="ArticleViewModel" />. For more details on what is returned, see <see cref="GetArticle" />
        ///         and <see cref="BuildArticleViewModel" />.
        ///     </para>
        ///     <para>NOTE: Cannot access articles that have been deleted.</para>
        /// </remarks>
        public virtual async Task<ArticleViewModel> GetByUrl(string urlPath, string lang = "", TimeSpan? cacheSpan = null, TimeSpan? layoutCache = null)
        {
            urlPath = urlPath?.ToLower().Trim(new char[] { ' ', '/' });
            if (string.IsNullOrEmpty(urlPath) || urlPath.Trim() == "/")
                urlPath = "root";

            if (_memoryCache == null || cacheSpan == null)
            {
                var entity = await DbContext.Pages.WithPartitionKey(urlPath)
               .Where(a => a.Published <= DateTimeOffset.UtcNow)
               .OrderByDescending(o => o.VersionNumber).FirstOrDefaultAsync();

                if (entity == null)
                    return null;

                return await BuildArticleViewModel(entity, lang);
            }

            _memoryCache.TryGetValue($"{urlPath}-{lang}", out ArticleViewModel model);

            if (model == null)
            {
                var data = await DbContext.Pages.WithPartitionKey(urlPath)
                   .Where(a => a.Published <= DateTimeOffset.UtcNow)
                   .OrderByDescending(o => o.VersionNumber).FirstOrDefaultAsync();

                if (data == null)
                    return null;

                model = await BuildArticleViewModel(data, lang, layoutCache);

                _memoryCache.Set($"{urlPath}-{lang}", model, cacheSpan.Value);
            }

            return model;

        }

        #endregion

        #region PRIVATE METHODS
        /// <summary>
        ///     This method creates an <see cref="ArticleViewModel" /> ready for display and edit.
        /// </summary>
        /// <param name="article"></param>
        /// <param name="lang"></param>
        /// <returns>
        ///     <para>Returns <see cref="ArticleViewModel" /> that includes:</para>
        ///     <list type="bullet">
        ///         <item>
        ///             Current ArticleVersionInfo
        ///         </item>
        ///         <item>
        ///             If the site is in authoring or publishing mode (<see cref="ArticleViewModel.ReadWriteMode" />)
        ///         </item>
        ///     </list>
        /// </returns>
        protected async Task<ArticleViewModel> BuildArticleViewModel(Article article, string lang)
        {
            var authorInfo = await DbContext.AuthorInfos.FirstOrDefaultAsync(f => f.UserId == article.UserId);

            return new ArticleViewModel
            {
                ArticleNumber = article.ArticleNumber,
                LanguageCode = lang,
                LanguageName = "",
                CacheDuration = 10,
                Content = article.Content,
                StatusCode = (StatusCodeEnum)article.StatusCode,
                Id = article.Id,
                Published = article.Published.HasValue ? article.Published.Value : null,
                Title = article.Title,
                UrlPath = article.UrlPath,
                Updated = article.Updated,
                VersionNumber = article.VersionNumber,
                HeadJavaScript = article.HeaderJavaScript,
                FooterJavaScript = article.FooterJavaScript,
                Layout = await GetDefaultLayout(),
                ReadWriteMode = _isEditor,
                RoleList = article.RoleList,
                Expires = article.Expires.HasValue ? article.Expires.Value : null,
                BannerImage = article.BannerImage,
                AuthorInfo = JsonConvert.SerializeObject(authorInfo).Replace("\"", "'")
            };
        }

        /// <summary>
        ///     This method creates an <see cref="ArticleViewModel" /> ready for display and edit.
        /// </summary>
        /// <param name="article"></param>
        /// <param name="lang"></param>
        /// <returns>
        ///     <para>Returns <see cref="ArticleViewModel" /> that includes:</para>
        ///     <list type="bullet">
        ///         <item>
        ///             Current ArticleVersionInfo
        ///         </item>
        ///         <item>
        ///             If the site is in authoring or publishing mode (<see cref="ArticleViewModel.ReadWriteMode" />)
        ///         </item>
        ///     </list>
        /// </returns>
        protected async Task<ArticleViewModel> BuildArticleViewModel(PublishedPage article, string lang, TimeSpan? layoutCache = null)
        {

            return new ArticleViewModel
            {
                ArticleNumber = article.ArticleNumber,
                BannerImage = article.BannerImage,
                LanguageCode = lang,
                LanguageName = "",
                CacheDuration = 10,
                Content = article.Content,
                StatusCode = (StatusCodeEnum)article.StatusCode,
                Id = article.Id,
                Published = article.Published.HasValue ? article.Published.Value : null,
                Title = article.Title,
                UrlPath = article.UrlPath,
                Updated = article.Updated,
                VersionNumber = article.VersionNumber,
                HeadJavaScript = article.HeaderJavaScript,
                FooterJavaScript = article.FooterJavaScript,
                Layout = await GetDefaultLayout(layoutCache),
                ReadWriteMode = _isEditor,
                RoleList = article.RoleList,
                Expires = article.Expires.HasValue ? article.Expires.Value : null,
                AuthorInfo = article.AuthorInfo
            };
        }

        /// <summary>
        ///     Provides a standard method for turning a title into a URL Encoded path.
        /// </summary>
        /// <param name="title">Title to be converted into a URL.</param>
        /// <returns></returns>
        /// <remarks>
        ///     <para>This is accomplished using <see cref="HttpUtility.UrlEncode(string)" />.</para>
        ///     <para>Blanks are turned into underscores (i.e. "_").</para>
        ///     <para>All strings are normalized to lower case.</para>
        /// </remarks>
        public static string HandleUrlEncodeTitle(string title)
        {
            return HttpUtility.UrlEncode(title.Trim().Replace(" ", "_").ToLower()).Replace("%2f", "/");
        }


        /// <summary>
        ///     Gets the default layout, including navigation menu.
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="includeMenu"></param>
        /// <returns></returns>
        /// <remarks>
        ///     <para>
        ///         Inserts a Bootstrap style nav bar where this '&lt;!--{COSMOS-UL-NAV}--&gt;' is placed in the
        ///         <see cref="LayoutViewModel.HtmlHeader" />
        ///     </para>
        /// </remarks>
        public async Task<LayoutViewModel> GetDefaultLayout(TimeSpan? layoutCache = null)
        {
            if (_memoryCache == null || layoutCache == null)
            {
                var entity = await DbContext.Layouts.AsNoTracking().FirstOrDefaultAsync(a => a.IsDefault);
                return new LayoutViewModel(entity);
            }

            _memoryCache.TryGetValue("defLayout", out LayoutViewModel model);

            if (model == null)
            {
                var entity = await DbContext.Layouts.FirstOrDefaultAsync(a => a.IsDefault);
                DbContext.Entry(entity).State = EntityState.Detached;
                model = new LayoutViewModel(entity);
                _memoryCache.Set("defLayout", model, layoutCache.Value);
            }

            return model;
        }


        #endregion

        #region CACHE FUNCTIONS

        /// <summary>
        ///     Serializes an object using <see cref="Newtonsoft.Json.JsonConvert.SerializeObject(object)" />
        ///     and <see cref="System.Text.Encoding.UTF32" />.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] Serialize(object obj)
        {
            if (obj == null) return null;
            return Encoding.UTF32.GetBytes(JsonConvert.SerializeObject(obj));
        }

        /// <summary>
        ///     Deserializes an object using <see cref="Newtonsoft.Json.JsonConvert.DeserializeObject(string)" />
        ///     and <see cref="System.Text.Encoding.UTF32" />.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] bytes)
        {
            var data = Encoding.UTF32.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(data);
        }

        #endregion

        /// <summary>
        /// Determines if a publisher can serve requests.
        /// </summary>
        /// <returns></returns>
        public bool GetPublisherHealth()
        {
            return true;
        }
    }
}