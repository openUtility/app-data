using Microsoft.AspNetCore.Mvc;
using app_data_core.Models;
using MySqlConnector;

namespace app_data_dashboard.Controllers;

[ApiController]
[Route("[controller]")]
public class SwitchController : ControllerBase
{


    private MySqlConnection _connection;
    private readonly ILogger<SwitchController> _logger;

    public SwitchController(ILogger<SwitchController> logger,
                            MySqlConnection connection) => (_logger, _connection) = (logger, connection);

    [HttpPost]
    public async Task<IActionResult> Post([FromBody]string name, [FromBody]bool value, [FromBody]string? tag) {
        string sql = @"
            INSERT INTO switches (switch, value) VALUES (@switch, @value)
            ON DUPLICATE KEY UPDATE value = VALUES(value);
        ";
        try {
            using (var command = new MySqlCommand(sql, this._connection))
            {
                string postName = name;
                if (String.IsNullOrWhiteSpace(tag) || !"production".Equals(tag.ToLower())) {
                    postName += ":" + tag;
                }
                command.Parameters.Add(new MySqlParameter("switch", postName));
                command.Parameters.Add(new MySqlParameter("value", value));
                int rtply = await command.ExecuteNonQueryAsync();
                if (rtply < 1) {
                    return new StatusCodeResult(StatusCodes.Status417ExpectationFailed);
                }
            }
        } catch (Exception ex) {
            this._logger.LogCritical(ex, "Failed Database connection");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
        return new StatusCodeResult(StatusCodes.Status204NoContent);
    }


    [HttpGet]
    public async Task<IEnumerable<Switch>> Get()
    {
        IDictionary<string, Switch> groupList = new Dictionary<string, Switch>();
        try
        {
            await this._connection.OpenAsync();

            using (var command = new MySqlCommand("SELECT switch, value FROM switches", this._connection))
            {

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string sName = reader.GetString(0) ?? "";
                        bool sValue = reader.GetBoolean(1);

                        if (!String.IsNullOrWhiteSpace(sName))
                        {

                            string[] arrObj = sName.Split(':');
                            if (groupList.ContainsKey(arrObj[0])) {
                                Switch actSwitch = groupList[arrObj[0]];
                                if (arrObj.Count() > 1 && !String.IsNullOrWhiteSpace(arrObj[1]))
                                {   
                                    if (actSwitch.Tags == null) {
                                        actSwitch.Tags = new Dictionary<string, bool>();
                                    }
                                    actSwitch.Tags.Add(arrObj[1], sValue);
                                } else {
                                    actSwitch.Value = sValue;
                                }
                            } else {
                                Switch obj = (arrObj.Count() > 1 && !String.IsNullOrWhiteSpace(arrObj[1])) ?
                                    new Switch(arrObj[0], arrObj[1], sValue) :
                                    new Switch(arrObj[0], sValue);
                                groupList.Add(arrObj[0], obj);
                            }
                        }
                    }
                }

            }

        }
        catch (Exception ex)
        {
            this._logger.LogCritical(ex, "Failed Database connection");
        }


        return groupList.Values;
    }
}
