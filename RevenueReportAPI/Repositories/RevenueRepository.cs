using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using System;

namespace RevenueReportAPI.Repositories
{
    public class RevenueRepository : IRevenueRepository
    {
        private readonly PetfriendsContext _context;
        public RevenueRepository(PetfriendsContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<dynamic>> GetUserBookingSummaries()
        {
            return await _context.UserBookingSummaries
                .Include(u => u.User)
                .Select(u => new
                {
                    u.Id,
                    UserName = u.User.FullName,
                    u.NumOfBook,
                    u.Amount,
                }).ToListAsync();
        }
        public async Task<IEnumerable<dynamic>> GetServiceRevenue(DateTime? startDate, DateTime? endDate)
        {
            DateOnly? start = startDate.HasValue ? DateOnly.FromDateTime(startDate.Value) : null;
            DateOnly? end = endDate.HasValue ? DateOnly.FromDateTime(endDate.Value) : null;
            var query = _context.ServiceRevenues
                .Where(r => (!startDate.HasValue || r.Date >= start) &&
                           (!endDate.HasValue || r.Date <= end))
                .Select(r => new
                {
                    r.Id,
                    r.ClinicServiceId,
                    r.Date,
                    r.Revenue,
                    r.CreatedAt,
                    r.UpdatedAt,
                    // Assuming there's a navigation property to get service type
                    ServiceType = r.ClinicService.Name // You might need to adjust this based on your actual model
                });

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<dynamic>> GetTotalRevenue(DateTime? startDate, DateTime? endDate)
        {
            DateOnly? start = startDate.HasValue ? DateOnly.FromDateTime(startDate.Value) : null;
            DateOnly? end = endDate.HasValue ? DateOnly.FromDateTime(endDate.Value) : null;
            var query = _context.DailyRevenueSummaries
                .Where(r => (!startDate.HasValue || r.Date >= start) &&
                           (!endDate.HasValue || r.Date <= end))
                .Select(r => new
                {
                    r.Id,
                    r.Date,
                    Revenue = r.TotalRevenue,
                    r.CreatedAt,
                    r.UpdatedAt
                });

            return await query.ToListAsync();
        }
    }
}

