using AppointmentManagementAPI.DTOs.ResultModel;
using AppointmentManagementAPI.DTOs.ResultModel.AppointmentDTOs;
using AppointmentManagementAPI.DTOs.ResultModel.PetDTOs;
using AppointmentManagementAPI.DTOs.ResultModel.ServiceDTOs;
using AppointmentManagementAPI.DTOs.ResultModel.UserDTOs;
using AppointmentManagementAPI.Repository;
using AppointmentManagementAPI.Utilities;
using DataAccess.Models;
using MySqlX.XDevAPI.Common;
using System.Runtime.CompilerServices;

namespace AppointmentManagementAPI.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentrepository;
        private readonly IMessageBus _messageBus;
        public AppointmentService(IAppointmentRepository appointmentrepository, IMessageBus messageBus)
        {
            _appointmentrepository = appointmentrepository;
            _messageBus = messageBus;
        }
        public async Task<ResultModel> GetAllAppointment(string token)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }
            try
            {
                // Fetch all appointments without pagination
                var appointments = await _appointmentrepository.GetAllApointment();

                if (!appointments.Any())
                {
                    result.IsSuccess = false;
                    result.Code = 200;
                    result.Message = "No appointments found";
                    return result;
                }

                // Transform entities to DTO
                var appointmentList = appointments.Select(a => new AppointmentListModel
                {
                    Id = a.Id,
                    CustomerName = a.UserName,
                    PetName = a.PetName,
                    Date = a.CreatedAt,
                    StartTime = a.StartAt,
                    ServiceNames = a.ServiceNames,
                    Status = a.Status
                }).ToList();
                
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = appointmentList;
                result.Message = "Successfully retrieved appointments";
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }

            return result;
        }
        public async Task<ResultModel> UpdateAppointmentStatus(string token, AppointmentUpdateStatusModel appointmentstatusmodel)
        {
            ResultModel Result = new();
            var userId = Encoder.DecodeToken(token, "userid");

            if (!Guid.TryParse(userId, out Guid id))
            {
                Result.IsSuccess = false;
                Result.Code = 400; // Bad request
                Result.Message = "Invalid user ID";
                return Result;
            }

            try
            {
                var appointment = await _appointmentrepository.Get(appointmentstatusmodel.Id);
                var appointments = await _appointmentrepository.GetAppointmentAndUserEmail(appointmentstatusmodel.Id);
                if (appointment == null)
                {
                    Result.IsSuccess = false;
                    Result.Code = 404; // Not found
                    Result.Message = "Appointment not found";
                    return Result;
                }
                if (appointment.Status == "Completed")
                {
                    Result.IsSuccess = false;
                    Result.Code = 400; // Bad request
                    Result.Message = "Cannot update completed appointment";
                    return Result;
                }
                if (appointmentstatusmodel.Status == "Completed")
                {
                    // Cập nhật trạng thái và thời gian hoàn thành
                    appointment.Status = appointmentstatusmodel.Status;
                    appointment.EndAt = DateTime.UtcNow.AddHours(7);

                    // Sử dụng FinalAmount đã được tính sẵn
                    var totalAmount = appointment.FinalAmount ?? 0;

                    // Cập nhật bảng UserBookingSummary
                    if (appointment.UserId.HasValue)
                    {
                        await UpdateUserBookingSummary(appointment.UserId.Value, totalAmount);
                    }
                    await _appointmentrepository.UpdateDailyRevenue(totalAmount);
                    var services = await _appointmentrepository.GetAppointmentServices(appointment.Id);
                    foreach (var service in services)
                    {
                        await _appointmentrepository.UpdateServiceRevenue(service.ClinicServiceId, service.Price ?? 0);
                    }

                    //Update into pet vaccine profile if user choose vaccination service
                    await UpdatePetVaccineInfo(appointment, services);

                    // Chuẩn bị dữ liệu trả về
                    Result.Data = new AppointmentUpdateResultModel
                    {
                        Status = appointment.Status,
                        Services = services,
                        TotalPrice = totalAmount,
                        EndAt = appointment.EndAt
                    };
                    _messageBus.PublishAppointmentActivity(
                        "APPOINTMENT_COMPLETED", 
                        appointment.Id);
                    _messageBus.PublishAppointmentReviewReminderNotification(
                        "APPOINTMENT_REVIEW_REMINDER",
                        appointment.Id);
                }
                else if (appointmentstatusmodel.Status == "Confirmed")
                {
                    // UPDATE USAGE LIMIT PROMOTION
                    var appointmentPromotions = await _appointmentrepository.GetAppointmentPromotions(appointment.Id);
                    foreach (var appointmentPromotion in appointmentPromotions)
                    {
                        if (appointmentPromotion.Promotion != null && appointmentPromotion.Promotion.UsageLimit > 0)
                        {
                            appointmentPromotion.Promotion.UsageLimit -= 1;
                            await _appointmentrepository.UpdatePromotion(appointmentPromotion.Promotion);
                        }
                    }

                    //SEND EMAIL
                    if (!string.IsNullOrWhiteSpace(appointments.Email))
                    {
                        string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TemplateEmail", "ConfirmAppointment.html");
                        string Html = File.ReadAllText(FilePath);
                        Html = Html.Replace("{{CustomerName}}", appointments.FullName);
                        Html = Html.Replace("{{StartAt}}", appointments.CreatedAt?.ToString("dd/MM") ?? "N/A");
                        Html = Html.Replace("{{StartAt}}", appointments.StartAt?.ToString("HH:mm") ?? "N/A");
                        Html = Html.Replace("{{EndAt}}", appointments.EndAt?.ToString("HH:mm") ?? "N/A");
                        bool EmailSent = await Email.SendEmail(appointments.Email, "Confirm appointment", Html);
                    }
                    else
                    {
                        Console.WriteLine("Email does not exist. Skipping email notification.");
                    }
                      _messageBus.PublishAppointmentActivity(
                        "APPOINTMENT_CONFIRMED", 
                        appointment.Id);

                        _messageBus.PublishAppointmentConfirmedNotification(
                        "APPOINTMENT_CONFIRMATION",
                        appointment.Id);
                }
                else if (appointmentstatusmodel.Status == "Canceled")
                {
                    if (!string.IsNullOrWhiteSpace(appointments.Email))
                    {
                        string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TemplateEmail", "CancelAppointment.html");
                        string Html = File.ReadAllText(FilePath);
                        Html = Html.Replace("{{CustomerName}}", appointments.FullName);
                        Html = Html.Replace("{{StartAt}}", appointments.CreatedAt?.ToString("dd/MM") ?? "N/A");
                        Html = Html.Replace("{{StartAt}}", appointments.StartAt?.ToString("HH:mm") ?? "N/A");
                        Html = Html.Replace("{{EndAt}}", appointments.EndAt?.ToString("HH:mm") ?? "N/A");
                        bool EmailSent = await Email.SendEmail(appointments.Email, "Cancel appointment", Html);
                    }
                    else
                    {
                        Console.WriteLine("Email does not exist. Skipping email notification.");
                    }
                     _messageBus.PublishAppointmentActivity(
                        "APPOINTMENT_CANCELLED", 
                        appointment.Id);
                }

                // Cập nhật trạng thái cuộc hẹn trong DB
                appointment.Status = appointmentstatusmodel.Status;
                await _appointmentrepository.Update(appointment);

                Result.IsSuccess = true;
                Result.Code = 200;
                Result.Message = "Appointment updated successfully";
            }
            catch (Exception e)
            {
                Result.IsSuccess = false;
                Result.Code = 500; // Internal Server Error
                Result.ResponseFailed = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }

            return Result;
        }
        //Helper method total amount
        private async Task UpdateUserBookingSummary(Guid userId, decimal totalAmount)
        {
            // Lấy thông tin hiện tại từ bảng UserBookingSummary
            var userBookingSummary = await _appointmentrepository.GetUserBookingSummary(userId);

            if (userBookingSummary == null)
            {
                // Nếu chưa có dữ liệu, tạo mới
                userBookingSummary = new UserBookingSummary
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    NumOfBook = 1,
                    Amount = totalAmount
                };
                await _appointmentrepository.AddUserBookingSummary(userBookingSummary);
            }
            else
            {
                // Nếu đã có dữ liệu, cập nhật
                userBookingSummary.NumOfBook += 1;
                userBookingSummary.Amount += totalAmount;
                await _appointmentrepository.UpdateUserBookingSummary(userBookingSummary);
            }
        }
        //Helper method send email


        public async Task<ResultModel> AddAppointment(string token, AppointmentAddModel appointment)
        {
            ResultModel result = new();
            var userIdFromToken = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userIdFromToken, out Guid userIdGuid))
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "Invalid user ID from token";
                return result;
            }

            try
            {
                // ================================
                // Bước 0: Tìm user thật qua phone/email
                // ================================
                if (!appointment.UserId.HasValue || appointment.UserId.Value == Guid.Empty)
                {
                    var userId = await _appointmentrepository.GetUserIdByPhoneOrEmail(
                        appointment.GuestPhoneNumber,
                        appointment.GuestEmail
                    );
                    if (userId.HasValue)
                    {
                        // Tìm thấy user => xài userId thật
                        appointment.UserId = userId;
                    }
                }
                if ((appointment.UserId.HasValue && appointment.UserId.Value != Guid.Empty)
                    && (!appointment.PetId.HasValue || appointment.PetId.Value == Guid.Empty))
                {
                    if (!string.IsNullOrEmpty(appointment.GuestPetName))
                    {
                        // Tìm pet thật theo tên + userId
                        var existingPet = await _appointmentrepository.GetPetByNameAndUserId(
                            appointment.GuestPetName,
                            appointment.UserId.Value
                        );
                        if (existingPet != null)
                        {
                            // Tìm thấy pet => gán PetId
                            appointment.PetId = existingPet.Id;
                        }
                        else
                        {
                            // Không tìm thấy pet trong hệ thống cho user này
                            result.IsSuccess = false;
                            result.Code = 400;
                            result.Message = "For system users, only pets already registered in the system can be used. Please register the pet first or use an existing pet.";
                            return result;
                        }
                    }
                    else
                    {
                        // Không có tên pet được cung cấp cho user trong hệ thống
                        result.IsSuccess = false;
                        result.Code = 400;
                        result.Message = "Pet name is required for system users.";
                        return result;
                    }
                }
                // ================================
                // 1) CHECK USER CHÍNH CHỦ
                // ================================
                Guid? guestUserId = null;
                if (appointment.UserId.HasValue && appointment.UserId.Value != Guid.Empty)
                {
                    // Đã có userId => skip GuestUser
                }
                else
                {
                    // Tạo/Lấy GuestUser
                    if (!appointment.GuestUserId.HasValue || appointment.GuestUserId.Value == Guid.Empty)
                    {
                        var guestUser = await _appointmentrepository.CreateOrGetGuestUser(appointment);
                        if (guestUser != null)
                        {
                            guestUserId = guestUser.Id;
                        }
                    }
                    else
                    {
                        guestUserId = appointment.GuestUserId;
                    }
                }

                // ================================
                // 2) CHECK PET CHÍNH CHỦ
                // ================================
                Guid? guestPetId = null;
                if (appointment.PetId.HasValue && appointment.PetId.Value != Guid.Empty)
                {
                    // Đã có petId => skip GuestPet
                }
                else
                {
                    // Tạo/Lấy GuestPet - Chỉ thực hiện nếu là guest user (không có UserId)
                    if (!appointment.UserId.HasValue || appointment.UserId.Value == Guid.Empty)
                    {
                        if (!appointment.GuestPetId.HasValue || appointment.GuestPetId.Value == Guid.Empty)
                        {
                            if (guestUserId.HasValue)
                            {
                                var guestPet = await _appointmentrepository.CreateOrGetGuestPet(appointment, guestUserId.Value);
                                if (guestPet != null)
                                {
                                    guestPetId = guestPet.Id;
                                }
                            }
                        }
                        else
                        {
                            guestPetId = appointment.GuestPetId;
                        }
                    }
                }
                 decimal totalAmount = 0;
                decimal discountAmount = 0;
                foreach (var serviceId in appointment.ClinicServiceIds)
                {
                    var service = await _appointmentrepository.GetClinicServiceById(serviceId);
                    if (service != null)
                    {
                        totalAmount += service.DiscountedPrice ?? 0;
                    }
                }
                // ================================
                // 3) TẠO APPOINTMENT
                // ================================
                var newAppointment = new Appointment
                {
                    Id = Guid.NewGuid(),
                    UserId = appointment.UserId,
                    PetId = appointment.PetId,
                    GuestUserId = guestUserId,
                    GuestPetId = guestPetId,
                    CreatedAt = DateTime.Now.AddHours(7),
                    StartAt = appointment.StartAt,
                    Status = appointment.Status,
                    Note = appointment.Note,
                    TotalAmount = totalAmount,
                    DiscountAmount = discountAmount,
                    FinalAmount = totalAmount - discountAmount
                };
                await _appointmentrepository.Insert(newAppointment);

                // ================================
                // 4) Thêm AppointmentClinicService
                // ================================
                foreach (var serviceId in appointment.ClinicServiceIds)
                {
                    var appointmentService = new AppointmentClinicService
                    {
                        Id = Guid.NewGuid(),
                        AppointmentId = newAppointment.Id,
                        ClinicServiceId = serviceId,
                        DateGiven = DateTime.Now.AddHours(7),
                        Notes = "Add successfully"
                    };
                    await _appointmentrepository.InsertAppointmentClinicService(appointmentService);
                }

                result.IsSuccess = true;
                result.Code = 200;
                result.Message = "Successfully added new appointment";
                 _messageBus.PublishAppointmentActivity(
                "APPOINTMENT_CREATED", 
                newAppointment.Id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }

            return result;
        }



        public async Task<ResultModel> GetListClinicservice(string token)
        {
            ResultModel result = new();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }
            try
            {
                var services = await _appointmentrepository.GetListClinicservices();
                if (services == null || !services.Any())
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "Not found vaccines";
                    return result;
                }
                var ServiceList = services.Select(s => new ServiceModel
                {
                    Id = s.Id,
                    Name = s.Name,
                }).ToList();

                //Success response
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = ServiceList;
                result.Message = "Successfully get all vaccine";
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }
            return result;
        }
        public async Task<ResultModel> CheckUserByPhoneNumber(string token, string phonenumber)
        {
            ResultModel result = new();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }
            try
            {
                var user = await _appointmentrepository.GetUserByPhoneNumber(phonenumber);
                if (user == null)
                {
                    var guestuser = await _appointmentrepository.GetGuestUserByPhoneNumber(phonenumber);
                    if (guestuser == null)
                    {
                        result.IsSuccess = false;
                        result.Code = 404;
                        result.Message = "Not found";
                        return result;
                    }
                }

                var userDto = new UserResponseModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Address = user.Address
                };
                result.IsSuccess = true;
                result.Code = 200;
                result.Message = "User get data successfully";
                result.Data = userDto;

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }
            return result;
        }
        public async Task<ResultModel> CheckPetByNameAndUserID(string token, string petName, Guid UserId)
        {
            ResultModel result = new();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }
            try
            {
                var pet = await _appointmentrepository.GetPetByNameAndUserId(petName, UserId);
                if (pet == null)
                {
                    var guestpet = await _appointmentrepository.GetGuestPetByNameAndGuestUserId(petName, UserId);
                    if (guestpet == null)
                    {
                        result.IsSuccess = false;
                        result.Code = 404;
                        result.Message = "Not found";
                        return result;
                    }
                }

                var petDto = new PetResponseModel
                {
                    Id = pet.Id,
                    Name = pet.Name,
                    DateOfBirth = pet.DateOfBirth,
                    Gender = pet.Gender,
                    Species = pet.Species,
                };
                result.IsSuccess = true;
                result.Code = 200;
                result.Message = "Pet get data successfully";
                result.Data = petDto;

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> DeleteAppointment(string token, Guid appointmentID)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }

            try
            {
                // Lấy thông tin appointment
                var appointment = await _appointmentrepository.GetAppointmentByID(appointmentID);
                if (appointment == null)
                {
                    result.IsSuccess = false;
                    result.Code = 404; // Not found
                    result.Message = "Appointment not found.";
                    return result;
                }

                // Lấy danh sách các dịch vụ liên quan đến appointment
                var relatedServices = appointment.AppointmentClinicServices;
                if (relatedServices != null && relatedServices.Any())
                {
                    // Xóa tất cả các dịch vụ liên quan
                    var servicesToDelete = relatedServices.ToList();

                    foreach (var service in servicesToDelete)
                    {
                        await _appointmentrepository.RemoveAppointmentClinicService(service);
                    }
                }

                // Xóa appointment
                await _appointmentrepository.DeleteAppointment(appointment);

                result.IsSuccess = true;
                result.Code = 200;
                result.Message = "Appointment and related services deleted successfully.";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Code = 500; // Internal server error
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }

            return result;
        }

        public async Task<ResultModel> GetAppointmentDetail(string token, Guid guid)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");

            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }

            try
            {
                // Lấy dữ liệu thô từ repository
                var appointmentEntity = await _appointmentrepository.GetAppointmentByID(guid);

                if (appointmentEntity != null)
                {
                    // Mapping dữ liệu sang DTO
                    var appointmentDetail = new AppointmentDetailModel
                    {
                        // Lấy UserId từ GuestUser nếu UserId là null
                        UserId = appointmentEntity.UserId != null ? appointmentEntity.User.Id : appointmentEntity.GuestUser?.Id,
                        FullName = appointmentEntity.UserId != null ? appointmentEntity.User.FullName : appointmentEntity.GuestUser?.FullName,
                        PhoneNumber = appointmentEntity.UserId != null ? appointmentEntity.User.PhoneNumber : appointmentEntity.GuestUser?.PhoneNumber,
                        Email = appointmentEntity.UserId != null ? appointmentEntity.User.Email : appointmentEntity.GuestUser?.Email,
                        Address = appointmentEntity.UserId != null ? appointmentEntity.User.Address : appointmentEntity.GuestUser?.Address,

                        // Lấy PetId từ GuestPet nếu PetId là null
                        PetId = appointmentEntity.PetId != null ? appointmentEntity.Pet.Id : appointmentEntity.GuestPet?.Id,
                        PetName = appointmentEntity.PetId != null ? appointmentEntity.Pet.Name : appointmentEntity.GuestPet?.Name,
                        DataOfBirth = appointmentEntity.PetId != null ? appointmentEntity.Pet.DateOfBirth : appointmentEntity.GuestPet?.DateOfBirth,
                        Gender = appointmentEntity.PetId != null ? appointmentEntity.Pet.Gender : appointmentEntity.GuestPet?.Gender,
                        Species = appointmentEntity.PetId != null ? appointmentEntity.Pet.Species : appointmentEntity.GuestPet?.Species,

                        // Thông tin dịch vụ
                        services = appointmentEntity.AppointmentClinicServices
                    .Select(acs => new ServiceModel
                    {
                        Id = acs.ClinicService.Id,
                        Name = acs.ClinicService.Name
                    })
                    .ToList(),

                        // Thông tin lịch hẹn
                        Status = appointmentEntity.Status,
                        CreateAt = appointmentEntity.CreatedAt,
                        StartAt = appointmentEntity.StartAt,
                        EndAt = appointmentEntity.EndAt,
                        Note = appointmentEntity.Note
                    };

                    result.IsSuccess = true;
                    result.Code = 200;
                    result.Data = appointmentDetail;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Code = 404; // Not found
                    result.Message = "Appointment not found";
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }
        public async Task<ResultModel> UpdateAppointment(string token, AppointmentUpdateModel appointmentUpdate)
        {
            ResultModel result = new();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }

            try
            {
                var appointment = await _appointmentrepository.GetAppointmentByID(appointmentUpdate.Id);
                var appointments = await _appointmentrepository.GetAppointmentAndUserEmail(appointmentUpdate.Id);
                if (appointment == null)
                {
                    result.IsSuccess = false;
                    result.Code = 404; // Not found
                    result.Message = "Appointment not found.";
                    return result;
                }
                if (appointment.Status == "Completed")
                {
                    result.IsSuccess = false;
                    result.Code = 400; // Bad request
                    result.Message = "Cannot update completed appointment.";
                    return result;
                }

                // Xử lý cho người dùng hệ thống
                if (appointment.UserId != null)
                {
                    // Kiểm tra xem người dùng có đang cố gắng thay đổi thông tin cá nhân không
                    var user = await _appointmentrepository.GetUserByID(appointment.UserId);
                    var pet = await _appointmentrepository.GetPetByID(appointment.PetId);

                    // Kiểm tra nếu thông tin cá nhân bị thay đổi
                    bool personalInfoChanged = false;
                    string changedFields = "";

                    if (user != null && (
                        (appointmentUpdate.FullName != null && user.FullName != appointmentUpdate.FullName) ||
                        (appointmentUpdate.PhoneNumber != null && user.PhoneNumber != appointmentUpdate.PhoneNumber) ||
                        (appointmentUpdate.Email != null && user.Email != appointmentUpdate.Email) ||
                        (appointmentUpdate.Address != null && user.Address != appointmentUpdate.Address)))
                    {
                        personalInfoChanged = true;
                        changedFields += "User information (name, phone, email, address)";
                    }

                    if (pet != null && (
                        (appointmentUpdate.PetName != null && pet.Name != appointmentUpdate.PetName) ||
                        (appointmentUpdate.DataOfBirth != null && pet.DateOfBirth != appointmentUpdate.DataOfBirth) ||
                        (appointmentUpdate.Gender != null && pet.Gender != appointmentUpdate.Gender) ||
                        (appointmentUpdate.Species != null && pet.Species != appointmentUpdate.Species)))
                    {
                        personalInfoChanged = true;
                        changedFields += (changedFields.Length > 0 ? ", " : "") + "Pet information (name, date of birth, gender, species)";
                    }

                    if (personalInfoChanged)
                    {
                        result.IsSuccess = false;
                        result.Code = 400; // Bad request
                        result.Message = $"Cannot update personal information for system users. Changed fields: {changedFields}. Only note, start time, status and services can be updated.";
                        return result;
                    }


                    // 4. Cập nhật dịch vụ
                    var existingServices = appointment.AppointmentClinicServices?.ToList() ?? new List<AppointmentClinicService>();
                    var updatedServiceIds = appointmentUpdate.ServiceIds ?? new List<Guid>();

                    // Xác định các dịch vụ cần xóa
                    var serviceIdsToRemove = existingServices
                        .Where(service => !updatedServiceIds.Contains(service.ClinicServiceId))
                        .Select(service => service.Id)
                        .ToList();

                    // Xóa các dịch vụ không còn trong danh sách cập nhật
                    foreach (var serviceId in serviceIdsToRemove)
                    {
                        await _appointmentrepository.RemoveAppointmentClinicServiceById(serviceId);
                    }

                    // Xác định các dịch vụ mới cần thêm
                    var newServiceIds = updatedServiceIds
                        .Where(serviceId => !existingServices.Any(existing => existing.ClinicServiceId == serviceId))
                        .ToList();

                    // Thêm các dịch vụ mới
                    foreach (var serviceId in newServiceIds)
                    {
                        var newService = new AppointmentClinicService
                        {
                            Id = Guid.NewGuid(),
                            AppointmentId = appointment.Id,
                            ClinicServiceId = serviceId,
                            DateGiven = DateTimeOffset.Now.DateTime,
                            Notes = "Add successfully"
                        };
                        await _appointmentrepository.InsertAppointmentClinicService(newService);
                    }
                    // 3. Cập nhật thông tin cuộc hẹn cơ bản
                    DateTime? endAt = null;
                    if (appointmentUpdate.Status == "Completed")
                    {
                        endAt = DateTime.Now.AddHours(7);
                    }

                    await _appointmentrepository.UpdateAppointmentBasicInfo(
                        appointment.Id,
                        appointmentUpdate.Status,
                        appointmentUpdate.StartAt,
                        appointmentUpdate.Note,
                        endAt
                    );



                    // 5. Xử lý các trạng thái đặc biệt
                    if (appointmentUpdate.Status == "Completed")
                    {
                        // Sử dụng FinalAmount đã được tính sẵn
                        var totalAmount = appointment.FinalAmount ?? 0;
                        // Cập nhật bảng UserBookingSummary
                        if (appointment.UserId.HasValue)
                        {
                            await UpdateUserBookingSummary(appointment.UserId.Value, totalAmount);
                        }
                        await _appointmentrepository.UpdateDailyRevenue(totalAmount);
                        var services = await _appointmentrepository.GetAppointmentServices(appointment.Id);
                        foreach (var service in services)
                        {
                            await _appointmentrepository.UpdateServiceRevenue(service.ClinicServiceId, service.Price ?? 0);
                        }

                        // Lấy appointment mới sau khi đã cập nhật
                        var updatedAppointment = await _appointmentrepository.GetAppointmentByID(appointment.Id);
                        await UpdatePetVaccineInfo(updatedAppointment, services);

                        result.Data = new AppointmentStatusResultModel
                        {
                            Status = appointmentUpdate.Status,
                            Services = services,
                            TotalPrice = totalAmount
                        };
                         _messageBus.PublishAppointmentActivity(
                        "APPOINTMENT_COMPLETED", 
                        appointment.Id);
                         _messageBus.PublishAppointmentReviewReminderNotification(
                        "APPOINTMENT_REVIEW_REMINDER",
                        appointment.Id);
                    }
                    else if (appointmentUpdate.Status == "Confirmed")
                    {
                        // UPDATE USAGE LIMIT PROMOTION
                        var appointmentPromotions = await _appointmentrepository.GetAppointmentPromotions(appointment.Id);
                        foreach (var appointmentPromotion in appointmentPromotions)
                        {
                            if (appointmentPromotion.Promotion != null && appointmentPromotion.Promotion.UsageLimit > 0)
                            {
                                appointmentPromotion.Promotion.UsageLimit -= 1;
                                await _appointmentrepository.UpdatePromotion(appointmentPromotion.Promotion);
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(appointments.Email))
                        {
                            string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TemplateEmail", "ConfirmAppointment.html");
                            string Html = File.ReadAllText(FilePath);
                            Html = Html.Replace("{{CustomerName}}", appointments.FullName);
                            Html = Html.Replace("{{StartAt}}", appointments.CreatedAt?.ToString("dd/MM") ?? "N/A");
                            Html = Html.Replace("{{StartAt}}", appointments.StartAt?.ToString("HH:mm") ?? "N/A");
                            Html = Html.Replace("{{EndAt}}", appointments.EndAt?.ToString("HH:mm") ?? "N/A");
                            bool EmailSent = await Email.SendEmail(appointments.Email, "Confirm appointment", Html);
                        }
                        else
                        {
                            Console.WriteLine("Email does not exist. Skipping email notification.");
                        }
                          _messageBus.PublishAppointmentActivity(
                        "APPOINTMENT_CONFIRMED", 
                        appointment.Id);
                    }
                    else if (appointmentUpdate.Status == "Canceled")
                    {
                        if (!string.IsNullOrWhiteSpace(appointments.Email))
                        {
                            string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TemplateEmail", "CancelAppointment.html");
                            string Html = File.ReadAllText(FilePath);
                            Html = Html.Replace("{{CustomerName}}", appointments.FullName);
                            Html = Html.Replace("{{StartAt}}", appointments.CreatedAt?.ToString("dd/MM") ?? "N/A");
                            Html = Html.Replace("{{StartAt}}", appointments.StartAt?.ToString("HH:mm") ?? "N/A");
                            Html = Html.Replace("{{EndAt}}", appointments.EndAt?.ToString("HH:mm") ?? "N/A");
                            bool EmailSent = await Email.SendEmail(appointments.Email, "Cancel appointment", Html);
                        }
                        else
                        {
                            Console.WriteLine("Email does not exist. Skipping email notification.");
                        }
                          _messageBus.PublishAppointmentActivity(
                        "APPOINTMENT_CANCELLED", 
                        appointment.Id);
                    }

                    result.IsSuccess = true;
                    result.Code = 200;
                    result.Message = "Appointment updated successfully.";
                }
                // Xử lý cho khách
                else if (appointment.UserId == null)
                {
                    // 1. Cập nhật GuestUser
                    if (appointment.GuestUserId.HasValue)
                    {
                        var guestUser = await _appointmentrepository.GetGuestUserByID(appointment.GuestUserId);
                        if (guestUser != null)
                        {
                            // Kiểm tra trùng lặp số điện thoại
                            if (guestUser.PhoneNumber != appointmentUpdate.PhoneNumber)
                            {
                                var existingGuestWithSamePhone = await _appointmentrepository.GetGuestUserByPhoneNumber(appointmentUpdate.PhoneNumber);
                                if (existingGuestWithSamePhone != null && existingGuestWithSamePhone.Id != appointment.GuestUserId)
                                {
                                    result.IsSuccess = false;
                                    result.Code = 400;
                                    result.Message = "Phone number already exists for another guest user";
                                    return result;
                                }
                            }

                            // Cập nhật thông tin GuestUser
                            guestUser.FullName = appointmentUpdate.FullName;
                            guestUser.PhoneNumber = appointmentUpdate.PhoneNumber;
                            guestUser.Email = appointmentUpdate.Email;
                            guestUser.Address = appointmentUpdate.Address;

                            // Cập nhật GuestUser riêng biệt
                            await _appointmentrepository.UpdateGuestUser(guestUser);
                        }
                    }

                    // 2. Cập nhật GuestPet
                    if (appointment.GuestPetId.HasValue)
                    {
                        var guestPet = await _appointmentrepository.GetGuestPetByID(appointment.GuestPetId);
                        if (guestPet != null)
                        {
                            guestPet.Name = appointmentUpdate.PetName;
                            guestPet.Species = appointmentUpdate.Species;
                            guestPet.Gender = appointmentUpdate.Gender;
                            guestPet.DateOfBirth = appointmentUpdate.DataOfBirth;

                            // Cập nhật GuestPet riêng biệt
                            await _appointmentrepository.UpdateGuestPet(guestPet);
                        }
                    }

                    // 3. Cập nhật thông tin cuộc hẹn cơ bản
                    DateTime? endAt = null;
                    if (appointmentUpdate.Status == "Completed")
                    {
                        endAt = DateTime.Now.AddHours(7);
                    }

                    await _appointmentrepository.UpdateAppointmentBasicInfo(
                        appointment.Id,
                        appointmentUpdate.Status,
                        appointmentUpdate.StartAt,
                        appointmentUpdate.Note,
                        endAt
                    );

                    // 4. Cập nhật dịch vụ
                    var existingServices = appointment.AppointmentClinicServices?.ToList() ?? new List<AppointmentClinicService>();
                    var updatedServiceIds = appointmentUpdate.ServiceIds ?? new List<Guid>();

                    // Xác định các dịch vụ cần xóa
                    var serviceIdsToRemove = existingServices
                        .Where(service => !updatedServiceIds.Contains(service.ClinicServiceId))
                        .Select(service => service.Id)
                        .ToList();

                    // Xóa các dịch vụ không còn trong danh sách cập nhật
                    foreach (var serviceId in serviceIdsToRemove)
                    {
                        await _appointmentrepository.RemoveAppointmentClinicServiceById(serviceId);
                    }

                    // Xác định các dịch vụ mới cần thêm
                    var newServiceIds = updatedServiceIds
                        .Where(serviceId => !existingServices.Any(existing => existing.ClinicServiceId == serviceId))
                        .ToList();

                    // Thêm các dịch vụ mới
                    foreach (var serviceId in newServiceIds)
                    {
                        var newService = new AppointmentClinicService
                        {
                            Id = Guid.NewGuid(),
                            AppointmentId = appointment.Id,
                            ClinicServiceId = serviceId,
                            DateGiven = DateTimeOffset.Now.DateTime,
                            Notes = "Add successfully"
                        };
                        await _appointmentrepository.InsertAppointmentClinicService(newService);
                    }

                    // 5. Xử lý các trạng thái đặc biệt
                    if (appointmentUpdate.Status == "Completed")
                    {
                        // Sử dụng FinalAmount đã được tính sẵn
                        var totalAmount = appointment.FinalAmount ?? 0;
                        // Cập nhật bảng UserBookingSummary
                        if (appointment.UserId.HasValue)
                        {
                            await UpdateUserBookingSummary(appointment.UserId.Value, totalAmount);
                        }
                        await _appointmentrepository.UpdateDailyRevenue(totalAmount);
                        var services = await _appointmentrepository.GetAppointmentServices(appointment.Id);
                        foreach (var service in services)
                        {
                            await _appointmentrepository.UpdateServiceRevenue(service.ClinicServiceId, service.Price ?? 0);
                        }

                        // Lấy appointment mới sau khi đã cập nhật
                        var updatedAppointment = await _appointmentrepository.GetAppointmentByID(appointment.Id);
                        await UpdatePetVaccineInfo(updatedAppointment, services);

                        result.Data = new AppointmentStatusResultModel
                        {
                            Status = appointmentUpdate.Status,
                            Services = services,
                            TotalPrice = totalAmount
                        };
                         _messageBus.PublishAppointmentActivity(
                        "APPOINTMENT_COMPLETED", 
                        appointment.Id);
                    }
                    else if (appointmentUpdate.Status == "Confirmed")
                    {
                        if (!string.IsNullOrWhiteSpace(appointments.Email))
                        {
                            string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TemplateEmail", "ConfirmAppointment.html");
                            string Html = File.ReadAllText(FilePath);
                            Html = Html.Replace("{{CustomerName}}", appointments.FullName);
                            Html = Html.Replace("{{StartAt}}", appointments.CreatedAt?.ToString("dd/MM") ?? "N/A");
                            Html = Html.Replace("{{StartAt}}", appointments.StartAt?.ToString("HH:mm") ?? "N/A");
                            Html = Html.Replace("{{EndAt}}", appointments.EndAt?.ToString("HH:mm") ?? "N/A");
                            bool EmailSent = await Email.SendEmail(appointments.Email, "Confirm appointment", Html);
                        }
                        else
                        {
                            Console.WriteLine("Email does not exist. Skipping email notification.");
                        }
                         _messageBus.PublishAppointmentActivity(
                        "APPOINTMENT_CONFIRMED", 
                        appointment.Id);
                    }
                    else if (appointmentUpdate.Status == "Canceled")
                    {
                        if (!string.IsNullOrWhiteSpace(appointments.Email))
                        {
                            string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TemplateEmail", "CancelAppointment.html");
                            string Html = File.ReadAllText(FilePath);
                            Html = Html.Replace("{{CustomerName}}", appointments.FullName);
                            Html = Html.Replace("{{StartAt}}", appointments.CreatedAt?.ToString("dd/MM") ?? "N/A");
                            Html = Html.Replace("{{StartAt}}", appointments.StartAt?.ToString("HH:mm") ?? "N/A");
                            Html = Html.Replace("{{EndAt}}", appointments.EndAt?.ToString("HH:mm") ?? "N/A");
                            bool EmailSent = await Email.SendEmail(appointments.Email, "Cancel appointment", Html);
                        }
                        else
                        {
                            Console.WriteLine("Email does not exist. Skipping email notification.");
                        }
                           _messageBus.PublishAppointmentActivity(
                        "APPOINTMENT_CANCELLED", 
                        appointment.Id);
                    }

                    result.IsSuccess = true;
                    result.Code = 200;
                    result.Message = "Appointment updated successfully.";
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Code = 500; // Internal server error
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }

            return result;
        }


        //Helper method for update pet vaccine in information
        private async Task UpdatePetVaccineInfo(Appointment appointment, List<AppointmentServiceDetailModel> services)
        {
            // Chỉ xử lý nếu có pet ID
            if (appointment.PetId == null)
                return;

            // Lấy danh sách các dịch vụ thuộc category "Vaccination"
            var vaccinationServices = await _appointmentrepository.GetVaccinationServices(services.Select(s => s.ClinicServiceId).ToList());

            if (vaccinationServices == null || !vaccinationServices.Any())
                return;

            foreach (var vaccineService in vaccinationServices)
            {
                // Kiểm tra xem pet đã có vaccine này chưa
                var existingPetVaccine = await _appointmentrepository.GetPetVaccineByNameAndPetId(vaccineService.Name, appointment.PetId.Value);

                if (existingPetVaccine != null)
                {
                    // Nếu đã có, thêm một liều mới
                    var nextDoseNumber = (existingPetVaccine.UserPetVaccineDoses.Count > 0)
                        ? existingPetVaccine.UserPetVaccineDoses.Max(d => d.DoseNumber) + 1
                        : 1;

                    var newDose = new UserPetVaccineDose
                    {
                        Id = Guid.NewGuid(),
                        UserPetVaccineId = existingPetVaccine.Id,
                        DoseNumber = nextDoseNumber,
                        DateGiven = appointment.EndAt ?? DateTime.UtcNow
                    };

                    await _appointmentrepository.AddUserPetVaccineDose(newDose);

                    // Cập nhật số liều
                    existingPetVaccine.NumberOfDoses = nextDoseNumber;
                    await _appointmentrepository.UpdateUserPetVaccine(existingPetVaccine);
                }
                else
                {
                    // Nếu chưa có, tạo mới vaccine và liều đầu tiên
                    var systemVaccine = await _appointmentrepository.GetVaccineByName(vaccineService.Name);

                    var newPetVaccine = new UserPetVaccine
                    {
                        Id = Guid.NewGuid(),
                        PetId = appointment.PetId.Value,
                        VaccineId = systemVaccine?.Id,
                        Name = vaccineService.Name,
                        NumberOfDoses = 1
                    };

                    await _appointmentrepository.AddUserPetVaccine(newPetVaccine);

                    var firstDose = new UserPetVaccineDose
                    {
                        Id = Guid.NewGuid(),
                        UserPetVaccineId = newPetVaccine.Id,
                        DoseNumber = 1,
                        DateGiven = appointment.EndAt ?? DateTime.UtcNow
                    };

                    await _appointmentrepository.AddUserPetVaccineDose(firstDose);
                }
            }
        }

        public async Task<ResultModel> GetPetsByPhoneOrEmail(string? phone, string? email)
        {
            var result = new ResultModel();

            try
            {
                if (string.IsNullOrEmpty(phone) && string.IsNullOrEmpty(email))
                {
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = "Either phone number or email must be provided";
                    return result;
                }

                var pets = await _appointmentrepository.GetPetsByPhoneOrEmail(phone, email);
                
                if (pets == null || !pets.Any())
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "No pets found for this user";
                    return result;
                }

                var petList = pets.Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    DateOfBirth = p.DateOfBirth,
                    Gender = p.Gender,
                    Species = p.Species
                }).ToList();

                result.IsSuccess = true;
                result.Code = 200;
                result.Data = petList;
                result.Message = "Successfully retrieved pet list";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }

            return result;
        }
    }
}
