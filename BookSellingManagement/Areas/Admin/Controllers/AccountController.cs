using BookSellingManagement.Models.ViewModels;
using BookSellingManagement.Models;
using BookSellingManagement.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace BookSellingManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Quản trị viên, Nhân viên")]
    public class AccountController : Controller
    {
        private UserManager<AppUserModel> _userManager;
        private SignInManager<AppUserModel> _signInManager;

        private readonly DataContext _dataContext;
        public AccountController(SignInManager<AppUserModel> signInManager, UserManager<AppUserModel> userManager, DataContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _dataContext = context;

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
