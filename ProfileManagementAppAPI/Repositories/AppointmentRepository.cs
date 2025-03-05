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
            .Where(c => c.Status == 1)
            .Select(c => new Category
            {
                Id = c.Id,
                Name = c.Name,
                Status = c.Status,
                ClinicServices = c.ClinicServices
                    .Where(cs => cs.IsBlocked == 1 && cs.Status == "ACTIVE")
                    .ToList()
            })
            .ToListAsync();
        }

        public async Task<IEnumerable<dynamic>> GetReview()
        {
            return await _context.Feedbacks
            .Include(r => r.User)
            .Select(r => new
            {
                r.Id,
                r.Content,
                r.Rating,
                r.CreatedAt,
                UserName = r.User.FullName,
                UserAvatar = r.User.AvatarUrl,
                UserEmail = r.User.Email,
            }).ToListAsync();
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


        public async Task<UserCart> CheckUserCart(Guid userId)
        {
            return await _context.UserCarts.FirstOrDefaultAsync(uc => uc.UserId == userId && uc.Status == 0);
        }
        public async Task AddNewCart(UserCart userCart)
        {
            await _context.UserCarts.AddAsync(userCart);
            await _context.SaveChangesAsync();
        }
        public async Task AddNewCartItem(UserCartItem userCartItem)
        {
            await _context.UserCartItems.AddAsync(userCartItem);
            await _context.SaveChangesAsync();
        }
        public async Task<UserCartItem> CheckUserCartItemByServiceId(Guid serviceId)
        {
            return await _context.UserCartItems.FirstOrDefaultAsync(uc => uc.ClinicServiceId == serviceId);
        }
        public async Task<UserCart> GetCartByUserId(Guid userId)
        {
            var cart = await _context.UserCarts
            .Include(uc => uc.User)
            .Include(uc => uc.UserCartItems)
            .ThenInclude(uc => uc.ClinicService)
            .Include(uc => uc.UserCartItems)
            .ThenInclude(uc => uc.Pet)
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.Status == 0);
            return cart;
        }
        public async Task UpdateCart(UserCart userCart)
        {
            _context.UserCarts.Attach(userCart);
            _context.Entry(userCart).Property(c => c.Datebook).IsModified = true;
            _context.Entry(userCart).Property(c => c.Notes).IsModified = true;
            _context.Entry(userCart).Property(c => c.Status).IsModified = true;
            await _context.SaveChangesAsync();
        }
        public async Task AddAppointment(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task AddAppointmentClinicService(AppointmentClinicService appointmentService)
        {
            await _context.AppointmentClinicServices.AddAsync(appointmentService);
            await _context.SaveChangesAsync();
        }
        public async Task<List<UserCartItem>> GetCartItemsByCartId(Guid cartId)
        {
            return await _context.UserCartItems
                .Where(item => item.CartId == cartId)
                .ToListAsync();
        }
        public async Task<UserCartItem> GetCartItemByServiceId(Guid serviceId)
        {
            return await _context.UserCartItems
                .FirstOrDefaultAsync(item => item.ClinicServiceId == serviceId);
        }

        public async Task RemoveCartItem(UserCartItem cartItem)
        {
            _context.UserCartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
        }
        public async Task RemoveCart(UserCart cart)
        {
            _context.UserCarts.Remove(cart);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Promotion>> GetPromotionTypeAllMember()
        {
            return await _context.Promotions.Where(p => p.TargetGroup == "All Customers" && p.Status == "Active").ToListAsync();
        }

        public async Task<List<Promotion>> GetPromotionTypeNewMember()
        {
            return await _context.Promotions.Where(p => p.TargetGroup == "First-Time Visitors" && p.Status == "Active").ToListAsync();
        }
        public async Task<List<Promotion>> GetPromotionTypeLoyaltyMember()
        {
            return await _context.Promotions.Where(p => p.TargetGroup == "Loyalty Members" && p.Status == "Active").ToListAsync();
        }
        public async Task<User> GetUserByUserId(Guid userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }


        //APPLY PROMOTION
        public async Task<Promotion> GetPromotionById(Guid promotionId)
        {
            return await _context.Promotions.FirstOrDefaultAsync(p => p.Id == promotionId);
        }

        public async Task AddAppointmentPromotion(AppointmentPromotion appointmentPromotion)
        {
            await _context.AppointmentPromotions.AddAsync(appointmentPromotion);
            await _context.SaveChangesAsync();
        }

        //BOOKING HISTORY
        public async Task<List<Appointment>> GetBookingHistory(Guid userId)
        {
            return await _context.Appointments.Where(a => a.UserId == userId).ToListAsync();
        }
    }
}
