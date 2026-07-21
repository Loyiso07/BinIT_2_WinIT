using System.Threading.Tasks;

namespace BinIT2WinIT.Services
{
    public interface IPointsService
    {
        Task<int> CalculatePoints(int materialTypeId, double weight);
        Task<int> AwardPoints(int residentId, int submissionId);
        Task<double> CalculateCO2Saved(int materialTypeId, double weight);
        Task<int> AwardWelcomeBonus(int residentId);
    }
}