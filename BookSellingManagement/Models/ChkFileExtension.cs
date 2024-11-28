using System;
using System.IO;

namespace BookSellingManagement.Models
{
    public class ChkFileExtension
    {
        // Danh sách các phần mở rộng được phép
        private static readonly string[] allowedExtensions = { ".jpg", ".png", ".jpeg" };

        // Phương thức kiểm tra phần mở rộng tệp
        public static bool IsAllowedExtension(string fileName)
        {
            // Lấy phần mở rộng từ tên tệp
            string extension = Path.GetExtension(fileName);

            // Kiểm tra xem phần mở rộng có nằm trong danh sách cho phép không
            return Array.Exists(allowedExtensions, ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
        }
    }
}
