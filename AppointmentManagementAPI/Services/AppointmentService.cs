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

                var appointmentList = appointments.Select(a => new AppointmentListModel
                {
                    Id = a.Id,
                    CustomerName = a.CustomerName,
                    PetName = a.PetName,
                    Date = a.Date,
                    StartTime = a.StartTime,
                    ServiceType = a.ServiceType,
                    Status = a.Status,
                }).ToList();

                var paginatedResult = await Pagination.GetPagination(appointmentList, page, 10);
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
                    Result.Code = 404;
                    Result.Message = "Not found";
                    return Result;
                }
                if (appointmentstatusmodel.Status == "Confirmed")
                {
                    string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TemplateEmail", "ConfirmAppointment.html");
                    string Html = File.ReadAllText(FilePath);
                    Html = Html.Replace("{{CustomerName}}", appointments.FullName);
                    Html = Html.Replace("{{StartAt}}", appointments.CreatedAt?.ToString("dd/MM") ?? "N/A");
                    Html = Html.Replace("{{StartAt}}", appointments.StartAt?.ToString("HH:mm") ?? "N/A");
                    Html = Html.Replace("{{EndAt}}", appointments.EndAt?.ToString("HH:mm") ?? "N/A");
                    bool EmailSent = await Email.SendEmail(appointments.Email, "Confirm appointment", Html);
                }
                if (appointmentstatusmodel.Status == "Canceled")
                {
                    string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TemplateEmail", "CancelAppointment.html");
                    string Html = File.ReadAllText(FilePath);
                    Html = Html.Replace("{{CustomerName}}", appointments.FullName);
                    Html = Html.Replace("{{StartAt}}", appointments.CreatedAt?.ToString("dd/MM") ?? "N/A");
                    Html = Html.Replace("{{StartAt}}", appointments.StartAt?.ToString("HH:mm") ?? "N/A");
                    Html = Html.Replace("{{EndAt}}", appointments.EndAt?.ToString("HH:mm") ?? "N/A");
                    bool EmailSent = await Email.SendEmail(appointments.Email, "Cancel appointment", Html);
                }
                if (appointmentstatusmodel.Status == "Completed")
                {
                    appointment.EndAt = DateTimeOffset.Now.DateTime;
                    string formattedDate = appointment.EndAt?.ToString("yyyy-MM-ddTHH:mm");


                }
                appointment.Status = appointmentstatusmodel.Status;
                _ = await _appointmentrepository.Update(appointment);
                Result.IsSuccess = true;
                Result.Data = appointment;
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

        public async Task<ResultModel> AddAppointment(string token, AppointmentAddModel appointment)
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
                // Kiểm tra và tạo Guest User nếu chưa có
                Guid? guestUserId = null;
                if (!appointment.GuestUserId.HasValue || appointment.GuestUserId.Value == Guid.Empty)
                {
                    if (!string.IsNullOrEmpty(appointment.GuestPhoneNumber))
                    {
                        var guestUser = await _appointmentrepository.GetGuestUserByPhoneNumber(appointment.GuestPhoneNumber);
                        if (guestUser == null)
                        {
                            guestUserId = Guid.NewGuid();
                            var newGuestUser = new GuestUser
                            {
                                Id = guestUserId.Value,
                                PhoneNumber = appointment.GuestPhoneNumber,
                                FullName = appointment.GuestFullName,
                                Email = appointment.GuestEmail,
                                Address = appointment.Address,
                                CreatedAt = DateTimeOffset.Now.DateTime
                            };
                            await _appointmentrepository.InsertGuestUser(newGuestUser);
                        }
                        else
                        {
                            guestUserId = guestUser.Id;
                        }
                    }
                }
                else
                {
                    guestUserId = appointment.GuestUserId;
                }

                // Kiểm tra và tạo Guest Pet nếu chưa có
                Guid? guestPetId = null;
                if (!appointment.GuestPetId.HasValue || appointment.GuestPetId.Value == Guid.Empty)
                {
                    if (!string.IsNullOrEmpty(appointment.GuestPetName) && guestUserId.HasValue)
                    {
                        var guestPet = await _appointmentrepository.GetGuestPetByNameAndGuestUserId(appointment.GuestPetName, guestUserId.Value);
                        if (guestPet == null)
                        {
                            guestPetId = Guid.NewGuid();
                            var newGuestPet = new GuestPet
                            {
                                Id = guestPetId.Value,
                                Name = appointment.GuestPetName,
                                DateOfBirth = appointment.GuestPetDateOfBirth,
                                Gender = appointment.GuestPetGender,
                                Species = appointment.GuestPetSpecies,
                                GuestUserId = guestUserId.Value,
                                CreatedAt = DateTimeOffset.Now.DateTime
                            };
                            await _appointmentrepository.InsertGuestPet(newGuestPet);
                        }
                        else
                        {
                            guestPetId = guestPet.Id;
                        }
                    }
                }
                else
                {
                    guestPetId = appointment.GuestPetId;
                }

                // Tạo appointment mới
                var newAppointment = new Appointment
                {
                    Id = Guid.NewGuid(),
                    UserId = appointment.UserId,
                    PetId = appointment.PetId,
                    GuestUserId = guestUserId,
                    GuestPetId = guestPetId,
                    ClinicServiceId = appointment.ClinicServiceId,
                    CreatedAt = DateTimeOffset.Now.DateTime,
                    StartAt = appointment.StartAt,
                    Status = appointment.Status,
                    Note = appointment.Note,
                };

                // Lưu appointment vào cơ sở dữ liệu
                await _appointmentrepository.Insert(newAppointment);

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
                var appointment = await _appointmentrepository.GetAppointmentByID(appointmentID);
                await _appointmentrepository.Remove(appointment);
                result.IsSuccess = true;
                result.Code = 200;
                result.Message = "Appointment deleted successfully.";
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

        public async Task <ResultModel> GetAppointmentDetail(string token, Guid guid)
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
                        ClinicServiceId = appointmentEntity.ClinicService?.Id,
                        ServiceName = appointmentEntity.ClinicService?.Name,

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

                appointment.ClinicServiceId = appointmentUpdate.ClinicServiceId;
                appointment.Status = appointmentUpdate.Status;
                appointment.StartAt = appointmentUpdate.StartAt;
                appointment.Note = appointmentUpdate.Note;
                var appointments = await _appointmentrepository.GetAppointmentAndUserEmail(appointmentUpdate.Id);
                if (appointmentUpdate.Status == "Confirmed")
                {
                    string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TemplateEmail", "ConfirmAppointment.html");
                    string Html = File.ReadAllText(FilePath);
                    Html = Html.Replace("{{CustomerName}}", appointments.FullName);
                    Html = Html.Replace("{{StartAt}}", appointments.CreatedAt?.ToString("dd/MM") ?? "N/A");
                    Html = Html.Replace("{{StartAt}}", appointments.StartAt?.ToString("HH:mm") ?? "N/A");
                    Html = Html.Replace("{{EndAt}}", appointments.EndAt?.ToString("HH:mm") ?? "N/A");
                    bool EmailSent = await Email.SendEmail(appointments.Email, "Confirm appointment", Html);
                }
                if (appointmentUpdate.Status == "Canceled")
                {
                    string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TemplateEmail", "CancelAppointment.html");
                    string Html = File.ReadAllText(FilePath);
                    Html = Html.Replace("{{CustomerName}}", appointments.FullName);
                    Html = Html.Replace("{{StartAt}}", appointments.CreatedAt?.ToString("dd/MM") ?? "N/A");
                    Html = Html.Replace("{{StartAt}}", appointments.StartAt?.ToString("HH:mm") ?? "N/A");
                    Html = Html.Replace("{{EndAt}}", appointments.EndAt?.ToString("HH:mm") ?? "N/A");
                    bool EmailSent = await Email.SendEmail(appointments.Email, "Cancel appointment", Html);
                }

                if (appointmentUpdate.Status == "Complete")
                {
                    appointment.EndAt = DateTimeOffset.Now.DateTime;
                    string formattedDate = appointment.EndAt?.ToString("yyyy-MM-ddTHH:mm");
                }
                await _appointmentrepository.Update(appointment);

                result.IsSuccess = true;
                result.Code = 200;
                result.Message = "Appointment updated successfully";
                
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Code = 500; // Internal Server Error
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }

            return result;
        }

    }
}
