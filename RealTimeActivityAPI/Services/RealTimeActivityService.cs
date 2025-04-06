using RealTimeActivityAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealTimeActivityAPI.Services
{
    public class RealTimeActivityService : IRealTimeActivityService
    {
        private readonly IRealTimeActivityAPIRepository _repository;

        public RealTimeActivityService(IRealTimeActivityAPIRepository repository)
        {
            _repository = repository;
        }
        
    }
}