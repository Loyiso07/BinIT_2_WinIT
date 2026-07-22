using BinIT2WinIT.Data;
using BinIT2WinIT.Models;
using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace BinIT2WinIT.Services
{
    public class PointsService : IPointsService
    {
        private readonly ApplicationDbContext _context;

        public PointsService()
        {
            _context = new ApplicationDbContext();
        }

        public async Task<int> CalculatePoints(int materialTypeId, double weight)
        {
            var rate = await _context.PointsRates
                .FirstOrDefaultAsync(p => p.MaterialTypeId == materialTypeId && p.IsActive);

            if (rate == null)
                return 0;

            return (int)(weight * rate.PointsPerKg);
        }

        public async Task<int> AwardPoints(int residentId, int submissionId)
        {
            var submission = await _context.RecyclingSubmissions
                .Include(s => s.MaterialType)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null)
                throw new Exception("Submission not found");

            var points = await CalculatePoints(submission.MaterialTypeId, submission.Weight);

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    submission.Status = "Confirmed";
                    submission.VerifiedDate = DateTime.Now;

                    var pointsTransaction = new PointsTransaction
                    {
                        ResidentId = residentId,
                        Amount = points,
                        Description = $"Recycling verified: {submission.Weight}kg of {submission.MaterialType.Name}",
                        Type = "Earn",
                        ReferenceId = submissionId,
                        TransactionDate = DateTime.Now
                    };
                    _context.PointsTransactions.Add(pointsTransaction);

                    var resident = await _context.Residents
                        .FirstOrDefaultAsync(r => r.ResidentId == residentId);

                    if (resident != null)
                    {
                        resident.PointsBalance += points;
                        resident.TotalCO2Saved += await CalculateCO2Saved(submission.MaterialTypeId, submission.Weight);
                    }

                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    return points;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<double> CalculateCO2Saved(int materialTypeId, double weight)
        {
            var factor = await _context.CO2Factors
                .FirstOrDefaultAsync(c => c.MaterialTypeId == materialTypeId && c.IsActive);

            if (factor == null)
                return 0;

            return weight * factor.CO2SavedPerKg;
        }

        public async Task<int> AwardWelcomeBonus(int residentId)
        {
            var config = await _context.SystemConfigurations
                .FirstOrDefaultAsync(c => c.ConfigKey == "WelcomeBonusPoints");

            var bonusPoints = config != null ? int.Parse(config.ConfigValue) : 100;

            var transaction = new PointsTransaction
            {
                ResidentId = residentId,
                Amount = bonusPoints,
                Description = "Welcome Bonus - Thank you for joining!",
                Type = "WelcomeBonus",
                TransactionDate = DateTime.Now
            };

            _context.PointsTransactions.Add(transaction);

            var resident = await _context.Residents
                .FirstOrDefaultAsync(r => r.ResidentId == residentId);

            if (resident != null)
            {
                resident.PointsBalance += bonusPoints;
            }

            await _context.SaveChangesAsync();

            return bonusPoints;
        }
    }
}