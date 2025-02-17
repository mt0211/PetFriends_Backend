using DataAccess.Models;
using ProfileManagementAppAPI.DTOs.ResultModel;
using ProfileManagementAppAPI.DTOs;

using ProfileManagementAppAPI.Repositories;
using ProfileManagementAppAPI.Utilities;
using ProfileManagementAppAPI.DTOs.ClinicProfileModel;

namespace ProfileManagementAppAPI.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _profileManagementRepository;
        public AppointmentService(IAppointmentRepository profileManagementRepository)
        {
            _profileManagementRepository = profileManagementRepository;
        }

       
    }
}
