using System.Drawing;
using AdminReport.DTO.ResultModel;
using AdminReport.Repositories;
using AdminReport.Utilities;

namespace AdminReport.Services
{
    public class AdminReportService : IAdminReportService
    {
        private readonly IAdminReportRepository _repository;
        public AdminReportService(IAdminReportRepository repository)
        {
            _repository = repository;
        }

        public async Task<ResultModel> GetDataUserStatus(string token, int year, int month)
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
            if (userId == null)
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Please authorize";
                return result;
            }
            var user = await _repository.GetUserByID(id);
            if (user.Role != "ADMIN")
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Permission denied!";
                return result;
            }
            try
            {
                var userData = await _repository.GetDataUserStatus(year, month);
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = userData;
                result.Message = "Successfully get data";
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
        public async Task<ResultModel> GetDataNewUser(string token, int year, int month)
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
            if (userId == null)
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Please authorize";
                return result;
            }
            var user = await _repository.GetUserByID(id);
            if (user.Role != "ADMIN")
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Permission denied!";
                return result;
            }
            try
            {
                var userData = await _repository.GetDataNewUser(year, month);
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = userData;
                result.Message = "Successfully get data";
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

        public async Task<ResultModel> GetPostDistribution(string token)
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
            if (userId == null)
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Please authorize";
                return result;
            }
            var user = await _repository.GetUserByID(id);
            if (user.Role != "ADMIN")
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Permission denied!";
                return result;
            }
            try
            {
                var postCount = await _repository.GetPostDistribution();
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = postCount;
                result.Message = "Successfully get data";
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
        public async Task<ResultModel> GetSystemVaccineCount(string token)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");

            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "Invalid user ID";
                return result;
            }

            try
            {
                var UsageLimit = await _repository.GetSystemVaccineCount();
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = UsageLimit;
                result.Message = "Successfully get vaccine usage statistics";
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
        public async Task<ResultModel> ExportAllReportsToExcel(string token, int year, int? month)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "Invalid user ID";
                return result;
            }

            var user = await _repository.GetUserByID(id);
            if (user == null || user.Role != "ADMIN")
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "Permission denied!";
                return result;
            }

            try
            {
                // Lấy dữ liệu từ repository
                var vaccineData = await _repository.GetTopVaccine();
                var postDistribution = await _repository.GetPostDistribution();
                var newUsers = await _repository.GetDataNewUser(year, month ?? 0);
                var userStatus = await _repository.GetDataUserStatusForExcel(year, month ?? 0);

                // Tạo file Excel với nhiều worksheet
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    // ================== 1. Worksheet cho Top Vaccines ==================
                    var vaccineWorksheet = package.Workbook.Worksheets.Add("Top Vaccines");

                    // --- Title: Top Vaccine Report (row 1) ---
                    vaccineWorksheet.Cells[1, 1].Value = "Top Vaccine Report";
                    var titleRange = vaccineWorksheet.Cells[1, 1, 1, 2];
                    titleRange.Merge = true;
                    titleRange.Style.Font.Bold = true;
                    titleRange.Style.Font.Size = 14;
                    titleRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    titleRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    titleRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                    titleRange.Style.Font.Color.SetColor(System.Drawing.Color.Black);

                    // Thêm viền cho titleRange
                    titleRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRange.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    titleRange.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    titleRange.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                    titleRange.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);

                    // --- Header (row 2) ---
                    vaccineWorksheet.Cells[2, 1].Value = "Vaccine Name";
                    vaccineWorksheet.Cells[2, 2].Value = "Number Of Doses";

                    using (var range = vaccineWorksheet.Cells[2, 1, 2, 2])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Font.Color.SetColor(ColorTranslator.FromHtml("#f2f4f4"));
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#1f618d"));

                        range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                        range.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    }

                    // --- Dữ liệu (bắt đầu từ row 3) ---
                    int row = 3;
                    foreach (var item in vaccineData)
                    {
                        vaccineWorksheet.Cells[row, 1].Value = item.Name;
                        vaccineWorksheet.Cells[row, 2].Value = item.NumberOfDoses;
                        row++;
                    }

                    // Thêm viền cho dữ liệu
                    if (row > 3)
                    {
                        var dataRange = vaccineWorksheet.Cells[3, 1, row - 1, 2];
                        dataRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                        dataRange.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                        dataRange.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                        dataRange.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                        dataRange.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    }

                    // Auto fit
                    vaccineWorksheet.Cells.AutoFitColumns();

                    // Tạo table (bao gồm header ở row 2 và dữ liệu đến row - 1)
                    var table = vaccineWorksheet.Tables.Add(vaccineWorksheet.Cells[2, 1, row - 1, 2], "VaccineTable");
                    table.ShowHeader = true;
                    table.ShowFilter = false;
                    table.TableStyle = OfficeOpenXml.Table.TableStyles.Medium2;


                    // ================== 2. Worksheet cho Post Distribution ==================
                    var postWorksheet = package.Workbook.Worksheets.Add("Post Distribution");

                    // --- Title: Post Distribution Report (row 1) ---
                    postWorksheet.Cells[1, 1].Value = "Post Distribution Report";
                    var titleRanges = postWorksheet.Cells[1, 1, 1, 2];
                    titleRanges.Merge = true;
                    titleRanges.Style.Font.Bold = true;
                    titleRanges.Style.Font.Size = 14;
                    titleRanges.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    titleRanges.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    titleRanges.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                    titleRanges.Style.Font.Color.SetColor(System.Drawing.Color.Black);

                    // Viền cho title
                    titleRanges.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRanges.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRanges.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRanges.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                    titleRanges.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    titleRanges.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    titleRanges.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                    titleRanges.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);

                    
                    // --- Header (row 2) ---
                    postWorksheet.Cells[2, 1].Value = "Status";
                    postWorksheet.Cells[2, 2].Value = "Percentage";

                    using (var range = postWorksheet.Cells[2, 1, 2, 2])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Font.Color.SetColor(ColorTranslator.FromHtml("#f2f4f4"));
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#1f618d"));

                        range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                        range.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    }

                    // --- Dữ liệu (row 3) ---
                    row = 3;
                    foreach (var item in postDistribution)
                    {
                        postWorksheet.Cells[row, 1].Value = item.type;
                        postWorksheet.Cells[row, 2].Value = item.value;
                        row++;
                    }

                    // Viền dữ liệu
                    if (row > 3)
                    {
                        var dataRange = postWorksheet.Cells[3, 1, row - 1, 2];
                        dataRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                        dataRange.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                        dataRange.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                        dataRange.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                        dataRange.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    }

                    postWorksheet.Cells.AutoFitColumns();
                    postWorksheet.Column(1).Width = 15; 
                    postWorksheet.Column(2).Width = 15;
                    var tablepost = postWorksheet.Tables.Add(postWorksheet.Cells[2, 1, row - 1, 2], "PostTable");
                    tablepost.ShowHeader = true;
                    tablepost.ShowFilter = false;
                    tablepost.TableStyle = OfficeOpenXml.Table.TableStyles.Medium2;


                    // ================== 3. Worksheet cho New Users ==================
                    var newUsersWorksheet = package.Workbook.Worksheets.Add("New Users");

                    // --- Title: New Users Report (row 1) ---
                    newUsersWorksheet.Cells[1, 1].Value = "New Users Report";
                    var titleRangess = newUsersWorksheet.Cells[1, 1, 1, 2];
                    titleRangess.Merge = true;
                    titleRangess.Style.Font.Bold = true;
                    titleRangess.Style.Font.Size = 14;
                    titleRangess.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    titleRangess.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    titleRangess.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                    titleRangess.Style.Font.Color.SetColor(System.Drawing.Color.Black);

                    // Viền cho title
                    titleRangess.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRangess.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRangess.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRangess.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                    titleRangess.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    titleRangess.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    titleRangess.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                    titleRangess.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);

                    newUsersWorksheet.Column(1).Width = 25; 
                    newUsersWorksheet.Column(2).Width = 25;
                    // --- Header (row 2) ---
                    newUsersWorksheet.Cells[2, 1].Value = "Period";
                    newUsersWorksheet.Cells[2, 2].Value = "Number of New Users";

                    using (var range = newUsersWorksheet.Cells[2, 1, 2, 2])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Font.Color.SetColor(ColorTranslator.FromHtml("#f2f4f4"));
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#1f618d"));

                        range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                        range.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    }

                    // --- Dữ liệu (row 3) ---
                    row = 3;
                    foreach (var item in newUsers)
                    {
                        // Nếu month > 0 => lấy day, ngược lại lấy month
                        string period = month.HasValue && month.Value > 0
                            ? (item.day != null ? item.day.ToString() : "")
                            : (item.month != null ? item.month.ToString() : "");

                        newUsersWorksheet.Cells[row, 1].Value = period;
                        newUsersWorksheet.Cells[row, 2].Value = item.value;
                        row++;
                    }

                    // Viền dữ liệu
                    if (row > 3)
                    {
                        var dataRange = newUsersWorksheet.Cells[3, 1, row - 1, 2];
                        dataRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                        dataRange.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                        dataRange.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                        dataRange.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                        dataRange.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    }

                    newUsersWorksheet.Cells.AutoFitColumns();

                    var tablenewuser = newUsersWorksheet.Tables.Add(newUsersWorksheet.Cells[2, 1, row - 1, 2], "NewUsersTable");
                    tablenewuser.ShowHeader = true;
                    tablenewuser.ShowFilter = false;
                    tablenewuser.TableStyle = OfficeOpenXml.Table.TableStyles.Medium2;


                    // ================== 4. Worksheet cho User Status ==================
                    var userStatusWorksheet = package.Workbook.Worksheets.Add("User Status");

                    // --- Title: New Users Report (row 1) ---
                    userStatusWorksheet.Cells[1, 1].Value = "Users Status Report";
                    var titleRangesss = userStatusWorksheet.Cells[1, 1, 1, 3];
                    titleRangesss.Merge = true;
                    titleRangesss.Style.Font.Bold = true;
                    titleRangesss.Style.Font.Size = 14;
                    titleRangesss.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    titleRangesss.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    titleRangesss.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                    titleRangesss.Style.Font.Color.SetColor(System.Drawing.Color.Black);


                    // Viền cho title
                    titleRangesss.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRangesss.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRangesss.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRangesss.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;


                    titleRangesss.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    titleRangesss.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    titleRangesss.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                    titleRangesss.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    // Tạo header 3 cột ở row 2
                    userStatusWorksheet.Cells[2, 1].Value = "Period";
                    userStatusWorksheet.Cells[2, 2].Value = "Active Users";
                    userStatusWorksheet.Cells[2, 3].Value = "Inactive Users";

                    // Format header (nếu muốn)
                    using (var range = userStatusWorksheet.Cells[2, 1, 2, 3])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Font.Color.SetColor(ColorTranslator.FromHtml("#f2f4f4"));
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#1f618d"));

                        range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                        range.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    }

                    // Ghi dữ liệu bắt đầu row = 2
                    row = 3;
                    foreach (var item in userStatus)
                    {
                        // Nếu là month => item.month, nếu là day => item.day
                        // Giả sử month => item.month:
                        userStatusWorksheet.Cells[row, 1].Value = item.month;  
                        userStatusWorksheet.Cells[row, 2].Value = item.ActiveUsers;
                        userStatusWorksheet.Cells[row, 3].Value = item.InactiveUsers;
                        row++;
                    }

                    // Viền dữ liệu
                    if (row > 3)
                    {
                        var dataRange = userStatusWorksheet.Cells[3, 1, row - 1, 3];
                        dataRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                        dataRange.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                        dataRange.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                        dataRange.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                        dataRange.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    }

                    // Auto fit
                    userStatusWorksheet.Cells.AutoFitColumns();

                    // Tạo table 3 cột
                    var tableuserstatus = userStatusWorksheet.Tables.Add(
                        userStatusWorksheet.Cells[2, 1, row - 1, 3],
                        "UserStatusTable"
                    );
                    tableuserstatus.ShowHeader = true;
                    tableuserstatus.ShowFilter = false;
                    tableuserstatus.TableStyle = OfficeOpenXml.Table.TableStyles.Medium2;


                    // ================== 5. Worksheet tổng quan (Summary) ==================
                    var summaryWorksheet = package.Workbook.Worksheets.Add("Summary");

                    summaryWorksheet.Cells[1, 1].Value = "Report Summary";
                    summaryWorksheet.Cells[1, 1].Style.Font.Bold = true;
                    summaryWorksheet.Cells[1, 1].Style.Font.Size = 14;

                    summaryWorksheet.Cells[3, 1].Value = "Report Date:";
                    summaryWorksheet.Cells[3, 2].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    summaryWorksheet.Cells[4, 1].Value = "Year:";
                    summaryWorksheet.Cells[4, 2].Value = year;

                    summaryWorksheet.Cells[5, 1].Value = "Month:";
                    summaryWorksheet.Cells[5, 2].Value = month.HasValue ? month.Value.ToString() : "All Months";

                    summaryWorksheet.Cells.AutoFitColumns();

                    // Convert to byte array
                    var fileBytes = package.GetAsByteArray();

                    result.IsSuccess = true;
                    result.Code = 200;
                    result.Data = Convert.ToBase64String(fileBytes);
                    result.Message = "Successfully exported all reports to Excel";
                }
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

    }
}
