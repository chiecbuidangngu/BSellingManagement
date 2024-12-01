using BookSellingManagement.Models;
using BookSellingManagement.Models.ViewModels;
using BookSellingManagement.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BookSellingManagement.Models.OrderModel;

namespace BookSellingManagement.Controllers
{

    public class AccountController : Controller
    {
        private UserManager<AppUserModel> _userManager;
        private SignInManager<AppUserModel> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _dataContext;
        public AccountController(SignInManager<AppUserModel> signInManager, UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager, DataContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _dataContext = context;
         
        }

        [HttpGet] 
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl }); // Trả về View form đăng nhập
        }
    
       [HttpPost]
       public async Task<IActionResult> Login(LoginViewModel loginVM)
       {
           if (ModelState.IsValid)
           {
               // Kiểm tra thông tin đăng nhập
               var result = await _signInManager.PasswordSignInAsync(loginVM.Username, loginVM.Password, false, false);

               if (result.Succeeded)
               {
                   // Lấy thông tin người dùng
                   var user = await _userManager.FindByNameAsync(loginVM.Username);

                   if (user != null)
                   {
                       // Kiểm tra vai trò của người dùng
                       var roles = await _userManager.GetRolesAsync(user);

                       // Điều hướng dựa trên vai trò
                       if (roles.Contains("Người dùng"))
                       {

                            return RedirectToAction("Index", "Books");
                        }

                        else
                       {

                         
                            return RedirectToAction("Index", "Home", new { area = "admin" });
                        }
                   }
               }
               ModelState.AddModelError("", "Tên người dùng hoặc mật khẩu không chính xác");
           }

           return View(loginVM); // Trả lại View nếu thông tin không hợp lệ
       }
        public IActionResult Register()
        {
            
            return View();
        }
     
        public async Task<IActionResult> UpdateAccount()
        {
            if (!(User.Identity?.IsAuthenticated ?? false))
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy User ID và Email từ Claims
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            // Truy vấn người dùng dựa trên email
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound();
            }

            // Đổ dữ liệu vào UserModel
            var model = new AppUserModel
            {
                FullName = user.FullName,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateInfoAccount(AppUserModel user)
        {
            // Lấy UserId từ Claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Truy vấn người dùng từ UserManager
            var userById = await _userManager.FindByIdAsync(userId);

            if (userById == null)
            {
                return NotFound(); // Trường hợp người dùng không tồn tại
            }

            // Cập nhật thông tin người dùng
            userById.FullName = user.FullName;
            userById.UserName = user.UserName;
            userById.Email = user.Email;
            userById.PhoneNumber = user.PhoneNumber;
            userById.Address = user.Address;

            // Cập nhật thông tin người dùng thông qua UserManager
            var result = await _userManager.UpdateAsync(userById);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Cập nhật thông tin tài khoản thành công"; // Hiển thị thông báo thành công
                return RedirectToAction("UpdateAccount", "Account"); // Quay lại trang cập nhật thông tin
            }
            else
            {
                // Xử lý lỗi nếu có
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(user); // Trả lại view nếu có lỗi
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserModel user)
        {
            if (ModelState.IsValid)
            {
                var newUser = new AppUserModel
                {
                    UserName = user.Username,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FullName = user.FullName,
                    Address = user.Address
                };

                var result = await _userManager.CreateAsync(newUser, user.Password);
                if (result.Succeeded)
                {
                    // Gán vai trò mặc định cho người dùng
                    string defaultRole = "Người dùng";
                    await _userManager.AddToRoleAsync(newUser, defaultRole);

                    return RedirectToAction("Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(user);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
                return View();
        }
        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await _signInManager.SignOutAsync();
            return Redirect(returnUrl);
        }
       

    }
}