using app_data_core.Models;
using app_data_switch.config;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;
using app_data_switch.Data;

namespace app_data_switch.Service;

public class SwitchService : ISwitchService {
    private static readonly Regex regexKeyPattern = new Regex("[^a-zA-Z0-9-:]");


    private readonly IMemoryCache _memoryCache;

    private SwitchEndpointConfiguration _configruation;

    private ISwitchSource _switchSource;

    private readonly ILogger<SwitchService>? _logger;

    public SwitchService(ISwitchSource switchSource,
                         IMemoryCache memoryCache,
                         SwitchEndpointConfiguration configuration) => (_switchSource, _memoryCache, _configruation) = (switchSource, memoryCache, configuration);

    public SwitchService(ISwitchSource switchSource,
                         IMemoryCache memoryCache,
                         SwitchEndpointConfiguration configuration,
                         ILogger<SwitchService> logger) : this(switchSource, memoryCache, configuration) => (_logger) = (logger);

    public Task<Tuple<IEnumerable<Switch>, bool>> fetch(Switch[] switches) {
        throw new NotImplementedException();
    }

    public async Task<Tuple<IEnumerable<Switch>, bool>> fetch(string[] flags) {
        IList<Switch> rtnList = new List<Switch>();
        IList<string> toBeQueried = new List<string>();
        bool hasFailedCall = false;

        foreach (string flg in flags) {
            bool rtnVal;
            if (_memoryCache.TryGetValue(flg, out rtnVal)) {
                rtnList.Add(new Switch(flg, rtnVal));
                continue;
            }
            toBeQueried.Add(flg);
        }
        if (toBeQueried.Count() > 0) {
            foreach (var dta in await _switchSource.fetch(toBeQueried.ToArray())) {
                if (!dta.Value.HasValue) {
                    hasFailedCall = true;
                }
                rtnList.Add(new Switch(dta.Key, dta.Value ?? false));
                _memoryCache.Set<bool>(dta.Key, dta.Value ?? false, TimeSpan.FromSeconds(this._configruation.CacheQueriesForXSeconds));
            }
        }
        return new Tuple<IEnumerable<Switch>, bool>(rtnList, hasFailedCall);
    }

    public async Task<Tuple<bool, bool>> fetch(string flag) {
        bool hasValue = true;
        bool rtnValue;
        if (!_memoryCache.TryGetValue<bool>(flag, out rtnValue)) {
            bool? rply = await _switchSource.fetch(flag);
            rtnValue = rply ?? false;
            hasValue = rply.HasValue;
        }
        return new Tuple<bool, bool>(rtnValue, hasValue);
    }

}
