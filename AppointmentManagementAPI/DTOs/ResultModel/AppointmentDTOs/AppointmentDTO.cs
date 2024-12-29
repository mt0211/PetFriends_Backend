using AppointmentManagementAPI.DTOs.ResultModel.ServiceDTOs;
using System.Text.Json.Serialization;

namespace AppointmentManagementAPI.DTOs.ResultModel.AppointmentDTOs
{
    public class AppointmentListModel
    {
        public Guid Id { get; set; }
        public string? CustomerName { get; set; }
        public string? PetName { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? StartTime { get; set; }
        public List<string>? ServiceNames { get; set; } // Danh sách tên các dịch vụ
        public string? Status { get; set; }
    }
    public class AppointmentUpdateStatusModel
    {
        public Guid Id { get; set; }
        public string? Status { get; set; }
    }
    public class AppointmentAddModel
    {
        public Guid? UserId { get; set; }
        public Guid? PetId { get; set; }
        public List<Guid> ClinicServiceIds { get; set; } = new List<Guid>(); // Danh sách các dịch vụ
        public DateTime? StartAt { get; set; }
        public string? Status { get; set; }
        public string? Note { get; set; }
        public Guid? GuestUserId { get; set; }
        public Guid? GuestPetId { get; set; }

        // Thông tin cho Guest User
        public string? GuestFullName { get; set; }
        public string? GuestPhoneNumber { get; set; }
        public string? GuestEmail { get; set; }
        public string? Address { get; set; }

        // Thông tin cho Guest Pet
        public string? GuestPetName { get; set; }
        public DateTime? GuestPetDateOfBirth { get; set; }
        public string? GuestPetGender { get; set; }
        public string? GuestPetSpecies { get; set; }
    }
    public class AppointmentDetailModel
    {
        public Guid? UserId { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public Guid? PetId { get; set; }
        public string? PetName { get; set; }
        public DateTime? DataOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Species { get; set; }
        public List<ServiceModel>? services { get; set; }
       
        public string? Status { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public string? Note { get; set; }
    }

    public class AppointmentUpdateModel
    {
        public Guid Id { get; set; } // ID của cuộc hẹn cần cập nhật

        public List<Guid> ServiceIds { get; set; } = new List<Guid>(); // ID dịch vụ của cuộc hẹn (ServiceType)
        public string? Status { get; set; } // Trạng thái cuộc hẹn (Confirmed, Canceled, Completed)
        public DateTime? StartAt { get; set; } // Thời gian bắt đầu cuộc hẹn       
        public string? Note { get; set; } // Ghi chú về cuộc hẹn
    }

}
