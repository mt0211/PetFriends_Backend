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
            return await _context.Categories.Where(c => c.Status == 1).ToListAsync();
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
            return new {user, reviewcount, rating};
        }
        public async Task<dynamic> GetAppointmentByUserID(Guid userID)
        {
            return await _context.Appointments
            .Include(a=>a.User)
            .Include(a=>a.Pet)
            .Include(a=>a.AppointmentClinicServices)
            .ThenInclude(acs=>acs.ClinicService)
            .Where(a => a.Id == userID)
            .Select(appointment => new{
                    Id = appointment.Id,
           CreatedAt = appointment.CreatedAt,
           StartAt = appointment.StartAt,
           EndAt = appointment.EndAt,
           Status = appointment.Status,
           Note = appointment.Note,
           UserName = appointment.User.FullName,
           PetName = appointment.Pet.Name,
           ServiceNames = appointment.AppointmentClinicServices
               .Select(service => service.ClinicService.Name)
               .ToList()
            }).ToListAsync();
        }

    }
}
