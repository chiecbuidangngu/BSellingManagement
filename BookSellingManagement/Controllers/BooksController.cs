using BookSellingManagement.Models;
using BookSellingManagement.Models.Book;
using BookSellingManagement.Models.OrderModel;
using BookSellingManagement.Reponsitory;
using BookSellingManagement.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BookSellingManagement.Controllers
{
    public class BooksController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<BooksController> _logger;
        private readonly UserManager<AppUserModel> _userManager;

        public BooksController(ILogger<BooksController> logger, DataContext context, UserManager<AppUserModel> userManager)
        {
            _logger = logger;
            _dataContext = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var books = _dataContext.Books
                .Include(b => b.Author) 
                .Include(b => b.Category) 
                .ToList(); // Chuyển đổi thành danh sách

            return View(books); // Truyền danh sách sách vào view
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
      

        public async Task<IActionResult> Details(string BookId = "")
        {
            // Kiểm tra nếu BookId rỗng hoặc null
            if (string.IsNullOrEmpty(BookId))
            {
                return RedirectToAction("Index"); 
            }

            // Tìm cuốn sách dựa trên BookId và bao gồm thông tin Author và Category
            var book = await _dataContext.Books
                                          .Include(b => b.Author)  
                                          .Include(b => b.Category) 
                                          .FirstOrDefaultAsync(b => b.BookId == BookId);

            if (book == null)
            {
                return RedirectToAction("Index"); 
            }

            // Lấy danh sách các sách khác trong cùng thể loại, ngoại trừ cuốn sách hiện tại
            var BooksByCategory = await _dataContext.Books
                                                    .Where(c => c.CategoryId == book.CategoryId && c.BookId != BookId)
                                                    .OrderByDescending(c => c.CategoryId)
                                                    .ToListAsync();

            // Truyền dữ liệu vào ViewBag
            ViewBag.BooksByCategory = BooksByCategory;

            // Truyền dữ liệu sách vào view
            return View(book);
        }
        // Phương thức Error với mã trạng thái
        public IActionResult Error(int statusCode)
        {
            if (statusCode == 404)
            {
                return PartialView("NotFound"); 
            }
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        




    }

}
