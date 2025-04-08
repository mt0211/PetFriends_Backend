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

        private string GetActivityDescription(string type, Appointment appointment)
        {
            string customerName = appointment.User?.FullName ?? appointment.GuestUser?.FullName ?? "Unknown";
            string petName = appointment.Pet?.Name ?? appointment.GuestPet?.Name ?? "Unknown pet";
            string time = appointment.StartAt?.ToString("HH:mm dd/MM/yyyy") ?? "Not specified";

            return type switch
            {
                "APPOINTMENT_CREATED" => $"{customerName} has booked a new appointment for {petName} at {time}",
                "APPOINTMENT_CONFIRMED" => $"Appointment for {customerName}'s {petName} has been confirmed for {time}",
                "APPOINTMENT_COMPLETED" => $"Completed appointment for {customerName}'s {petName}",
                "APPOINTMENT_CANCELLED" => $"Cancelled appointment for {customerName}'s {petName} scheduled at {time}",
                _ => $"Updated appointment for {customerName}'s {petName}"
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
                Title = GetActivityTitle(type, appointment),
                Description = GetActivityDescription(type, appointment),
                CreatedAt = DateTime.UtcNow,
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

        public async Task<List<ActivityDTO>> GetRecentActivities(int count = 10)
        {
            var activities = await _repository.GetRecentActivities(count);
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

        private string GetActivityTitle(string type, Appointment appointment)
        {
            string customerName = appointment.User?.FullName ?? appointment.GuestUser?.FullName ?? "Unknown";
            return type switch
            {
                "APPOINTMENT_CREATED" => $"New appointment for",
                "APPOINTMENT_CONFIRMED" => $"Appointment confirmed for {customerName}",
                "APPOINTMENT_COMPLETED" => $"Appointment completed for {customerName}",
                "APPOINTMENT_CANCELLED" => $"Appointment cancelled for {customerName}",
                _ => $"Appointment update for {customerName}"
            };
        }

        private string GetActivityIcon(string type)
        {
            return type switch
            {
                "APPOINTMENT_CREATED" => "📝",
                "APPOINTMENT_CONFIRMED" => "✅",
                "APPOINTMENT_COMPLETED" => "🎉",
                "APPOINTMENT_CANCELLED" => "❌",
                _ => "ℹ️"
            };
        }
    }
}
