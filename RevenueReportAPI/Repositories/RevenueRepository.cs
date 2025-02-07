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
                    UserAvatar = u.User.AvatarUrl,
                    u.NumOfBook,
                    u.Amount,
                }).ToListAsync();
        }
        public async Task<IEnumerable<dynamic>> GetDetailServiceRevenue(DateTime? startDate, DateTime? endDate)
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
                    ServiceType = r.ClinicService.Name // Điều chỉnh theo model thực tế
                });

            return await query.ToListAsync();
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
                    ServiceType = r.ClinicService.Name // Điều chỉnh theo model thực tế
                });

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<dynamic>> GetTotalRevenue(int year, int? month)
        {
            var query = _context.DailyRevenueSummaries
                .Where(r => r.Date.Year == year);

            if (month.HasValue)
            {
                query = query.Where(r => r.Date.Month == month.Value);
            }

            return await query.Select(r => new
            {
                r.Date,
                Revenue = r.TotalRevenue
            }).ToListAsync();
        }
    }
}

