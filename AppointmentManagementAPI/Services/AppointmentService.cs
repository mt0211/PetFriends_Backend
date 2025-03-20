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
        public AppointmentService(IAppointmentRepository appointmentrepository)
        {
            _appointmentrepository = appointmentrepository;
        }
        public async Task<ResultModel> GetAllAppointment(string token, int page)
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
                // Fetch raw data from the repository
                var appointments = await _appointmentrepository.GetAllApointment();

                if (appointments == null || !appointments.Any())
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "No appointments found";
                    return result;
                }

                if (page == 0)
                {
                    page = 1;
                }

                // Transform entities to DTO
                var appointmentList = appointments.Select(a => new AppointmentListModel
                {
                    Id = a.Id, // Lấy Id từ kiểu ẩn danh
                    CustomerName = a.UserName, // Lấy UserName từ kiểu ẩn danh
                    PetName = a.PetName, // Lấy PetName từ kiểu ẩn danh
                    Date = a.CreatedAt, // Lấy CreatedAt từ kiểu ẩn danh
                    StartTime = a.StartAt, // Lấy StartAt từ kiểu ẩn danh
                    ServiceNames = a.ServiceNames, // Lấy ServiceNames từ kiểu ẩn danh
                    Status = a.Status, // Lấy Status từ kiểu ẩn danh
                }).ToList();

                // Paginate the result
                var paginatedResult = await Pagination.GetPagination(appointmentList, page, 1000);
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = paginatedResult;
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

                if (appointmentstatusmodel.Status == "Completed")
                {
                    // Cập nhật trạng thái và thời gian hoàn thành
                    appointment.Status = appointmentstatusmodel.Status;
                    appointment.EndAt = DateTimeOffset.Now.DateTime;

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
                }
                else if (appointmentstatusmodel.Status == "Canceled")
                {
                    if (!string.IsNullOrWhiteSpace(appointments.Email)) {
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
                    // Tạo/Lấy GuestPet
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
                    CreatedAt = DateTimeOffset.Now.DateTime,
                    StartAt = appointment.StartAt,
                    Status = appointment.Status,
                    Note = appointment.Note,
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
                        DateGiven = DateTimeOffset.Now.DateTime,
                        Notes = "Add successfully"
                    };
                    await _appointmentrepository.InsertAppointmentClinicService(appointmentService);
                }

                result.IsSuccess = true;
                result.Code = 200;
                result.Message = "Successfully added new appointment";
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
                await _appointmentrepository.Remove(appointment);

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
                    appointment.Status = appointmentUpdate.Status;
                    appointment.StartAt = appointmentUpdate.StartAt;
                    appointment.Note = appointmentUpdate.Note;
                    var existingServices = appointment.AppointmentClinicServices.ToList();
                    var updatedServiceIds = appointmentUpdate.ServiceIds; 
                    var servicesToRemove = existingServices
                        .Where(service => !updatedServiceIds.Contains(service.ClinicServiceId))
                        .ToList();

                    foreach (var service in servicesToRemove)
                    {
                        await _appointmentrepository.RemoveAppointmentClinicService(service);
                    }
                    var newServiceIds = updatedServiceIds
                        .Where(serviceId => !existingServices.Any(existing => existing.ClinicServiceId == serviceId))
                        .ToList();
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

                    if (appointmentUpdate.Status == "Completed")
                    {
                        appointment.Status = appointmentUpdate.Status;
                        appointment.EndAt = DateTimeOffset.Now.DateTime;

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
                            await UpdatePetVaccineInfo(appointment, services);
                            
                            result.Data = new AppointmentStatusResultModel
                            {
                                Status = appointment.Status,
                                Services = services,
                                TotalPrice = totalAmount
                            };
                  
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
                    }
                    await _appointmentrepository.Update(appointment);

                    result.IsSuccess = true;
                    result.Code = 200;
                    result.Message = "Appointment updated successfully.";
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
    }
}
