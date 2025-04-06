using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealTimeActivityAPI.Repositories
{
    public class RealTimeActivityAPIRepository : IRealTimeActivityAPIRepository
    {
        private readonly PetfriendsContext _context;

        public RealTimeActivityAPIRepository(PetfriendsContext context)
        {
            _context = context;
        }
        
        
    }
}