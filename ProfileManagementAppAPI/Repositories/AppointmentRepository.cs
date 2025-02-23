using AppAppointmentManagementAPI.DTOs.ReviewModel;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ProfileManagementAppAPI.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly PetfriendsContext _context;

        public AppointmentRepository(PetfriendsContext context) 
        {
            _context = context;

        }

        public async Task AddReview(Feedback reviewEntity)
        {
            await _context.Feedbacks.AddAsync(reviewEntity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Category>> GetCategory()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<IEnumerable<Feedback>> GetReview()
        {
            return await _context.Feedbacks.ToListAsync();
        }

        public async Task<Feedback> GetReviewById(Guid reviewId)
        {
            return await _context.Feedbacks.FirstOrDefaultAsync(r => r.Id == reviewId);

        }

        public async Task UpdateReview(Feedback reviewUpdateModel)
        {
            _context.Feedbacks.Attach(reviewUpdateModel);
            _context.Entry(reviewUpdateModel).Property(c => c.Content).IsModified = true;
            _context.Entry(reviewUpdateModel).Property(c => c.Rating).IsModified = true;
            await _context.SaveChangesAsync();
        }

    }
}
