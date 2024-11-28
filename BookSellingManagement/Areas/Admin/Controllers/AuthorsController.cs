using BookSellingManagement.Models;
using BookSellingManagement.Models.Authors;
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
    public class AuthorsController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public AuthorsController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index(int pg = 1)
        {

            List<AuthorModel> author = _dataContext.Authors.ToList();

            const int pageSize = 10;

            if (pg < 1)
            {
                pg = 1;
            }
            int recsCount = author.Count();

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize;

            var data = author.Skip(recSkip).Take(pager.PageSize).ToList();

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
        public async Task<IActionResult> Create(AuthorModel author)
        {
            if (ModelState.IsValid)
            {
                // Tạo AuthorSlug từ AuthorName
                author.AuthorSlug = author.AuthorName.Replace(" ", "-");

                // Kiểm tra slug có trùng không
                var slug = await _dataContext.Authors.FirstOrDefaultAsync(b => b.AuthorSlug == author.AuthorSlug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Tác giả đã có.");
                    return View(author);
                }

                // Tạo AuthorId mới bằng GUID
                author.AuthorId = Guid.NewGuid().ToString();

                // Tạo CategoryCode với định dạng TL + năm + số thứ tự
                string yearPrefix = "TG" + DateTime.Now.Year.ToString(); // TL2024
                var latestAuthor = await _dataContext.Authors
                                                        .Where(a => a.AuthorCode.StartsWith(yearPrefix)) // Lọc thể loại trong năm hiện tại
                                                        .OrderByDescending(c => c.AuthorCode) 
                                                        .FirstOrDefaultAsync();

                int codeNumber = 1; 
                if (latestAuthor != null)
                {
                    string latestCode = latestAuthor.AuthorCode.Substring(yearPrefix.Length); // Lấy phần sau TL2024
                    codeNumber = int.Parse(latestCode) + 1; 
                }

                author.AuthorCode = yearPrefix + codeNumber.ToString("D3");

                // Xử lý hình ảnh (nếu có)
                if (author.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    string imageName = Guid.NewGuid() + "-" + author.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        await author.ImageUpload.CopyToAsync(fs);
                    }

                    author.Image = imageName;
                }

                // Lưu tác giả vào cơ sở dữ liệu
                _dataContext.Add(author);
                await _dataContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm tác giả thành công.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = "Model đang bị lỗi. Vui lòng kiểm tra lại.";
                return View(author);
            }
        }

        public async Task<IActionResult> Edit(string AuthorId)
        {
            AuthorModel author = await _dataContext.Authors.FirstOrDefaultAsync(a => a.AuthorId == AuthorId);
            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AuthorModel author)
        {


            if (ModelState.IsValid)
            {
                // Tải sách từ cơ sở dữ liệu
                var existingBook = await _dataContext.Authors.AsNoTracking().FirstOrDefaultAsync(b => b.AuthorId == author.AuthorId);
                if (existingBook == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tác giả cần cập nhật.";
                    return RedirectToAction("Index");
                }

                // Tạo slug cho thể loại
                author.AuthorSlug = author.AuthorName.Replace(" ", "-");

                // Kiểm tra slug có trùng hay không
                var slug = await _dataContext.Authors
                                              .FirstOrDefaultAsync(c => c.AuthorSlug == author.AuthorSlug && c.AuthorId != author.AuthorId);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Tác giả đã có.");
                    return View(author);
                }

                // Xử lý hình ảnh (nếu có)
                if (author.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    string imageName = Guid.NewGuid() + "-" + author.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    // Xóa ảnh cũ nếu không phải "noimage.jpg"
                    if (!string.Equals(existingBook.Image, "noimage.jpg", StringComparison.OrdinalIgnoreCase))
                    {
                        string oldImagePath = Path.Combine(uploadsDir, existingBook.Image);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Lưu ảnh mới
                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        await author.ImageUpload.CopyToAsync(fs);
                    }
                    existingBook.Image = imageName;
                }

         
                _dataContext.Update(existingBook);
                await _dataContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật thông tin sách thành công.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Model đang bị lỗi. Vui lòng kiểm tra lại.";
            return View(author);
        }


        public async Task<IActionResult> Delete(string AuthorId)
        {
            AuthorModel author = await _dataContext.Authors.FindAsync(AuthorId);
            if (author == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tác giả cần xóa.";
                return RedirectToAction("Index");
            }

            // Xóa ảnh cũ nếu không phải là "noimage.jpg"
            if (!string.Equals(author.Image, "noimage.jpg", StringComparison.OrdinalIgnoreCase))
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                string oldFileImage = Path.Combine(uploadsDir, author.Image);

                if (System.IO.File.Exists(oldFileImage))
                {
                    System.IO.File.Delete(oldFileImage); // Thực hiện xóa file ảnh
                }
            }


            _dataContext.Authors.Remove(author);
            await _dataContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa tác giả thành công.";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Details(string AuthorId)
        {
            AuthorModel author = await _dataContext.Authors.FirstOrDefaultAsync(a => a.AuthorId == AuthorId);
            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }
    }
}
