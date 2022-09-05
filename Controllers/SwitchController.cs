using System.Data.Common;
using System.Text.RegularExpressions;
using app_data_switch.config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MySqlConnector;

namespace app_data_switch.Controllers;

[ApiController]
[Route("[controller]")]
public class SwitchController : ControllerBase {
    private static readonly Regex regexKeyPattern = new Regex("[^a-zA-Z0-9-:]");


    private readonly IMemoryCache _memoryCache;

    private MySqlConnection _connection;

    private SwitchEndpointConfiguration _configruation;

    private readonly ILogger<SwitchController> _logger;

    public SwitchController(
        ILogger<SwitchController> logger,
        IMemoryCache memoryCache,
        MySqlConnection connection,
        SwitchEndpointConfiguration configuration
        ) {
        _logger = logger;
        _memoryCache = memoryCache;
        _connection = connection;
        _configruation = configuration;

    }


    /// <summary>
    /// This will clean up and attach the :dev,test,prod suffexes on the key
    /// Cleanup items...
    ///  * all lower case
    ///  * attaches the env suffix's (except prod)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="EnvStr"></param>
    /// <returns></returns>
    private string CleanUpKey(string key, string envStr) {

        // make sure it's lowercase
        string rtnValue = key.ToLowerInvariant();
        

        // 11 is just a magic number so we don't let env tages get to long...
        if (!String.IsNullOrWhiteSpace(envStr) && envStr.Length < 11) {
            // remove all the non alpha numberic letters... / symbols. 
            rtnValue += ":" + envStr.ToLowerInvariant();
        }

        // remove all unsafe characters...
        rtnValue = SwitchController.regexKeyPattern.Replace(rtnValue, String.Empty);

        // strip off PROD, or PRODUCTION
        if (rtnValue.EndsWith(":prod")){
            rtnValue = rtnValue.Substring(0, rtnValue.Length -5);
        } else if (rtnValue.EndsWith(":production")) {
            rtnValue = rtnValue.Substring(0, rtnValue.Length -11);
        }

        return rtnValue;
    }

    /// <summary>
    /// This will return the IP address for the requested item. 
    /// </summary>
    /// <returns></returns>
    private string FindIPAddress() {
        string rtnIP = Request.Headers["HTTP_X_FORWARDED_FOR"];
        if (String.IsNullOrWhiteSpace(rtnIP)) {
            rtnIP = Request.Headers["REMOTE_ADDR"];
        }
        if (String.IsNullOrWhiteSpace(rtnIP)) {
            rtnIP = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        }
        return rtnIP;
    }

    [HttpGet("{id}")]
    public async Task<Boolean> Get(string id) {
        bool rtnValue = false;
        // security - we want to controll how much memory we are consuming with 
        // memory cache - if the key length > X just kick out. 
        if (String.IsNullOrWhiteSpace(id) || id.Length > this._configruation.allowedIDLength) {
            return false;
        }

        // request check, 
        // lets make sure this user isn't just brute forcing our keys, or 
        // attempting a DOS attack...
        int badAttempt = 0;
        string ip = this.FindIPAddress();
        _memoryCache.TryGetValue(ip, out badAttempt);

        if (badAttempt > this._configruation.lockoutAfterXAttempts) {
            return false;
        }

        // attempt to attach the environment. 
        string switchKey = this.CleanUpKey(id, Request.Headers["environment"]);

        // if the memory cache doesn't have the key, then look it up
        if (!_memoryCache.TryGetValue(switchKey, out rtnValue)) {
            try {
                bool successfullQuery = false;
                await this._connection.OpenAsync();

                using var command = new MySqlCommand("SELECT value FROM switches WHERE switch=@switch;", this._connection);
                command.Parameters.AddWithValue("switch", switchKey);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync()) {
                    successfullQuery = true;
                    rtnValue = reader.GetBoolean(0);
                }

                // additional Check... IF user keeps looking up DB calls that are fruitless
                // lets lock them out... as they might be tring to do a DOS attack..
                if (!successfullQuery) {                
                    badAttempt++;
                    if (!String.IsNullOrWhiteSpace(ip)) {
                        _memoryCache.Set(ip, badAttempt, TimeSpan.FromMinutes(this._configruation.CacheLockoutCountForXMinutes));
                    }
                }
            } catch (Exception ex) {
                this._logger.LogCritical(ex, "Failed Database connection");
            } 
            _memoryCache.Set(switchKey, rtnValue, TimeSpan.FromSeconds(this._configruation.CacheQueriesForXSeconds));
        }
        return rtnValue;
    }
}
