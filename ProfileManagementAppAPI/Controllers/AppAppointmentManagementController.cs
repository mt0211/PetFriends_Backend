using Microsoft.AspNetCore.Mvc;
using ProfileManagementAppAPI.Services;
using ProfileManagementAppAPI.DTOs;
using ProfileManagementAppAPI.DTOs.ClinicProfileModel;

using DataAccess.Models;
using ProfileManagementAppAPI.DTOs.ResultModel;
namespace ProfileManagementAppAPI.Controllers
{



    [ApiController]
    [Route("api/profilemanagement")]

    public class AppAppointmentManagementController : ControllerBase
    {
        private readonly IAppointmentService _profileManagementService;

        public AppAppointmentManagementController(IAppointmentService profileManagementService)
        {
            _profileManagementService = profileManagementService;
        }

        


    }
}
