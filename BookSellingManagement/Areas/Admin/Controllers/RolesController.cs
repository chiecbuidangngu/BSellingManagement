using BookSellingManagement.Models;
using BookSellingManagement.Models.Categories;
using BookSellingManagement.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace BookSellingManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Quản trị viên")]
    public class RolesController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUserModel> _userManager; 

        public RolesController(DataContext context, RoleManager<IdentityRole> roleManager, UserManager<AppUserModel> userManager)
        {
            _dataContext = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string search = "")
        {
            // Lấy danh sách Roles từ cơ sở dữ liệu và sắp xếp theo Id
            var rolesQuery = _dataContext.Roles.AsQueryable();

            // Áp dụng bộ lọc tìm kiếm nếu có từ khóa
            if (!string.IsNullOrEmpty(search))
            {
                rolesQuery = rolesQuery.Where(r => r.Name.Contains(search)); // Tìm kiếm theo tên Role
                ViewBag.Search = search; // Để hiển thị lại từ khóa trên giao diện
            }

            var roles = await rolesQuery.OrderBy(r => r.Id).ToListAsync();

            return View(roles);
        }

        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IdentityRole role)
        {
            // Kiểm tra nếu tên vai trò trống
            if (string.IsNullOrEmpty(role.Name))
            {
                ModelState.AddModelError("", "Tên vai trò không được để trống.");
                return View(role);
            }

            // Kiểm tra xem vai trò có tồn tại trong cơ sở dữ liệu hay không
            if (await _roleManager.RoleExistsAsync(role.Name))
            {
                // Nếu vai trò đã tồn tại, thông báo lỗi
                ModelState.AddModelError("", "Vai trò này đã tồn tại.");
                return View(role);
            }

            // Nếu vai trò chưa tồn tại, tạo mới vai trò
            var result = await _roleManager.CreateAsync(new IdentityRole(role.Name));

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Tạo vai trò mới thành công!";
                return RedirectToAction("Index");
            }
            else
            {
                // Xử lý nếu có lỗi khi tạo vai trò
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(role);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var role = await _roleManager.FindByIdAsync(id);
            return View(role);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(string id, IdentityRole model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound();
                }
                role.Name = model.Name;
                try
                {
                    await _roleManager.UpdateAsync(role);
                    TempData["SuccessMessage"] = "Chỉnh sửa quyền thành công!";
                    return RedirectToAction("Index");
                }
                catch(Exception ex)
                    {
                    ModelState.AddModelError("", "Có lỗi xảy ra, vui lòng thử lại sau");
                }

            }
            return View(model ?? new IdentityRole { Id = id});
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Kiểm tra xem có người dùng nào gán vai trò này không
            var usersWithRole = await _userManager.GetUsersInRoleAsync(role.Name);
            if (usersWithRole.Any())
            {
                // Nếu có người dùng, không cho phép xóa và hiển thị thông báo lỗi
                TempData["ErrorMessage"] = "Không thể xóa vai trò vì có người dùng đang gán vai trò này.";
                return RedirectToAction("Index");
            }

            try
            {
                await _roleManager.DeleteAsync(role);
                TempData["SuccessMessage"] = "Xóa vai trò thành công!";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra, vui lòng thử lại sau");
            }

            return RedirectToAction("Index");
        }
    }
}
