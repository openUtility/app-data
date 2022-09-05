namespace app_data_switch.config;

public class SwitchEndpointConfiguration {
    public double CacheQueriesForXSeconds { get; set; } = 10;
    public double CacheLockoutCountForXMinutes { get; set; } = 5;
    public int lockoutAfterXAttempts { get; set; } = 1000;
    public int allowedIDLength { get; set; } = 25;
}
