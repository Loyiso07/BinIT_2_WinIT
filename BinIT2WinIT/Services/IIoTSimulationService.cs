using System.Threading.Tasks;

namespace BinIT2WinIT.Services
{
    public interface IIoTSimulationService
    {
        Task SimulateBinData();
        Task CheckAndSendAlerts();
    }
}