using BookSellingManagement.Models;
using BookSellingManagement.Models.ViewModels;
using BookSellingManagement.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BookSellingManagement.Models.OrderModel;
using BookSellingManagement.Areas.Admin.Reponsitory;
using Newtonsoft.Json.Linq;

namespace BookSellingManagement.Controllers
{

    public class AccountController : Controller
    {
        private UserManager<AppUserModel> _userManager;
        private SignInManager<AppUserModel> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;
        public AccountController(IEmailSender emailSender,SignInManager<AppUserModel> signInManager, UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager, DataContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _dataContext = context;
            _emailSender = emailSender;
         
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
                       var roles = await _userManager.GetRolesAsync(user);

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
           return View(loginVM); 
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
                return NotFound(); 
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
                TempData["SuccessMessage"] = "Cập nhật thông tin tài khoản thành công"; 
                return RedirectToAction("UpdateAccount", "Account"); 
            }
            else
            {

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(user); 
        }
        public IActionResult Register()
        {
            return View();
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

                    TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";

                    var receiver = user.Email; 
                    var subject = "Thông báo Đăng ký thành công tại HuTa Book";
                    var message = $@"
                                        Chào mừng bạn đến với HuTa Book!
        
                                        Tài khoản của bạn đã được đăng ký thành công tại cửa hàng sách HuTa Book.
        
                                        Thông tin tài khoản:
                                        Họ tên: {user.FullName}
                                        Email: {user.Email}
                                        Số điện thoại: {user.PhoneNumber}
        
                                        Trân trọng,
                                        HuTa Book 
                                    ";
                    await _emailSender.SendEmailAsync(receiver, subject, message);
                    return RedirectToAction("Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> SendMailForgotPass(AppUserModel user)
        {
            var checkMail = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (checkMail == null)
            {
                TempData["ErrorMessage"] = "Email chưa được đăng ký";
                return RedirectToAction("ForgotPassword", "Account");
            }
            else
            {
                string token = Guid.NewGuid().ToString();
                // Update token to user
                checkMail.Token = token;
                _dataContext.Update(checkMail);
                await _dataContext.SaveChangesAsync();

                var receiver = checkMail.Email;
                var subject = $"Thay đổi mật khẩu - {checkMail.Email}";
                var resetLink = $"{Request.Scheme}://{Request.Host}/Account/NewPassword?email={checkMail.Email}&token={token}";
                var message = $@"
                                Nhấn vào liên kết bên dưới để tạo mật khẩu mới:
                                {resetLink}
                                Nếu bạn không yêu cầu thay đổi mật khẩu, vui lòng bỏ qua email này.";

                await _emailSender.SendEmailAsync(receiver, subject, message);
            }
                TempData["SuccessMessage"] = $"Đã gửi một email đến {checkMail.Email}. Vui lòng kiểm tra hộp thư của bạn.";
                return RedirectToAction("ForgotPassword", "Account");

            }
        [HttpPost]
        public async Task<IActionResult> UpdateNewPassword(AppUserModel user)
        {
            var checkuser = await _userManager.Users.Where(u => u.Email == user.Email).Where(u => u.Token == user.Token).FirstOrDefaultAsync();
            if (checkuser != null)
            {
                string newtoken = Guid.NewGuid().ToString();
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var passwordHash = passwordHasher.HashPassword(checkuser, user.PasswordHash);

                checkuser.PasswordHash = passwordHash;
                checkuser.Token = newtoken;
                await _userManager.UpdateAsync(checkuser);
                TempData["SuccessMessage"] = "Cập nhật mật khẩu mới thành công";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                TempData["ErrorMessage"] = "Email không tìm thấy hoặc token chưa đúng";
                return RedirectToAction("ForgotPassword", "Account");
            }
            return View();
        }
            public async Task<IActionResult> NewPassword(AppUserModel user, string token)
            {
            var checkuser = await _userManager.Users.Where(u => u.Email == user.Email).Where(u => u.Token == user.Token).FirstOrDefaultAsync();
            if (checkuser != null)
            {
                ViewBag.Email = checkuser.Email;
                ViewBag.Token = token;
            }
            else
            {
                TempData["ErrorMessage"] = "Email không tìm thấy hoặc token chưa đúng";
                return RedirectToAction("ForgotPassword", "Account");
            }
            return View();
            }

        
        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await _signInManager.SignOutAsync();
            return Redirect(returnUrl);
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }
    }
}