using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ProfileManagementAppAPI.Repositories
{
    public class AppointmentRepository : Repository<Category>, IAppointmentRepository
    {
        private readonly PetfriendsContext _context;

        public AppointmentRepository(PetfriendsContext context) : base(context)
        {
            _context = context;

        }

        

        public async Task<IEnumerable<Category>> GetCategory()
        {
            return await _context.Categories.ToListAsync();
        }

        
    }
}
