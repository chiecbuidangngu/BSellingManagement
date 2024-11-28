using System.ComponentModel.DataAnnotations;

namespace BookSellingManagement.Models.ViewModels
{
 
    
        public class LoginViewModel
        {
            public string Id { get; set; }

            [Required(ErrorMessage = "Nhập tên người dùng")]
            public string Username { get; set; }

            [Required(ErrorMessage = "Nhập mật khẩu")]
            public string Password { get; set; }

             public string ReturnUrl { get; set; }
    }
    
}
