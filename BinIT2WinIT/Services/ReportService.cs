using BinIT2WinIT.Data;
using SmartRecycling.Data;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace BinIT2WinIT.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService()
        {
            _context = new ApplicationDbContext();
        }

        public async Task<object> GenerateMonthlyReport(int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var submissions = await _context.RecyclingSubmissions
                .Include(s => s.MaterialType)
                .Where(s => s.SubmissionDate >= startDate && s.SubmissionDate < endDate)
                .ToListAsync();

            var totalWeight = submissions.Sum(s => s.Weight);
            var totalPoints = submissions.Sum(s => (int)(s.Weight * s.MaterialType.PointsRates.FirstOrDefault(p => p.IsActive)?.PointsPerKg ?? 0));

            return new
            {
                Month = month,
                Year = year,
                TotalSubmissions = submissions.Count,
                TotalWeight = totalWeight,
                TotalPoints = totalPoints,
                Submissions = submissions
            };
        }

        public async Task<object> GenerateUserImpactReport(string userId)
        {
            var resident = await _context.Residents
                .Include(r => r.Submissions)
                .Include(r => r.PointsTransactions)
                .FirstOrDefaultAsync(r => r.UserId == userId);

            return resident;
        }

        public async Task<object> GetCommunityLeaderboard()
        {
            var topResidents = await _context.Residents
                .OrderByDescending(r => r.PointsBalance)
                .Take(10)
                .ToListAsync();

            return topResidents;
        }
    }
}