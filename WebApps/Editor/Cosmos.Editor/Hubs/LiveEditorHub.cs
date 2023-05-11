using Cosmos.Cms.Controllers;
using Cosmos.Cms.Data.Logic;
using Cosmos.Cms.Models;
using Cosmos.Common.Data;
using HtmlAgilityPack;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Cosmos.Cms.Hubs
{
    /// <summary>
    /// Live editor collaboration hub
    /// </summary>
    /// [Authorize(Roles = "Reviewers, Administrators, Editors, Authors")]
    public class LiveEditorHub : Hub
    {
        private readonly ArticleEditLogic _articleLogic;
        private readonly ILogger<LiveEditorHub> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="articleLogic"></param>
        /// <param name="logger"></param>
        public LiveEditorHub(ArticleEditLogic articleLogic, ILogger<LiveEditorHub> logger)
        {
            _articleLogic = articleLogic;
            _logger = logger;
        }

        /// <summary>
        /// Adds an editor to the page group.
        /// </summary>
        /// <param name="articleId"></param>
        /// <returns></returns>
        public async Task JoinArticleGroup(string articleId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Article:{articleId}");
        }

        /// <summary>
        /// Joins the editing room
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task Notification(string data)
        {
            try
            {

                var model = JsonConvert.DeserializeObject<LiveEditorSignal>(data);

                switch (model.Command)
                {
                    case "join":
                        await Groups.AddToGroupAsync(Context.ConnectionId, model.EditorId);
                        break;
                    case "save":
                        try
                        {
                            await SaveEditorContent(model);
                            model.Command = "saved"; // Let caller know item is saved.
                            await Clients.Caller.SendCoreAsync("broadcastMessage", new[] { JsonConvert.SerializeObject(model) });
                            await Clients.OthersInGroup(model.EditorId).SendCoreAsync("broadcastMessage", new[] { data });
                        }
                        catch (Exception e)
                        {
                            _logger.LogError($"SIGNALR: {model.Command} method error: {e.Message}", e);
                            await ReturnErrorToCaller("Error saving editor content.", model);
                        }
                        break;
                    case "SavePageProperties":
                        var result = await SavePageProperties(model);
                        if (result == null)
                        {
                            await ReturnErrorToCaller("Error saving page properties.", model);
                        }
                        else
                        {
                            model.Data = JsonConvert.SerializeObject(result);
                            // Alert others
                            await Clients.OthersInGroup($"Article:{model.ArticleId}").SendCoreAsync("broadcastMessage", new[] { JsonConvert.SerializeObject(model) });
                            // Notify caller of job done.
                            model.Command = "PropertiesSaved";
                            await Clients.Caller.SendCoreAsync("broadcastMessage", new[] { JsonConvert.SerializeObject(model) });
                        }
                        break;
                    default:
                        await Clients.OthersInGroup(model.EditorId).SendCoreAsync("broadcastMessage", new[] { data });
                        break;
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}", e);
            }

        }

        private async Task ReturnErrorToCaller(string errorMessage, LiveEditorSignal model)
        {
            model.Command = "Error";
            model.Data = errorMessage;
            await Clients.Caller.SendCoreAsync("broadcastMessage", new[] { JsonConvert.SerializeObject(model) });
        }

        /// <summary>
        /// Saves page properties like title, published, banner image, etc...
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task<HtmlEditorSignal> SavePageProperties(LiveEditorSignal model)
        {
            if (model == null)
            {
                _logger.LogError("SIGNALR: SavePageProperties method, model was null.");
                return null;
            }

            // Next pull the original. This is a view model, not tracked by DbContext.
            var article = await _articleLogic.Get(model.ArticleId, EnumControllerName.Edit, model.UserId);
            if (article == null)
            {
                _logger.LogError($"SIGNALR: SavePageProperties method, could not find artile with ID: {model.ArticleId}.");
                return null;
            }

            try
            {
                var json = JsonConvert.SerializeObject(model.Data);
                var properties = JsonConvert.DeserializeObject<HtmlEditorSignal>(json);

                // Make sure we are setting to the orignal updated date/time
                // This is validated to make sure that someone else hasn't already edited this
                // entity
                article.Updated = DateTimeOffset.UtcNow;

                article.Title = properties.Title;
                article.Published = properties.Published;
                article.RoleList = properties.RoleList;
                article.BannerImage = properties.BannerImage;
                
                // Save changes back to the database
                var result = await _articleLogic.Save(article, model.UserId);

                return new HtmlEditorSignal()
                {
                    BannerImage = result.Model.BannerImage,
                    RoleList = result.Model.RoleList,
                    Published = result.Model.Published,
                    Updated = result.Model.Updated,
                    Title = result.Model.Title,
                    UrlPath = result.Model.UrlPath,
                    VersionNumber = result.Model.VersionNumber
                };

            }
            catch (Exception e)
            {
                _logger.LogError($"SIGNALR: SavePageProperties failed for article ID: {model.ArticleId} with the following error: " + e.Message, e);
                return null;
            }
        }

        private async Task SaveEditorContent(LiveEditorSignal model)
        {
            if (model == null)
            {
                _logger.LogError("SIGNALR: SaveEditorContent method, model was null.");
                return;
            }

            // Next pull the original. This is a view model, not tracked by DbContext.
            var article = await _articleLogic.Get(model.ArticleId, EnumControllerName.Edit, model.UserId);
            if (article == null)
            {
                _logger.LogError($"SIGNALR: SaveEditorContent method, could not find artile with ID: {model.ArticleId}.");
                return;
            }

            // Get the editable regions from the original document.
            var originalHtmlDoc = new HtmlDocument();
            originalHtmlDoc.LoadHtml(article.Content);
            var originalEditableDivs = originalHtmlDoc.DocumentNode.SelectNodes("//*[@data-ccms-ceid]");

            // Find the region we are updating
            var target = originalEditableDivs.FirstOrDefault(w => w.Attributes["data-ccms-ceid"].Value == model.EditorId);
            if (target != null)
            {
                // Update the region now
                target.InnerHtml = model.Data as string;
            }

            // Now carry over what's being updated to the original.
            article.Content = originalHtmlDoc.DocumentNode.OuterHtml;

            // Make sure we are setting to the orignal updated date/time
            // This is validated to make sure that someone else hasn't already edited this
            // entity
            article.Updated = DateTimeOffset.UtcNow;

            // Save changes back to the database
            var result = await _articleLogic.Save(article, model.UserId);


        }

        /// <summary>
        /// Sends a signal to update editors in the group
        /// </summary>
        /// <param name="editorId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task UpdateEditors(string editorId, string data)
        {
            await Clients.OthersInGroup(editorId).SendCoreAsync("updateEditors", new[] { data });
        }

        private string BaseValidateHtml(string inputHtml)
        {
            if (!string.IsNullOrEmpty(inputHtml))
            {
                var contentHtmlDocument = new HtmlDocument();
                contentHtmlDocument.LoadHtml(HttpUtility.HtmlDecode(inputHtml));
                //if (contentHtmlDocument.ParseErrors.Any())
                //    foreach (var error in contentHtmlDocument.ParseErrors)
                //        modelState.AddModelError(fieldName, error.Reason);

                return contentHtmlDocument.ParsedText.Trim();
            }

            return string.Empty;
        }
    }
}
