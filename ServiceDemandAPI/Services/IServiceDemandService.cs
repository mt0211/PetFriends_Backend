using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceDemandAPI.DTOs.ResultModel;

namespace ServiceDemandAPI.Services
{
    public interface IServiceDemandService
    {
        Task<ResultModel> GetAppointmentDemand(string token, string dayOfWeek, DateTime? referenceDate = null);
    }
}