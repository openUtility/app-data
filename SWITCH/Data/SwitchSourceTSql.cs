using app_data_core.Models;
using MySqlConnector;

namespace app_data_switch.Data;

public class SwitchSourceTSql : ISwitchSource {


    private MySqlConnection _connection;

    private readonly ILogger<SwitchSourceTSql>? _logger;

    public SwitchSourceTSql(MySqlConnection connection) => (_connection) = connection;

    public SwitchSourceTSql(MySqlConnection connection, ILogger<SwitchSourceTSql> logger) : this(connection) => (_logger) = (logger);

    public async Task<IEnumerable<KeyValuePair<string, bool?>>> fetch(string[] flags) {
        IDictionary<string, bool?> rtnList = flags.ToDictionary(x => x, x => (bool?)null, StringComparer.OrdinalIgnoreCase);
        
        var flagCount = flags.Select((x,y) => "@switch" + y.ToString())
                        .Aggregate<string>((x, y) => x + "," + y)
                        .DefaultIfEmpty();
        await this._connection.OpenAsync();

        using var command = new MySqlCommand(
            String.Format("SELECT switch, value FROM switches WHERE switch IN ({0});",
                        flagCount),
        this._connection);
        
        for(int i = 0, len = flags.Count(); i < len; i++) {
            command.Parameters.AddWithValue($"switch{i}", flags[i]);
        }

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) {
            string swt = reader.GetString(0);
            // not checking
            rtnList[swt] = reader.GetBoolean(0);
        }
        return rtnList;
    }

    public async Task<bool?> fetch(string flag) {
        bool? rtnValue = null;
        await this._connection.OpenAsync();

        using var command = new MySqlCommand(
            "SELECT value FROM switches WHERE switch=@switch;",
            this._connection);
        command.Parameters.AddWithValue("switch", flag);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync()) {
            rtnValue = reader.GetBoolean(0);
        }
        return rtnValue;
    }
}