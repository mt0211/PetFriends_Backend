using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ServiceDemandAPI.Repositories.ServiceDemandRepository;

namespace ServiceDemandAPI.Repositories
{
    public interface IServiceDemandRepository 
    {
        Task<IEnumerable<BusyHourStat>> GetAppointmentDemandByHour(DayOfWeek dayOfWeek, DateTime? referenceDate = null);
    }
}