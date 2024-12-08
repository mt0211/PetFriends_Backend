using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AppointmentManagementAPI.Repository
{
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        private readonly PetFriendsContext _context;
        public AppointmentRepository(PetFriendsContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<dynamic>> GetAllApointment()
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Pet)
                .Include(a => a.ClinicService)
                .Select(a => new
                {
                    Id = a.Id,
                    CustomerName = a.User.FullName,
                    PetName = a.Pet.Name,
                    Date = a.Pet.DateOfBirth,
                    StartTime = a.StartAt,
                    ServiceType = a.ClinicService.Name,
                    Status = a.Status,
                }).ToListAsync();
        }
       
       
    }
}
