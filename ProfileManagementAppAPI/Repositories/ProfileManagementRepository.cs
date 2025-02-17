using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ProfileManagementAppAPI.Repositories
{
    public class ProfileManagementRepository : Repository<User>, IProfileManagementRepository
    {
        private readonly PetfriendsContext _context;

        public ProfileManagementRepository(PetfriendsContext context) : base(context)
        {
            _context = context;

        }


    }
}
