using System.Threading.Tasks;

namespace BinIT2WinIT.Services
{
    public interface IReferralService
    {
        Task<string> GenerateReferralCode(string userId);
        Task<bool> ProcessReferral(string referralCode, string newUserId);
        Task<int> GetInfluencerPoints(string userId);
        Task<int> GetTotalReferrals(string userId);
    }
}