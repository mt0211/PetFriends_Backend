using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

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

        //EXPORT DATA TO EXCEL
        public async Task<List<dynamic>> GetUserBookingSummariesForExport()
        {
            var users = await _context.UserBookingSummaries
            .Include(u => u.User)
            .Select(u => new{
                UserName = u.User.FullName,
                NumberOfBook =  u.NumOfBook,
                Amount = u.Amount,
            }).OrderByDescending(u => u.NumberOfBook)
            .ToListAsync();
            return users.Cast<dynamic>().ToList();
        }

        public async Task<List<dynamic>> GetRevenueForExport(int year, int? month)
        {
            if (month.HasValue && month.Value == 0)
            {
                month = null;
            }
            if (!month.HasValue)
            {
                var revenue = await _context.DailyRevenueSummaries
                .Where(r => r.Date.Year == year)
                .Select(r => new
                {
                    r.Date,
                    Revenue = r.TotalRevenue
                })
                .OrderBy(r=>r.Date)
                .ToListAsync();
                return revenue.Cast<dynamic>().ToList();
            }
            else
            {
                    var revenue = await _context.DailyRevenueSummaries
                    .Where(r => r.Date.Month == month.Value)
                    .Select(r => new
                    {
                        r.Date,
                        Revenue = r.TotalRevenue
                    })
                    .OrderBy(r=>r.Date)
                    .ToListAsync();
                    return revenue.Cast<dynamic>().ToList();
            }
        }

        public async Task<List<dynamic>> GetServiceRevenueForExport(DateTime? startDate, DateTime? endDate)
        {
             DateOnly? start = startDate.HasValue ? DateOnly.FromDateTime(startDate.Value) : null;
            DateOnly? end = endDate.HasValue ? DateOnly.FromDateTime(endDate.Value) : null;
            var query = await _context.ServiceRevenues
                            .Where(r => (!startDate.HasValue || r.Date >= start) &&
                                       (!endDate.HasValue || r.Date <= end))
                            .Select(r => new
                            {
                                r.Date,
                                r.Revenue,
                                ServiceType = r.ClinicService.Name // Điều chỉnh theo model thực tế
                            }).OrderBy(r=>r.Date)
            .ToListAsync();
            return query.Cast<dynamic>().ToList();
        }

        public async Task<(List<dynamic> userbookingsummaries, List<dynamic> revenues, List<dynamic> servicerevenue)> GetAllDataForExport(int year, int? month,DateTime? startDate, DateTime? endDate)
        {
            var UserBookingSummaries = await GetUserBookingSummariesForExport();
            var Revenue = await GetRevenueForExport(year, month);
            var ServiceRevenue = await GetServiceRevenueForExport(startDate, endDate);
            return (UserBookingSummaries, Revenue, ServiceRevenue);
        }

       
    }
}

