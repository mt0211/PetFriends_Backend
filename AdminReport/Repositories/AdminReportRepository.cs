using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AdminReport.Repositories
{
    public class AdminReportRepository : IAdminReportRepository
    {
        private readonly PetfriendsContext _context;
        public AdminReportRepository(PetfriendsContext context)
        {
            _context = context;
        }

        //add method here:
        //

        public async Task<IEnumerable<dynamic>> GetDataUserStatus(int year, int? month)
        {
            // Kiểm tra nếu month là 0, thì gán lại thành null
            if (month.HasValue && month.Value == 0)
            {
                month = null;
            }

            if (!month.HasValue)
            {
                // Lấy dữ liệu theo năm (12 tháng)
                var userStatistics = await _context.Users
                    .Where(u => u.LastLoggedIn.HasValue && u.LastLoggedIn.Value.Year == year)
                    .GroupBy(u => u.LastLoggedIn.Value.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        ActiveUsers = g.Count(u => u.Status == "ACTIVE"),
                        InactiveUsers = g.Count(u => u.Status == "INACTIVE")
                    })
                    .ToListAsync();

                var months = new[]
                {
                     "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
                };

                return months.Select((m, index) => new List<dynamic>
                {
                    new { month = m, type = "Active Users", value = userStatistics.FirstOrDefault(x => x.Month == index + 1)?.ActiveUsers ?? 0 },
                    new { month = m, type = "Inactive Users", value = userStatistics.FirstOrDefault(x => x.Month == index + 1)?.InactiveUsers ?? 0 }
                }).SelectMany(x => x);
            }

            // Nếu month hợp lệ => Lấy dữ liệu theo ngày
            int daysInMonth = DateTime.DaysInMonth(year, month.Value);
            var dailyStatistics = await _context.Users
                .Where(u => u.LastLoggedIn.HasValue &&
                            u.LastLoggedIn.Value.Year == year &&
                            u.LastLoggedIn.Value.Month == month.Value)
                .GroupBy(u => u.LastLoggedIn.Value.Day)
                .Select(g => new
                {
                    Day = g.Key,
                    ActiveUsers = g.Count(u => u.Status == "ACTIVE"),
                    InactiveUsers = g.Count(u => u.Status == "INACTIVE")
                })
                .ToListAsync();

            return Enumerable.Range(1, daysInMonth).SelectMany(d => new List<dynamic>
                {
                    new { day = d.ToString("D2"), type = "Active Users", value = dailyStatistics.FirstOrDefault(x => x.Day == d)?.ActiveUsers ?? 0 },
                    new { day = d.ToString("D2"), type = "Inactive Users", value = dailyStatistics.FirstOrDefault(x => x.Day == d)?.InactiveUsers ?? 0 }
                });
        }

        public async Task<IEnumerable<dynamic>> GetDataNewUser(int year, int? month)
        {
            // Kiểm tra nếu month là 0, thì gán lại thành null
            if (month.HasValue && month.Value == 0)
            {
                month = null;
            }

            if (!month.HasValue)
            {
                // Lấy dữ liệu theo năm (12 tháng)
                var userStatistics = await _context.Users
                    .Where(u => u.CreatedAt.HasValue && u.CreatedAt.Value.Year == year)
                    .GroupBy(u => u.CreatedAt.Value.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        NewUsers = g.Count()
                    })
                    .ToListAsync();

                var months = new[]
                {
            "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
        };

                return months.Select((m, index) => new
                {
                    month = m,
                    type = "New Users",
                    value = userStatistics.FirstOrDefault(x => x.Month == index + 1)?.NewUsers ?? 0
                });
            }

            // Nếu month hợp lệ => Lấy dữ liệu theo ngày
            int daysInMonth = DateTime.DaysInMonth(year, month.Value);
            var dailyStatistics = await _context.Users
                .Where(u => u.CreatedAt.HasValue &&
                            u.CreatedAt.Value.Year == year &&
                            u.CreatedAt.Value.Month == month.Value)
                .GroupBy(u => u.CreatedAt.Value.Day)
                .Select(g => new
                {
                    Day = g.Key,
                    NewUsers = g.Count()
                })
                .ToListAsync();

            return Enumerable.Range(1, daysInMonth).Select(d => new
            {
                day = d.ToString("D2"),
                type = "New Users",
                value = dailyStatistics.FirstOrDefault(x => x.Day == d)?.NewUsers ?? 0
            });
        }

        public async Task<IEnumerable<dynamic>> GetPostDistribution()
        {
            var totalPosts = await _context.ForumPosts.CountAsync();

            if (totalPosts == 0)
            {
                return new List<dynamic>
        {
            new { type = "Approved", value = 0 },
            new { type = "Rejected", value = 0 },
             new { type = "Pending", value = 0 },
        };
            }

            var statusCounts = await _context.ForumPosts
                .GroupBy(p => p.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();
            var result = new List<dynamic>
        {
            new { type = "Approved", value = statusCounts.FirstOrDefault(x => x.Status == 2)?.Count * 100 / totalPosts ?? 0 },
            new { type = "Rejected", value = statusCounts.FirstOrDefault(x => x.Status == 0)?.Count * 100 / totalPosts ?? 0 },
            new { type = "Pending", value = statusCounts.FirstOrDefault(x => x.Status == 0)?.Count * 100 / totalPosts ?? 0 }
        };

            return result;
        }

        public async Task<User> GetUserByID(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return null;
            }
            return user;
        }

        

        public async Task<List<dynamic>> GetTopVaccine()
        {
            var count = await _context.UserPetVaccines
            .Where(upv => upv.VaccineId != null)
             .Select(upv => new 
            { upv.Name
            , upv.NumberOfDoses
            })
            .Distinct()
            .OrderByDescending(upv => upv.NumberOfDoses)
            .ToListAsync();
            return  count.Cast<dynamic>().ToList();
        }

        public async Task<(List<dynamic> vaccineData, List<dynamic> postDistribution, IEnumerable<dynamic> newUsers, IEnumerable<dynamic> userStatus)> GetAllReportsDataForExport(int year, int? month)
        {
            // Lấy dữ liệu vaccine
            var vaccineData = await GetTopVaccine();
            
            // Lấy dữ liệu phân phối bài đăng
            var postDistribution = await GetPostDistribution();
            
            // Lấy dữ liệu người dùng mới
            var newUsers = await GetDataNewUser(year, month ?? 0);
            
            // Lấy dữ liệu trạng thái người dùng
            var userStatus = await GetDataUserStatusForExcel(year, month ?? 0);
            
            return (vaccineData, postDistribution.ToList(), newUsers, userStatus);
        }
        
        public async Task<IEnumerable<dynamic>> GetDataUserStatusForExcel(int year, int? month)
        {
            // Nếu month = 0 => gán lại null
            if (month.HasValue && month.Value == 0)
            {
                month = null;
            }

            if (!month.HasValue)
            {
                // Lấy dữ liệu cả năm (12 tháng)
                var userStatistics = await _context.Users
                    .Where(u => u.LastLoggedIn.HasValue && u.LastLoggedIn.Value.Year == year)
                    .GroupBy(u => u.LastLoggedIn.Value.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        ActiveUsers = g.Count(u => u.Status == "ACTIVE"),
                        InactiveUsers = g.Count(u => u.Status == "INACTIVE")
                    })
                    .ToListAsync();

                var months = new[] { "Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec" };

                // Trả về MỘT dòng cho mỗi tháng
                return months.Select((m, index) => new 
                {
                    month = m,
                    ActiveUsers = userStatistics.FirstOrDefault(x => x.Month == index + 1)?.ActiveUsers ?? 0,
                    InactiveUsers = userStatistics.FirstOrDefault(x => x.Month == index + 1)?.InactiveUsers ?? 0
                });
            }
            else
            {
                // Lấy dữ liệu theo ngày
                int daysInMonth = DateTime.DaysInMonth(year, month.Value);
                var dailyStatistics = await _context.Users
                    .Where(u => u.LastLoggedIn.HasValue &&
                                u.LastLoggedIn.Value.Year == year &&
                                u.LastLoggedIn.Value.Month == month.Value)
                    .GroupBy(u => u.LastLoggedIn.Value.Day)
                    .Select(g => new
                    {
                        Day = g.Key,
                        ActiveUsers = g.Count(u => u.Status == "ACTIVE"),
                        InactiveUsers = g.Count(u => u.Status == "INACTIVE")
                    })
                    .ToListAsync();

                // Mỗi ngày 1 dòng
                return Enumerable.Range(1, daysInMonth).Select(d => new
                {
                    day = d.ToString("D2"),
                    ActiveUsers = dailyStatistics.FirstOrDefault(x => x.Day == d)?.ActiveUsers ?? 0,
                    InactiveUsers = dailyStatistics.FirstOrDefault(x => x.Day == d)?.InactiveUsers ?? 0
                });
            }
        }
        
    }

}
