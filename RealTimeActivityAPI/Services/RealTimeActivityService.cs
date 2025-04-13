using DataAccess.Models;
using Microsoft.AspNetCore.SignalR;
using RealTimeActivityAPI.DTOs;
using RealTimeActivityAPI.Hubs;
using RealTimeActivityAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RealTimeActivityAPI.Services
{
    public class RealTimeActivityService : IRealTimeActivityService
    {
        private readonly IRealTimeActivityAPIRepository _repository;
        private readonly IHubContext<ActivityHub> _hubContext;
        private readonly IHubContext<AdminActivityHub> _adminHubContext;

        public RealTimeActivityService(IRealTimeActivityAPIRepository repository, IHubContext<ActivityHub> hubContext, IHubContext<AdminActivityHub> adminHubContext)
        {
            _repository = repository;
            _hubContext = hubContext;
            _adminHubContext = adminHubContext;
        }

        //Appointment description
        private string GetAppointmentActivityDescription(string type, Appointment appointment)
        {
            // Get single name instead of duplicating
            string customerName = appointment.User?.FullName ?? appointment.GuestUser?.FullName ?? "Unknown";
            string petName = appointment.Pet?.Name ?? appointment.GuestPet?.Name ?? "Unknown pet";
            string time = appointment.StartAt?.ToString("HH:mm dd/MM/yyyy") ?? "Not specified";

            return type switch
            {
                "APPOINTMENT_CREATED" => $"{customerName} has booked a new appointment for {petName} at {time}",
                "APPOINTMENT_CONFIRMED" => $"Appointment for {customerName}'s {petName} has been confirmed at {time}",
                "APPOINTMENT_COMPLETED" => $"Completed appointment for {customerName}'s {petName} at {time}",
                "APPOINTMENT_CANCELLED" => $"Cancelled appointment for {customerName}'s {petName} scheduled at {time}",
                "APP_APPOINTMENT_CREATED" => $"{customerName} has booked a new appointment for {petName} at {time}",
                "APP_APPOINTMENT_CANCELED" => $"{customerName} has cancelled an appointment for {petName} at {time}",
                _ => $"Updated appointment for {customerName}'s {petName}"
            };
        }
        //Feedback description
        private string GetFeedbackActivityDescription(string type, Feedback feedback)
        {
            string customerName = feedback.User?.FullName ?? "Unknown";
            string content = feedback.Content;
            string time = feedback.CreatedAt?.ToString("HH:mm dd/MM/yyyy") ?? "Not specified";
            return type switch
            {
                "FEEDBACK_RECEIVED" => $"\"{content}\" {time}",
                _ => $"Updated feedback for {customerName}"
            };
        }

        //CliniccService description
        private string GetClinicServiceActivityDescription(string type, ClinicService clinicService)
        {
            string clinicServiceName = clinicService.Name;
            string time = clinicService.CreateAt?.ToString("HH:mm dd/MM/yyyy") ?? "Not specified";
            return type switch
            {
                "CLINIC_SERVICE_CREATED" => $"{clinicServiceName} at {time}",
                "DISCOUNT_ENDED" => $"The discount for service \"{clinicServiceName}\" ended at {time}",
                _ => $"Updated clinic service: {clinicServiceName}"
            };
        }

        //Promotion description
        private string GetPromotionActivityDescription(string type, Promotion promotion)
        {
            string description = promotion.Description;
            string time = DateTime.Now.ToString("HH:mm dd/MM/yyyy") ?? "Not specified";
            return type switch
            {
                "PROMOTION_CREATED" => $"New promotion created: \"{description}\" at {time}",
                "PROMOTION_EXPIRED" => $"Promotion \"{description}\" expired at {time}",
                _ => $"Updated promotion: {description}"
            };
        }

        //User description
        private string GetUserActivityDescription(string type, User user)
        {
            string userName = user.FullName ?? "Unknown";
            string email = user.Email ?? "Unknown";
            string time = DateTime.Now.ToString("HH:mm dd/MM/yyyy") ?? "Not specified";
            return type switch
            {
                "USER_CREATED" => $"New user {userName} at {time}",
                "APP_USER_CREATED" => $"New user with email \"{email}\" register at {time}",
                _ => $"Updated user {userName}"
            };           
        }
        
        //Post description
        private string GetPostActivityDescription(string type, ForumPost post)
        {
            string userName = post.User.FullName ?? "Unknown";
            string content = post.PostContent;
            string time = DateTime.Now.ToString("HH:mm dd/MM/yyyy") ?? "Not specified";
            return type switch
            {
                "POST_CREATED" => $"{userName} shared a new forum post at {time}",
                _ => $"Updated post by {userName}"
            };
        }

        public async Task<ActivityDTO> CreateAppointmentActivity(string type, Guid appointmentId)
        {
            var appointment = await _repository.GetAppointmentById(appointmentId);
            if (appointment == null) return null;

            var activity = new Activity
            {
                Id = Guid.NewGuid(),
                Type = type,
                AppointmentId = appointmentId,
                UserId = appointment.UserId,
                PetId = appointment.PetId,
                Title = GetAppointmentActivityTitle(type, appointment),
                Description = GetAppointmentActivityDescription(type, appointment),
                CreatedAt = DateTime.UtcNow.AddHours(7),
                Metadata = JsonSerializer.Serialize(new
                {
                    appointmentId = appointment.Id.ToString(),
                    status = appointment.Status,
                    customerName = appointment.User?.FullName ?? appointment.GuestUser?.FullName,
                    petName = appointment.Pet?.Name ?? appointment.GuestPet?.Name
                })
            };


            var createdActivity = await _repository.CreateActivity(activity);
            var activityDto = MapToDTO(createdActivity);

            // Gửi activity mới tới tất cả clients
            await _hubContext.Clients.All.SendAsync("ReceiveActivity", activityDto);

            return activityDto;
        }
        public async Task<ActivityDTO> CreateFeedbackActivity(string type, Guid feedbackId)
        {
            var feedback = await _repository.GetFeedbackById(feedbackId);
            if (feedback == null) return null;
            var activity = new Activity
            {
                Id = Guid.NewGuid(),
                Type = type,
                UserId = feedback.UserId,
                Title = GetFeedbackActivityTitle(type, feedback),
                Description = GetFeedbackActivityDescription(type, feedback),
                CreatedAt = DateTime.UtcNow.AddHours(7),
                Metadata = JsonSerializer.Serialize(new
                {
                    feedbackId = feedback.Id.ToString(),
                    content = feedback.Content,
                    customerName = feedback.User?.FullName
                })
            };
            var createdActivity = await _repository.CreateActivity(activity);
            var feedbackDto = MapToDTO(createdActivity);
            await _hubContext.Clients.All.SendAsync("ReceiveActivity", feedbackDto);
            return feedbackDto;
        }

        public async Task<ActivityDTO> CreateClinicServiceActivity(string type, Guid clinicServiceId)
        {
            var clinicservice = await _repository.GetClinicServiceById(clinicServiceId);
            if (clinicservice == null) return null;
            var activity = new Activity
            {
                Id = Guid.NewGuid(),
                Type = type,
                Title = GetClinicServiceActivityTitle(type, clinicservice),
                Description = GetClinicServiceActivityDescription(type, clinicservice),
                ClinicServiceId = clinicservice.Id,
                CreatedAt = DateTime.UtcNow.AddHours(7),
                Metadata = JsonSerializer.Serialize(new
                {
                    clinicServiceId = clinicservice.Id.ToString(),
                    name = clinicservice.Name,
                })
            };
            var createdActivity = await _repository.CreateActivity(activity);
            var activityDto = MapToDTO(createdActivity);
            await _hubContext.Clients.All.SendAsync("ReceiveActivity", activityDto);
            await _adminHubContext.Clients.All.SendAsync("ReceiveActivity", activityDto);
            return activityDto;
        }

        public async Task<ActivityDTO> CreatePromotionActivity(string type, Guid promotionId)
        {
            var promotion = await _repository.GetPromotionById(promotionId);
            if (promotion == null) return null;
            var activity = new Activity
            {
                Id = Guid.NewGuid(),
                Type = type,
                Title = GetPromotionActivityTitle(type, promotion),
                Description = GetPromotionActivityDescription(type, promotion),
                CreatedAt = DateTime.UtcNow.AddHours(7),
                Metadata = JsonSerializer.Serialize(new
                {
                    promotionId = promotion.Id.ToString(),
                    name = promotion.Name,
                })
            };
            var createdActivity = await _repository.CreateActivity(activity);
            var activityDto = MapToDTO(createdActivity);
            await _hubContext.Clients.All.SendAsync("ReceiveActivity", activityDto);
            return activityDto;
        }

        public async Task<ActivityDTO> CreateUserActivity(string type, Guid userId)
        {
            var user = await _repository.GetUserById(userId);
            if (user == null) return null;
            var activity = new Activity
            {
                Id = Guid.NewGuid(),
                Type = type,
                Title = GetUserActivityTitle(type, user),
                Description = GetUserActivityDescription(type, user),
                CreatedAt = DateTime.UtcNow.AddHours(7),
                Metadata = JsonSerializer.Serialize(new
                {
                    userId = user.Id.ToString(),
                    name = user.FullName,
                })
            };
            var createdActivity = await _repository.CreateActivity(activity);
            var activityDto = MapToDTO(createdActivity);
            await _adminHubContext.Clients.All.SendAsync("ReceiveActivity", activityDto);
            return activityDto;
        }

        public async Task<ActivityDTO> CreatePostActivity(string type, Guid postId)
        {
            var post = await _repository.GetForumPostById(postId);
            if (post == null) return null;
            var activity = new Activity
            {
                Id = Guid.NewGuid(),
                Type = type,
                Title = GetPostActivityTitle(type, post),
                Description = GetPostActivityDescription(type, post),
                CreatedAt = DateTime.UtcNow.AddHours(7),
                Metadata = JsonSerializer.Serialize(new
                {
                    postuserId = post.Id.ToString(),
                    content = post.PostContent,
                })
            };
            var createdActivity = await _repository.CreateActivity(activity);
            var activityDto = MapToDTO(createdActivity);
            await _adminHubContext.Clients.All.SendAsync("ReceiveActivity", activityDto);
            return activityDto;
        }
        


        public async Task<List<ActivityDTO>> GetRecentActivities()
        {
            var activities = await _repository.GetRecentActivities();
            return activities.Select(MapToDTO).ToList();
        }

        private ActivityDTO MapToDTO(Activity activity)
        {
            var metadata = activity.Metadata != null
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(activity.Metadata)
                : new Dictionary<string, string>();

            return new ActivityDTO
            {
                Type = activity.Type,
                Title = activity.Title,
                Description = activity.Description,
                Icon = GetActivityIcon(activity.Type),
                CreatedAt = activity.CreatedAt,
                Metadata = metadata
            };
        }

        //Appointment title
        private string GetAppointmentActivityTitle(string type, Appointment appointment)
        {
            string customerName = appointment.User?.FullName ?? appointment.GuestUser?.FullName ?? "Unknown";

            return type switch
            {
                "APPOINTMENT_CREATED" => $"New appointment for {customerName}",
                "APPOINTMENT_CONFIRMED" => $"Appointment confirmed for {customerName}",
                "APPOINTMENT_COMPLETED" => $"Appointment completed for {customerName}",
                "APPOINTMENT_CANCELLED" => $"Appointment cancelled for {customerName}",
                "APP_APPOINTMENT_CREATED" => $"{customerName} has just booked an appointment",
                "APP_APPOINTMENT_CANCELED" => $"{customerName} has cancelled an appointment",
                _ => $"Appointment update for {customerName}"
            };
        }
        //Feedback title
        private string GetFeedbackActivityTitle(string type, Feedback feedback)
        {
            string customerName = feedback.User?.FullName ?? "Unknown";
            return type switch
            {
                "FEEDBACK_RECEIVED" => $"Feedback receive from {customerName}",
                _ => $"Feedback update from {customerName}"
            };

        }

        //ClinicService title
        private string GetClinicServiceActivityTitle(string type, ClinicService clinicService)
        {
            string clinicServiceName = clinicService.Name;
            return type switch
            {
                "CLINIC_SERVICE_CREATED" => $"New clinic service created named {clinicServiceName}",
                 "DISCOUNT_ENDED" => $"Discount ended for service {clinicServiceName}",
                _ => $"Clinic service update: {clinicServiceName}"
            };
        }

        //Promotion title
        private string GetPromotionActivityTitle(string type, Promotion promotion)
        {
            string promotionName = promotion.Name;
            return type switch
            {
                "PROMOTION_CREATED" => $"New promotion created {promotionName}",
                "PROMOTION_EXPIRED" => $"Promotion {promotionName} has expired",
                _ => $"Promotion update: {promotionName}"
            };
        }
        //User title
        private string GetUserActivityTitle(string type, User user)
        {
            string userName = user.FullName;
            string email = user.Email;
            return type switch
            {
                "USER_CREATED" => $"{userName} has just signed up",
                "APP_USER_CREATED" => $"User with email \"{email}\" has just signed up",
                _ => $"User update: {userName}"
            };
        }

        //Post title
        private string GetPostActivityTitle(string type, ForumPost post)
        {
            string userName = post.User.FullName;
            return type switch
            {
                "POST_CREATED" => $"{userName} has just created a new discussion post",
            };
        }
        //icon
        private string GetActivityIcon(string type)
        {
            return type switch
            {
                "APPOINTMENT_CREATED" => "📝",
                "APPOINTMENT_CONFIRMED" => "✅",
                "APPOINTMENT_COMPLETED" => "🎉",
                "APPOINTMENT_CANCELLED" => "❌",
                "FEEDBACK_RECEIVED" => "📣",
                "APP_APPOINTMENT_CREATED" => "📝",
                "APP_APPOINTMENT_CANCELLED" => "❌",
                "DISCOUNT_ENDED" => "⏳",
                "PROMOTION_CREATED" => "📣",
                "PROMOTION_EXPIRED" => "⏳",
                _ => "ℹ️"
            };
        }

    }
}
