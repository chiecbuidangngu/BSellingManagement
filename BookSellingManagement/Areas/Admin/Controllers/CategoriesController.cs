using BookSellingManagement.Models;
using BookSellingManagement.Models.Book;
using BookSellingManagement.Models.Categories;
using BookSellingManagement.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookSellingManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Quản trị viên, Nhân viên")]
    public class CategoriesController : Controller
    {
        private readonly DataContext _dataContext;

        public CategoriesController(DataContext context)
        {
            _dataContext = context;

        }
        public async Task<IActionResult> Index(int pg = 1, string search = "")
        {
            const int pageSize = 10;

            if (pg < 1)
            {
                pg = 1;
            }

            // Lấy danh sách danh mục từ cơ sở dữ liệu
            var categories = _dataContext.Categories.AsQueryable();

            // Áp dụng bộ lọc tìm kiếm nếu có từ khóa
            if (!string.IsNullOrEmpty(search))
            {
                categories = categories.Where(c => c.CategoryName.Contains(search));
                ViewBag.Search = search; // Lưu từ khóa tìm kiếm để hiển thị lại trên giao diện
            }

            // Tính tổng số bản ghi sau khi lọc
            int recsCount = await categories.CountAsync();
            int recSkip = (pg - 1) * pageSize;

            // Phân trang
            var data = await categories
                .OrderBy(c => c.CategoryId) // Sắp xếp theo `CategoryId` hoặc cột khác nếu cần
                .Skip(recSkip)
                .Take(pageSize)
                .ToListAsync();

            // Tạo đối tượng phân trang
            var pager = new Paginate(recsCount, pg, pageSize);
            ViewBag.Pager = pager;

            return View(data);
        }

        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryModel category)
        {
            // Kiểm tra tính hợp lệ của model
            if (ModelState.IsValid)
            {
        
                category.CategorySlug = category.CategoryName.Replace(" ", "-");

      
                var slug = await _dataContext.Categories.FirstOrDefaultAsync(c => c.CategorySlug == category.CategorySlug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Thể loại đã có.");
                    return View(category);
                }

          
                category.CategoryId = Guid.NewGuid().ToString(); 

                // Tạo CategoryCode với định dạng TL + năm + số thứ tự
                string yearPrefix = "TL" + DateTime.Now.Year.ToString(); // TL2024
                var latestCategory = await _dataContext.Categories
                                                        .Where(c => c.CategoryCode.StartsWith(yearPrefix)) // Lọc thể loại trong năm hiện tại
                                                        .OrderByDescending(c => c.CategoryCode) // Sắp xếp giảm dần
                                                        .FirstOrDefaultAsync();

                int codeNumber = 1;
                if (latestCategory != null)
                {
                    string latestCode = latestCategory.CategoryCode.Substring(yearPrefix.Length); // Lấy phần sau TL2024
                    codeNumber = int.Parse(latestCode) + 1;
                }

                category.CategoryCode = yearPrefix + codeNumber.ToString("D3"); 

          
                _dataContext.Add(category);
                await _dataContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm thể loại thành công.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = "Model đang bị lỗi. Vui lòng kiểm tra lại.";
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

            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string CategoryId)
        {
            CategoryModel category = await _dataContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == CategoryId);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra nếu CategoryId là null hoặc không tồn tại
                if (string.IsNullOrEmpty(category.CategoryId))
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thể loại cần sửa.";
                    return RedirectToAction("Index");
                }

      
                category.CategorySlug = category.CategoryName.Replace(" ", "-");

               
                var slug = await _dataContext.Categories
                                              .FirstOrDefaultAsync(c => c.CategorySlug == category.CategorySlug && c.CategoryId != category.CategoryId);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Thể loại đã có.");
                    return View(category);
                }

                // Lấy thể loại từ cơ sở dữ liệu và gắn nó vào DbContext
                var existingCategory = await _dataContext.Categories
                                                          .AsNoTracking() // Tránh việc theo dõi thể loại này
                                                          .FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId);

                if (existingCategory != null)
                {
                    // Giữ nguyên CategoryCode khi cập nhật
                    category.CategoryCode = existingCategory.CategoryCode;
                }

                // Đảm bảo không có hai thể loại trùng nhau trong DbContext
                var trackedEntity = _dataContext.Entry(category);
                if (trackedEntity.State == EntityState.Detached)
                {
                    _dataContext.Attach(category);  // Gắn thể loại vào DbContext nếu chưa được theo dõi
                }


                _dataContext.Update(category);
                await _dataContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật thông tin thể loại thành công.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = "Model đang bị lỗi. Vui lòng kiểm tra lại.";
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

            return View(category);
        }

        public async Task<IActionResult> Delete(string CategoryId)
        {
            // Tìm sách trong cơ sở dữ liệu
            CategoryModel category = await _dataContext.Categories.FindAsync(CategoryId);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thể loại  cần xóa.";
                return RedirectToAction("Index");
            }

            _dataContext.Categories.Remove(category);
            await _dataContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa thể loại thành công.";
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Details(string CategoryId)
        {
            CategoryModel category = await _dataContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == CategoryId);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

    }
}

