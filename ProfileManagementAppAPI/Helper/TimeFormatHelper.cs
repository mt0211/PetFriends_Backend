using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ProfileManagementAppAPI.Helper
{
    public class TimeFormatHelper
    {
        public static string NormalizeTimeFormat(string timeString)
        {
            if (string.IsNullOrEmpty(timeString))
                return timeString;
                
            // Chuẩn hóa SA -> AM
            timeString = timeString.Replace("SA", "AM").Replace("sa", "AM");
            
            // Chuẩn hóa CH -> PM
            timeString = timeString.Replace("CH", "PM").Replace("ch", "PM");
            
            return timeString;
        }
        
        // Phân tích chuỗi thời gian với nhiều định dạng
        public static bool TryParseTime(string timeString, out DateTime result)
        {
            // Chuẩn hóa định dạng
            string normalizedTime = NormalizeTimeFormat(timeString);
            
            // Danh sách các định dạng thời gian được hỗ trợ
            string[] formats = new[] { 
                "h:mm tt", 
                "hh:mm tt", 
                "H:mm",     // 24-hour format
                "HH:mm"     // 24-hour format
            };
            
            return DateTime.TryParseExact(
                normalizedTime, 
                formats, 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out result);
        }
        
        // Định dạng thời gian hiển thị với cả AM/PM và SA/CH
        public static string FormatTimeWithBothNotations(DateTime time)
        {
            string amPmFormat = time.ToString("h:mm tt", CultureInfo.InvariantCulture);
            string vietnameseFormat = amPmFormat
                .Replace("AM", "SA")
                .Replace("PM", "CH");
                
            return vietnameseFormat;
        }
    }
}