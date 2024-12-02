using BookSellingManagement.Models;
using BookSellingManagement.Models.Book;
using BookSellingManagement.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookSellingManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Quản trị viên, Nhân viên")]
    public class BooksController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public BooksController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index(int pg = 1, string search = "")
        {
            const int pageSize = 10;

            if (pg < 1)
            {
                pg = 1;
            }

            // Lấy danh sách sách từ cơ sở dữ liệu, bao gồm thông tin thể loại và tác giả
            var books = _dataContext.Books
                .Include(b => b.Category)
                .Include(b => b.Author)
                .AsQueryable();

            // Áp dụng bộ lọc tìm kiếm nếu có từ khóa
            if (!string.IsNullOrEmpty(search))
            {
                books = books.Where(b => b.BookName.Contains(search) ||
                                         b.Category.CategoryName.Contains(search) ||
                                         b.Author.AuthorName.Contains(search));
                ViewBag.Search = search; // Để hiển thị lại từ khóa trên giao diện
            }

            // Tính tổng số bản ghi sau khi lọc
            int recsCount = await books.CountAsync();
            int recSkip = (pg - 1) * pageSize;

            // Phân trang
            var data = await books
                .OrderBy(b => b.BookId)
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
            // Gán ViewBag.Categories với SelectList để Razor nhận diện đúng kiểu dữ liệu
            ViewBag.Categories = new SelectList(_dataContext.Categories, "CategoryId", "CategoryName");
            ViewBag.Authors = new SelectList(_dataContext.Authors, "AuthorId", "AuthorName");

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookModel book)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "CategoryId", "CategoryName", book.CategoryId);
            ViewBag.Authors = new SelectList(_dataContext.Authors, "AuthorId", "AuthorName", book.AuthorId);

            if (ModelState.IsValid)
            {
              
                book.BookId = Guid.NewGuid().ToString();

                // Tạo BookCode với định dạng HT + năm + số thứ tự
                string yearPrefix = "HT" + DateTime.Now.Year.ToString(); // HT2024
                var latestBook = await _dataContext.Books
                                                   .Where(b => b.BookCode.StartsWith(yearPrefix)) // Lọc sách cùng năm
                                                   .OrderByDescending(b => b.BookCode) // Sắp xếp giảm dần
                                                   .FirstOrDefaultAsync();

                int codeNumber = 1; // Số thứ tự mặc định là 1
                if (latestBook != null)
                {
                    string latestCode = latestBook.BookCode.Substring(yearPrefix.Length); // Lấy phần số sau HT2024
                    codeNumber = int.Parse(latestCode) + 1; // Tăng số thứ tự lên
                }

                book.BookCode = yearPrefix + codeNumber.ToString("D3"); // Định dạng HT2024001, HT2024002,...

                // Tạo slug cho sách
                book.BookSlug = book.BookName.Replace(" ", "-");

                // Kiểm tra slug có trùng hay không
                var slug = await _dataContext.Books.FirstOrDefaultAsync(b => b.BookSlug == book.BookSlug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Sách đã có.");
                    return View(book);
                }

                // Xử lý hình ảnh
                if (book.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    string imageName = Guid.NewGuid() + "-" + book.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await book.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    book.Image = imageName;
                }

            
                _dataContext.Add(book);
                await _dataContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm sách thành công.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Model đang bị lỗi. Vui lòng kiểm tra lại.";
            return View(book);
        }
        public async Task<IActionResult> Edit(string BookId)
        {
            BookModel book = await _dataContext.Books.FirstOrDefaultAsync(b => b.BookId == BookId);
            if (book == null)
            {
                return NotFound();
            }
            // Gán ViewBag.Categories và ViewBag.Authors với SelectList để Razor nhận diện đúng kiểu dữ liệu
            ViewBag.Categories = new SelectList(_dataContext.Categories, "CategoryId", "CategoryName", book.CategoryId);
            ViewBag.Authors = new SelectList(_dataContext.Authors, "AuthorId", "AuthorName", book.AuthorId);

            return View(book);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookModel book)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "CategoryId", "CategoryName", book.CategoryId);
            ViewBag.Authors = new SelectList(_dataContext.Authors, "AuthorId", "AuthorName", book.AuthorId);

            if (ModelState.IsValid)
            {
                // Tải sách từ cơ sở dữ liệu
                var existingBook = await _dataContext.Books.AsNoTracking().FirstOrDefaultAsync(b => b.BookId == book.BookId);
                if (existingBook == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sách cần cập nhật.";
                    return RedirectToAction("Index");
                }

                // Cập nhật các trường cần thiết
                existingBook.BookName = book.BookName;
                existingBook.CategoryId = book.CategoryId;
                existingBook.AuthorId = book.AuthorId;
                existingBook.Price = book.Price;
                existingBook.BookSlug = book.BookName.Replace(" ", "-");

                // Kiểm tra slug có trùng không
                var slugExists = await _dataContext.Books
                    .AnyAsync(b => b.BookSlug == existingBook.BookSlug && b.BookId != existingBook.BookId);
                if (slugExists)
                {
                    ModelState.AddModelError("", "Slug của sách đã tồn tại.");
                    return View(book);
                }

                // Xử lý hình ảnh (nếu có)
                if (book.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    string imageName = Guid.NewGuid() + "-" + book.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await book.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    book.Image = imageName;

                  
                }

       
                _dataContext.Update(existingBook);
                await _dataContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật thông tin sách thành công.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Model đang bị lỗi. Vui lòng kiểm tra lại.";
            return View(book);
        }

        public async Task<IActionResult> Delete(string BookId)
        {
            // Tìm sách trong cơ sở dữ liệu
            BookModel book = await _dataContext.Books.FindAsync(BookId);

            if (book == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sách cần xóa.";
                return RedirectToAction("Index");
            }

            // Xóa ảnh cũ nếu không phải là "noimage.jpg"
            if (!string.Equals(book.Image, "noimage.jpg", StringComparison.OrdinalIgnoreCase))
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                string oldfileImage = Path.Combine(uploadsDir, book.Image);

                if (System.IO.File.Exists(oldfileImage))
                {
                    System.IO.File.Delete(oldfileImage); // Thực hiện xóa file ảnh
                }
            }

      
            _dataContext.Books.Remove(book);
            await _dataContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa sách thành công.";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Details(string BookId)
        {
            var book = await _dataContext.Books.FirstOrDefaultAsync(b => b.BookId == BookId);
            if (book == null)
            {
                return NotFound();
            }


            ViewBag.Categories = new SelectList(_dataContext.Categories, "CategoryId", "CategoryName", book.CategoryId);
            ViewBag.Authors = new SelectList(_dataContext.Authors, "AuthorId", "AuthorName", book.AuthorId);

            return View(book);
        }

    }
}

