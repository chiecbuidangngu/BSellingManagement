using System.ComponentModel.DataAnnotations;

namespace BookSellingManagement.Models
{
    public class UserModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Nhập tên người dùng")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
  
        public string Email { get; set; }

        [Required(ErrorMessage = "Nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Nhập họ và tên")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Nhập địa chỉ")]
        [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
        public string Address { get; set; }

        public string Role { get; set; }
    }

}
