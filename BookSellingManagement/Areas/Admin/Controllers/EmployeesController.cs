using BookSellingManagement.Models;
using BookSellingManagement.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookSellingManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Quản trị viên")]
    public class EmployeesController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _dataContext;

        public EmployeesController(DataContext context, UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dataContext = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pg = 1, string search = "")
        {
            const int pageSize = 10; // Số lượng nhân viên mỗi trang

            if (pg < 1)
            {
                pg = 1;
            }

            // Lấy danh sách nhân viên
            var employeesQuery = from u in _userManager.Users
                                 join ur in _dataContext.UserRoles on u.Id equals ur.UserId
                                 join r in _dataContext.Roles on ur.RoleId equals r.Id
                                 where r.Name == "Nhân viên"
                                 select u; // Chỉ chọn đối tượng AppUserModel

            // Áp dụng bộ lọc tìm kiếm nếu có từ khóa
            if (!string.IsNullOrEmpty(search))
            {
                employeesQuery = employeesQuery.Where(u =>
                    u.UserName.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.PhoneNumber.Contains(search));

                ViewBag.Search = search; // Lưu từ khóa tìm kiếm để hiển thị lại trên giao diện
            }

            int recsCount = await employeesQuery.CountAsync(); // Tổng số nhân viên sau khi lọc

            // Tạo đối tượng phân trang
            var pager = new Paginate(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;

            // Áp dụng phân trang
            var data = await employeesQuery
                .OrderBy(u => u.UserName) // Sắp xếp theo UserName hoặc tiêu chí khác nếu cần
                .Skip(recSkip)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Pager = pager;

            return View(data);
        }



        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var role = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(role, "Id", "Name"); 
            return View(new AppUserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppUserModel user)
        {
            if (ModelState.IsValid)
            {
                // Tạo người dùng
                var createUserResult = await _userManager.CreateAsync(user, user.PasswordHash);
                if (createUserResult.Succeeded)
                {
                    var createdUser = await _userManager.FindByEmailAsync(user.Email);

                    // Gán vai trò "Nhân viên" cho người dùng mới
                    var role = await _roleManager.FindByNameAsync("Nhân viên"); // Tìm vai trò "Nhân viên"
                    if (role != null)
                    {
                        var addToRoleResult = await _userManager.AddToRoleAsync(createdUser, role.Name);
                        if (!addToRoleResult.Succeeded)
                        {
                            foreach (var error in addToRoleResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }
                    }

                    TempData["SuccessMessage"] = "Thêm mới tài khoản nhân viên thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in createUserResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(user);
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Model có một vài thứ đang lỗi";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var role = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(role, "Id", "Name");

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AppUserModel user)
        {
            var existingUser = await _userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                existingUser.UserName = user.UserName;
                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.RoleId = user.RoleId;

                var updateUserResult = await _userManager.UpdateAsync(existingUser);
                if (updateUserResult.Succeeded)
                {
                    TempData["SuccessMessage"] = "Chỉnh sửa tài khoản nhân viên thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    AddIdentityErrors(updateUserResult);
                    return View(existingUser);
                }
            }

            var role = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(role, "Id", "Name");

            TempData["ErrorMessage"] = "Model có một vài thứ đang lỗi";
            var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            string errorMessage = string.Join("\n", errors);
            return View(existingUser);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                return RedirectToAction("Error");
            }

            TempData["SuccessMessage"] = "Xóa tài khoản nhân viên thành công!";
            return RedirectToAction("Index");
        }

        private void AddIdentityErrors(IdentityResult identityResult)
        {
            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        public async Task<IActionResult> Detail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
    }
}
