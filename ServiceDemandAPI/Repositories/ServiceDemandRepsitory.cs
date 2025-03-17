using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;


namespace ServiceDemandAPI.Repositories
{
    public class ServiceDemandRepository : IServiceDemandRepository
    {
        private readonly PetfriendsContext _context;
        public ServiceDemandRepository(PetfriendsContext context)
        {
            _context = context;
        }
        
        public async Task<IEnumerable<BusyHourStat>> GetAppointmentDemandByHour(DayOfWeek dayOfWeek, DateTime? referenceDate = null)
        {
            var today = referenceDate?.Date ?? DateTime.UtcNow.Date;

            var startDate = new DateTime(1753, 1, 1);
            var endDate = today;

            int targetDayOfWeek = (int)dayOfWeek + 1;

            var sqlQuery = @"
                SELECT 
                    DATEPART(HOUR, CreatedAt) AS Hour, 
                    COUNT(*) AS AppointmentCount
                FROM dbo.Appointment
                WHERE CreatedAt >= @startDate AND CreatedAt <= @endDate
                AND DATEPART(weekday, CreatedAt) = @targetDayOfWeek
                GROUP BY DATEPART(HOUR, CreatedAt)";

            var appointments = await _context.Database
                .SqlQueryRaw<AppointmentHourStat>(sqlQuery,
                    new SqlParameter("@startDate", startDate),
                    new SqlParameter("@endDate", endDate),
                    new SqlParameter("@targetDayOfWeek", targetDayOfWeek))
                .ToListAsync();

            int actualWeekCount = (int)Math.Ceiling((endDate - startDate).TotalDays / 7);

            var result = Enumerable.Range(0, 24)
                .Select(hour => new BusyHourStat
                {
                    Hour = $"{(hour % 12 == 0 ? 12 : hour % 12)}{(hour < 12 ? "AM" : "PM")}",
                    Count = appointments.Any(a => a.Hour == hour)
                        ? (int)Math.Ceiling(appointments.Where(a => a.Hour == hour).Sum(a => a.AppointmentCount) / (double)actualWeekCount)
                        : 0,
                         Period = GetPeriodLabel(hour)
                })
                .OrderBy(x => DateTime.ParseExact(x.Hour, "htt", null))
                .ToList();

            return result;
        }

        public class BusyHourStat
        {
            public string Hour { get; set; }
            public int Count { get; set; }
            public string Period { get; set; }
        }
        public class AppointmentHourStat
        {
            public int Hour { get; set; }
            public int AppointmentCount { get; set; }
        }
        private string GetPeriodLabel(int hour)
        {
            return hour switch
            {
                >= 6 and < 12 => "Morning",
                >= 12 and < 17 => "Afternoon",
                >= 17 and < 21 => "Evening",
                _ => "Night"
            };
        }
    }
}