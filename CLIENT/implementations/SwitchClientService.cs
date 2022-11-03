using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using app_data_client.implementations.models;
using app_data_client.models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace app_data_client.implementations {
    public class SwitchClientService : ISwitchClientService {

        private readonly Configuration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<SwitchClientService> _logger;

        public SwitchClientService(Configuration config, 
                             IHttpClientFactory httpClientFactory, 
                             IMemoryCache memoryCache) => (_config, _httpClientFactory, _memoryCache) = (config, httpClientFactory, memoryCache);

        public SwitchClientService(Configuration config, 
                             IHttpClientFactory httpClientFactory,
                             IMemoryCache memoryCache,
                             ILogger<SwitchClientService> logger): this(config, httpClientFactory, memoryCache) => (_logger) = (logger);


        public Task<bool> getSwitch(string name) {
            return this.getSwitch(name, new CancellationToken());
        }

        public async Task<bool> getSwitch(string name, CancellationToken cancellationToken) {
            _logger.LogDebug("request for switch [{0}]", name);

            // attempt to pull the name/value from the memory
            bool rtnValue = false;
            if (_memoryCache.TryGetValue<bool>(name, out rtnValue)) {
                return rtnValue;
            }

            // if it wasn't in the memory, attempt to get it from an HTTP request
            using (HttpClient client = _httpClientFactory.CreateClient()) {
                client.BaseAddress = new Uri(this._config.Endpoint);
                var httpRequestMessage = new HttpRequestMessage(
                    HttpMethod.Get,
                    string.Format("/switch/{0}", name))
                ;
                if (!String.IsNullOrWhiteSpace(_config.Environment)) {
                    httpRequestMessage.Headers.Add("environment", _config.Environment);
                }
                if (!String.IsNullOrWhiteSpace(_config.Token)) {
                    httpRequestMessage.Headers.Add("Auth-Token", _config.Token);
                }
                try {
                    _logger?.LogDebug("REQUEST SENT {0}::{1}: {2}", this._config.Endpoint, httpRequestMessage.RequestUri, httpRequestMessage.Method);
                    HttpResponseMessage rply = await client.SendAsync(httpRequestMessage, cancellationToken);
                    _logger?.LogDebug("Responce Statuscode {0}", rply.StatusCode);
                    if (rply.IsSuccessStatusCode){
                        string strRplyBody = await rply.Content.ReadAsStringAsync();
                        _logger?.LogDebug("Response content [{0}]", strRplyBody);
                        rtnValue = "true".Equals(strRplyBody?.ToLower());
                    }
                } catch(HttpRequestException exception) {
                    _logger?.LogCritical(exception, "Lookup failed on switch [{0}]", name);
                    return false;
                }  catch(TaskCanceledException exception) {
                    _logger?.LogCritical(exception, "Lookup failed on switch [{0}]", name);
                    return false;
                }

                _memoryCache.Set<bool>(name, rtnValue, TimeSpan.FromSeconds(_config.CacheTimeoutInSeconds));
            }
            return rtnValue;
        }


        public Task<IDictionary<string, bool>> getSwitches(IEnumerable<string> names) {
            return this.getSwitches(names, new CancellationToken());
        }
        public async Task<IDictionary<string, bool>> getSwitches(IEnumerable<string> names, CancellationToken cancellationToken) {
            IList<string> toBeQueried = new List<string>();
            IDictionary<string, bool> rtnValues = new Dictionary<string, bool>();

            // attempt to get the value from the cache
            foreach (string swt in names) {
                bool outVal;
                if (_memoryCache.TryGetValue(swt, out outVal)) {
                    rtnValues.Add(swt, outVal);
                    continue;
                }
                toBeQueried.Add(swt);
            }
            // if any weren't in the cache, look them up.
            if (toBeQueried.Count > 0) {
                using (HttpClient client = _httpClientFactory.CreateClient()) {
                    client.BaseAddress = new Uri(this._config.Endpoint);

                var json = System.Text.Json.JsonSerializer.Serialize(toBeQueried);
                StringContent data = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    var httpRequestMessage = new HttpRequestMessage(
                        HttpMethod.Post,
                        "/switch/lookup") {
                            Content = data
                        }
                    ;
                    if (!String.IsNullOrWhiteSpace(_config.Environment)) {
                        httpRequestMessage.Headers.Add("environment", _config.Environment);
                    }
                    if (!String.IsNullOrWhiteSpace(_config.Token)) {
                        httpRequestMessage.Headers.Add("Auth-Token", _config.Token);
                    }
                    try {
                        _logger?.LogDebug("{3} SENT {0}::{1}: {2}", this._config.Endpoint, httpRequestMessage.RequestUri, json, httpRequestMessage.Method);
                        HttpResponseMessage rply = await client.SendAsync(httpRequestMessage, cancellationToken);
                        _logger?.LogDebug("Responce Statuscode {0}", rply.StatusCode);
                        if (rply.IsSuccessStatusCode){
                            string strRplyBody = await rply.Content.ReadAsStringAsync();
                            _logger?.LogDebug("Response content [{0}]", strRplyBody);
                            SwitchResponse[] objRply = System.Text.Json.JsonSerializer.Deserialize<SwitchResponse[]>(strRplyBody);

                            foreach (var entry in objRply) {
                                rtnValues.Add(entry.Name, entry.Value);
                                _memoryCache.Set<bool>(entry.Name, entry.Value, TimeSpan.FromSeconds(_config.CacheTimeoutInSeconds));
                            }
                        }
                    } catch(HttpRequestException exception) {
                        _logger?.LogCritical(exception, "Lookup failed on names [{0}]", names);
                    }  catch(TaskCanceledException exception) {
                        _logger?.LogCritical(exception, "Lookup failed on names [{0}]", names);
                    }
                }
            }
            return rtnValues;
        }
    }
}