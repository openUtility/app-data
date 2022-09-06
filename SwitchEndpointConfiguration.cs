namespace app_data_switch.config;

public class SwitchEndpointConfiguration {
    public double CacheQueriesForXSeconds { get; set; }
    public double CacheLockoutCountForXMinutes { get; set; }
    public int lockoutAfterXAttempts { get; set; }
    public int allowedIDLength { get; set; }
}
