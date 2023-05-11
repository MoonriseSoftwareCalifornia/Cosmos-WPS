using AspNetCore.Identity.Services.SendGrid;
using Cosmos.Common.Models;
using Cosmos.Cms.Common.Services.Configurations;
using Cosmos.Cms.Data.Logic;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Cosmos.Cms.Controllers
{
    /// <summary>
    /// Contact Us Controller
    /// </summary>
    //[ResponseCache(NoStore = true)]
    public class ContactUsController : Controller
    {
        private readonly ArticleEditLogic _articleLogic;
        private readonly SendGridEmailSender _emailSender;
        private readonly ILogger<ContactUsController> _logger;
        private readonly IOptions<CosmosConfig> _cosmosOptions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cosmosOptions"></param>
        /// <param name="logger"></param>
        /// <param name="emailSender"></param>
        /// <param name="articleLogic"></param>
        public ContactUsController(
            IOptions<CosmosConfig> cosmosOptions,
            ILogger<ContactUsController> logger,
            IEmailSender emailSender,
            ArticleEditLogic articleLogic)
        {
            _cosmosOptions = cosmosOptions;
            _logger = logger;
            _emailSender = (SendGridEmailSender)emailSender;
            _articleLogic = articleLogic;
        }

        /// <summary>
        /// Contact us email form
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var defaultLayout = await _articleLogic.GetDefaultLayout();
            ViewData["layout"] = defaultLayout;

            return View(new EmailMessageViewModel());
        }


        /// <summary>
        /// Send Email Message
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(EmailMessageViewModel model)
        {

            var defaultLayout = await _articleLogic.GetDefaultLayout();
            ViewData["layout"] = defaultLayout;

            if (ModelState.IsValid)
            {
                try
                {
                    await _emailSender.SendEmailAsync(_cosmosOptions.Value.SendGridConfig.EmailFrom,
                        model.Subject, model.Content);

                    model.SendSuccess = true;
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message, e);
                }
            }

            return View(model);
        }

    }
}
