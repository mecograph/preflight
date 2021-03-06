﻿using Preflight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Preflight.Services.Interfaces;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Preflight.Api
{
    [PluginController("preflight")]
    public class ApiController : UmbracoAuthorizedApiController 
    { 
        private readonly ISettingsService _settingsService;
        private readonly IContentChecker _contentChecker;

        private IHttpActionResult Error(string message)
        {
            return Ok(new
            {
                status = HttpStatusCode.InternalServerError,
                data = message
            });
        }

        public ApiController(ISettingsService settingsService, IContentChecker contentChecker)
        {
            _settingsService = settingsService;
            _contentChecker = contentChecker;
        }

        /// <summary>
        /// Get Preflight settings object
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getSettings")]
        public IHttpActionResult GetSettings()
        {
            try
            {
                return Ok(new
                {
                    status = HttpStatusCode.OK,
                    data = _settingsService.Get()
                });
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// Get Preflight settings object value
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getSettingValue/{id}")]
        public IHttpActionResult GetSettingValue(string id)
        {
            try
            {
                List<SettingsModel> settings = _settingsService.Get().Settings;
                SettingsModel model = settings.First(s => s.Alias == id);

                return Ok(new
                {
                    status = HttpStatusCode.OK,
                    value = model?.Value
                });
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// Sabve Preflight settings object
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("saveSettings")]
        public IHttpActionResult SaveSettings(PreflightSettings settings)
        {
            try
            {
                return Ok(new
                {
                    status = HttpStatusCode.OK,
                    data = _settingsService.Save(settings)
                });
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// Entry point for all content checking
        /// </summary>
        /// <param name="id">Node id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("check/{id}")]
        public IHttpActionResult Check(int id)
        {            
            try
            {
                IContent content = Services.ContentService.GetById(id);
                bool failed = _contentChecker.CheckContent(content);

                return Ok(new
                {
                    status = HttpStatusCode.OK,
                    failed
                });
            }
            catch(Exception ex)
            {
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// Entry point for checking sub-set of properties
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("checkdirty")]
        public IHttpActionResult CheckDirty(DirtyProperties data)
        {
            try
            {
                bool failed = _contentChecker.CheckDirty(data);

                return Ok(new
                {
                    status = HttpStatusCode.OK,
                    failed
                });
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}
