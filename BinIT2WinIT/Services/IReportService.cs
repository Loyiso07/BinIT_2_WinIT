using System.Threading.Tasks;

namespace BinIT2WinIT.Services
{
    public interface IReportService
    {
        Task<object> GenerateMonthlyReport(int month, int year);
        Task<object> GenerateUserImpactReport(string userId);
        Task<object> GetCommunityLeaderboard();
    }
}