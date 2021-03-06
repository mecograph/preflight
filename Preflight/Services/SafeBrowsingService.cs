﻿using HtmlAgilityPack;
using Newtonsoft.Json;
using Preflight.Constants;
using Preflight.Extensions;
using Preflight.Models;
using Preflight.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using Umbraco.Core;

namespace Preflight.Services
{
    public class SafeBrowsingService : ISafeBrowsingService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public List<BrokenLinkModel> Check(string text, string apiKey)
        {
            List<BrokenLinkModel> response = new List<BrokenLinkModel>();

            if (!apiKey.HasValue()) return response;

            var doc = new HtmlDocument();
            doc.LoadHtml(text);

            HtmlNodeCollection links = doc.DocumentNode.SelectNodes(KnownStrings.HrefXPath);

            if (links == null || !links.Any()) return response;

            string[] hrefs = links.Select(l => l.GetAttributeValue("href", string.Empty))
                .Where(l => l.StartsWith("http")).ToArray();

            // check for cached responses - avoids request when page is being resaved
            MemoryCache cache = MemoryCache.Default;
            List<BrokenLinkModel> fromCache = new List<BrokenLinkModel>();

            foreach (string href in hrefs)
            {
                var cacheItem = (BrokenLinkModel)cache.Get(KnownStrings.CacheKey + href);
                if (null == cacheItem) continue;

                fromCache.Add(cacheItem);
                hrefs = hrefs.Except(href.AsEnumerableOfOne()).ToArray();
            }


            SafeBrowsingResponseModel safeBrowsingResult = SafeBrowsingLookup(hrefs, apiKey);

            if (safeBrowsingResult.Matches.Any())
            {
                response.AddRange(safeBrowsingResult.Matches.Select(m => new BrokenLinkModel
                {
                    Href = m.Threat.Url,
                    Status = m.ThreatType,
                    Unsafe = true,
                    Text = links.First(l => l.GetAttributeValue("href", string.Empty) == m.Threat.Url)
                        .InnerText
                }));

                foreach (BrokenLinkModel item in response)
                {
                    cache.Add(KnownStrings.CacheKey + item.Href, item,
                        new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(3600) });
                }
            }

            // add cached results
            response.AddRange(fromCache);

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="apiKey"></param>
        private static SafeBrowsingResponseModel SafeBrowsingLookup(IEnumerable<string> urls, string apiKey)
        {
            try
            {
                string result;
                string url = KnownStrings.SafeBrowsingUrl + apiKey;

                ThreatEntry[] threatEntries = urls.Select(u => new ThreatEntry { Url = u }).ToArray();

                var requestModel = new SafeBrowsingRequestModel
                {
                    Client = new Client
                    {
                        ClientId = "preflight-test",
                        ClientVersion = "0.0.0"
                    },
                    ThreatInfo = new ThreatInfo
                    {
                        ThreatTypes = new[] { "MALWARE", "SOCIAL_ENGINEERING" },
                        PlatformTypes = new[] { "WINDOWS" },
                        ThreatEntryTypes = new[] { "URL" },
                        ThreatEntries = threatEntries
                    }
                };

                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    result = client.UploadString(url, "POST", JsonConvert.SerializeObject(requestModel));
                }

                return JsonConvert.DeserializeObject<SafeBrowsingResponseModel>(result);
            }
            catch (Exception)
            {
                return new SafeBrowsingResponseModel();
            }
        }
    }
}
