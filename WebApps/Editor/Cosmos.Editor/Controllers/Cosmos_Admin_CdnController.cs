﻿using Azure.ResourceManager;
using Azure.ResourceManager.Cdn;
using Azure.ResourceManager.Resources;
using Cosmos.Common.Data;
using Cosmos.Cms.Common.Services.Configurations;
using Cosmos.Cms.Models;
using Cosmos.Cms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cosmos.Cms.Controllers
{
    /// <summary>
    /// Cosmos Systems Administrator Controller
    /// </summary>
    //[ResponseCache(NoStore = true)]
    [Authorize(Roles = "Administrators, Editors")]
    public class Cosmos_Admin_CdnController : Controller
    {
        private readonly ILogger<Cosmos_Admin_CdnController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IOptions<CosmosConfig> _options;
        private readonly AzureSubscription _azureSubscription;

        /// <summary>
        /// CDN Service Name
        /// </summary>
        public static string CDNSERVICENAME = "CDN";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dbContext"></param>
        /// <param name="options"></param>
        /// <param name="azureSubscription"></param>
        public Cosmos_Admin_CdnController(ILogger<Cosmos_Admin_CdnController> logger,
           ApplicationDbContext dbContext,
           IOptions<CosmosConfig> options,
           AzureSubscription azureSubscription
        )
        {
            _logger = logger;
            _dbContext = dbContext;
            _options = options;
            _azureSubscription = azureSubscription;
        }

        /// <summary>
        /// Gets the CDN Integration Status
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var model = await _dbContext.Settings.FirstOrDefaultAsync(f => f.Name == CDNSERVICENAME);

            if (model == null)
            {
                return View(null);
            }

            return View(JsonConvert.DeserializeObject<AzureCdnEndpoint>(model.Value));
        }

        /// <summary>
        /// Disable CDN integration
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> DisableCdn()
        {
            var oldSetting = await _dbContext.Settings.FirstOrDefaultAsync(f => f.Name == CDNSERVICENAME);

            if (oldSetting != null)
            {
                _dbContext.Settings.Remove(oldSetting);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// CDN Selection
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> EnableCdn()
        {
            return View(await GetEndPoints());
        }

        private async Task<List<AzureCdnEndpoint>> GetEndPoints()
        {
            var model = new List<AzureCdnEndpoint>();

            try
            {
                var url = new Uri(_options.Value.SiteSettings.PublisherUrl);
                ViewData["Publisher"] = url;

                //var sub = client.GetDefaultSubscriptionAsync();

                SubscriptionResource subscription = _azureSubscription.Subscription;
                ResourceGroupCollection resourceGroups = subscription.GetResourceGroups();

                var data = resourceGroups.GetAllAsync();

                await foreach (var group in data)
                {
                    var profiles = group.GetProfiles();
                    foreach (var profile in profiles)
                    {
                        var endpoints = profile.GetCdnEndpoints();
                        foreach (var end in endpoints)
                        {
                            foreach (var origin in end.Data.Origins)
                            {
                                model.Add(new AzureCdnEndpoint()
                                {
                                    EndPointId = end.Id,
                                    CdnProfileName = profile.Data.Name,
                                    EndPointHostName = origin.HostName,
                                    EndpointName = end.Data.Name,
                                    ResourceGroupName = group.Data.Name,
                                    SkuName = profile.Data.SkuName.Value.ToString(),
                                    SubscriptionId = subscription.Id
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }

            return model;
        }

        /// <summary>
        /// Enables CDN Integration
        /// </summary>
        /// <param name="EndPointId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EnableCdn([FromForm] string EndPointId)
        {
            var endpoints = await GetEndPoints();

            var endpoint = endpoints.FirstOrDefault(f => f.EndPointId == EndPointId);

            if (endpoint == null)
            {
                return NotFound();
            }

            var oldSetting = await _dbContext.Settings.FirstOrDefaultAsync(f => f.Name == CDNSERVICENAME);

            if (oldSetting != null)
            {
                _dbContext.Settings.Remove(oldSetting);
                await _dbContext.SaveChangesAsync();
            }

            var cdnSetting = new Setting()
            {
                Name = CDNSERVICENAME,
                Description = "CDN Integration. Type: " + endpoint.SkuName + ".",
                Group = "Performance",
                IsRequired = false,
                Value = JsonConvert.SerializeObject(endpoint)
            };

            _dbContext.Settings.Add(cdnSetting);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
