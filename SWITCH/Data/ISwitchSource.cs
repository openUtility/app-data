namespace app_data_switch.Data;

public interface ISwitchSource {
    Task<IEnumerable<KeyValuePair<string, bool?>>> fetch(string[] flags);
    Task<bool?> fetch(string flag);
}