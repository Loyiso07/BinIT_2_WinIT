using BinIT2WinIT.Data;
using BinIT2WinIT.Models;
using SmartRecycling.Data;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace BinIT2WinIT.Services
{
    public class ReferralService : IReferralService
    {
        private readonly ApplicationDbContext _context;

        public ReferralService()
        {
            _context = new ApplicationDbContext();
        }

        public async Task<string> GenerateReferralCode(string userId)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string code;

            // Ensure code is unique
            do
            {
                code = new string(Enumerable.Repeat(chars, 8)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
            while (await _context.Residents.AnyAsync(r => r.ReferralCode == code));

            return code;
        }

        public async Task<bool> ProcessReferral(string referralCode, string newUserId)
        {
            var referrer = await _context.Residents
                .FirstOrDefaultAsync(r => r.ReferralCode == referralCode);

            if (referrer == null)
                return false;

            var newResident = await _context.Residents
                .FirstOrDefaultAsync(r => r.UserId == newUserId);

            if (newResident == null)
                return false;

            // Get influencer points from config
            var config = await _context.SystemConfigurations
                .FirstOrDefaultAsync(c => c.ConfigKey == "InfluencerPointsPerReferral");

            var points = config != null ? int.Parse(config.ConfigValue) : 50;

            // Update referrer
            referrer.InfluencerPoints += points;
            referrer.TotalReferrals += 1;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<int> GetInfluencerPoints(string userId)
        {
            var resident = await _context.Residents
                .FirstOrDefaultAsync(r => r.UserId == userId);

            return resident?.InfluencerPoints ?? 0;
        }

        public async Task<int> GetTotalReferrals(string userId)
        {
            var resident = await _context.Residents
                .FirstOrDefaultAsync(r => r.UserId == userId);

            return resident?.TotalReferrals ?? 0;
        }
    }
}