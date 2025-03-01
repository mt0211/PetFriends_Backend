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
            return await _context.Categories
            .Include(c => c.ClinicServices)
            .Where(c => c.Status == 1 && c.ClinicServices.Any(cs => cs.IsBlocked == 1))
            .ToListAsync();
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
        public async Task<dynamic> GetClinicInformation()
        {
            var user = await _context.Users.FirstOrDefaultAsync(email => email.Email == "petfriends.contacts@gmail.com");
            var reviewcount = await _context.Feedbacks.CountAsync();
            var rating = await _context.Feedbacks.AverageAsync(r => r.Rating);
            return new { user, reviewcount, rating };
        }

        ///Note: Phòng ngừa khi get category call api không được.
        // public async Task<dynamic> GetServiceByCategoryID(Guid categoryID)
        // {
        //     return await _context.ClinicServices
        //     .Where(s=>s.Category == categoryID)
        //     .ToListAsync();
        // }

        public async Task<IEnumerable<Pet>> GetPetListByUserId(Guid userId)
        {
            return await _context.Pets.Where(p => p.UserId == userId).ToListAsync();
        }

    }
}
