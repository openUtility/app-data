using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace app_data_client {

    public interface ISwitchClientService {




        Task<bool> getSwitch(string name);
        Task<bool> getSwitch(string name, CancellationToken cancellationToken);

        Task<IDictionary<string, bool>> getSwitches(IEnumerable<string> names);
        Task<IDictionary<string, bool>> getSwitches(IEnumerable<string> names, CancellationToken cancellationToken);

    }
}