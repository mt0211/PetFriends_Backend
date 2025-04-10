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

        public RealTimeActivityService(IRealTimeActivityAPIRepository repository, IHubContext<ActivityHub> hubContext)
        {
            _repository = repository;
            _hubContext = hubContext;
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
                "CLINIC_SERVICE_CREATED" => $"New clinic service created: {clinicServiceName} at {time}",
                _ => $"Updated clinic service: {clinicServiceName}"
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
                _ => $"Clinic service update: {clinicServiceName}"
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
                _ => "ℹ️"
            };
        }

    }
}
