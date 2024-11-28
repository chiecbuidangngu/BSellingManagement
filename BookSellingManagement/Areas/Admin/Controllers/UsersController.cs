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
    public class UsersController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _dataContext;

        public UsersController(DataContext context, UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dataContext = context;

        }
        [HttpGet]
        public async Task<IActionResult> Index(int pg = 1)
        {
            const int pageSize = 10; // Số lượng bản ghi mỗi trang

            if (pg < 1)
            {
                pg = 1;
            }

            var usersWithRolesQuery =
                from u in _dataContext.Users
                join ur in _dataContext.UserRoles on u.Id equals ur.UserId
                join r in _dataContext.Roles on ur.RoleId equals r.Id
                select new
                {
                    User = u,          // Giữ đối tượng người dùng
                    RoleName = r.Name  // Lấy tên vai trò
                };

            var usersWithRoles = await usersWithRolesQuery.ToListAsync();

            int recsCount = usersWithRoles.Count(); // Tổng số bản ghi

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize;

            var data = usersWithRoles.Skip(recSkip).Take(pager.PageSize).ToList();

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
                var createUserResult = await _userManager.CreateAsync(user, user.PasswordHash);
                if (createUserResult.Succeeded)
                {
                    var createUser = await _userManager.FindByEmailAsync(user.Email);
                    var userId = createUser.Id;
                    var roles = _roleManager.FindByIdAsync(user.RoleId);

                    var addToRoleResult = await _userManager.AddToRoleAsync(createUser, roles.Result.Name);
                    if(!addToRoleResult.Succeeded)
                    {
                        foreach (var error in createUserResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                    TempData["SuccessMessage"] = "Thêm mới tài khoản người dùng thành công!";
                    return RedirectToAction("Index", "Users");
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
            var role = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(role, "Id", "Name");
            return View(user);


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
            var existingUser = await _userManager.FindByIdAsync(id); // Lấy user dựa vào id
            if (existingUser == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Cập nhật thông tin người dùng, ngoại trừ mật khẩu
           
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;

                // Kiểm tra nếu vai trò thay đổi
                if (existingUser.RoleId != user.RoleId)
                {
                    // Xóa vai trò cũ
                    var oldRole = await _roleManager.FindByIdAsync(existingUser.RoleId);
                    if (oldRole != null)
                    {
                        await _userManager.RemoveFromRoleAsync(existingUser, oldRole.Name);
                    }

                    // Thêm vai trò mới
                    var newRole = await _roleManager.FindByIdAsync(user.RoleId);
                    if (newRole != null)
                    {
                        var isInRole = await _userManager.IsInRoleAsync(existingUser, newRole.Name);
                        if (!isInRole) // Chỉ thêm nếu chưa thuộc vai trò mới
                        {
                            var addToRoleResult = await _userManager.AddToRoleAsync(existingUser, newRole.Name);
                            if (!addToRoleResult.Succeeded)
                            {
                                AddIdentityErrors(addToRoleResult);
                                return View(existingUser);
                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Vai trò mới không hợp lệ.");
                        return View(existingUser);
                    }

                    // Cập nhật RoleId trong model
                    existingUser.RoleId = user.RoleId;
                }

                var updateUserResult = await _userManager.UpdateAsync(existingUser);
                if (updateUserResult.Succeeded)
                {
                    TempData["SuccessMessage"] = "Chỉnh sửa tài khoản người dùng thành công!";
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
            TempData["SuccessMessage"] = "Xóa tài khoản người dùng thành công!";
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
            var role = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(role, "Id", "Name");

            return View(user);
        }





    }

}

