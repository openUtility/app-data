using app_data_core.Models;

namespace app_data_switch.Service;

public interface ISwitchService {

    Task<Tuple<IEnumerable<Switch>, bool>> fetch(Switch[] switches);
    Task<Tuple<IEnumerable<Switch>, bool>> fetch(string[] flags);
    Task<Tuple<bool, bool>> fetch(string flag);
}
