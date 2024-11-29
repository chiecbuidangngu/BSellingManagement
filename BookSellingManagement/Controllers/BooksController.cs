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

        public async Task<IActionResult> Index(int pg = 1)
        {
            // Lấy danh sách sách cùng với các thông tin Author và Category
            var booksQuery = _dataContext.Books
                .Include(b => b.Author)
                .Include(b => b.Category);

            // Chuyển đổi thành danh sách và tính tổng số bản ghi
            List<BookModel> books = await booksQuery.ToListAsync(); // Hoặc sử dụng ToList() nếu không cần await

            const int pageSize = 10;  // Số lượng sách hiển thị trên một trang
            int recsCount = books.Count();  // Tổng số sách

            // Tính toán phân trang
            var pager = new Paginate(recsCount, pg, pageSize);

            // Tính số lượng bản ghi cần bỏ qua
            int recSkip = (pg - 1) * pageSize;

            // Lấy dữ liệu cho trang hiện tại
            var data = books.Skip(recSkip).Take(pager.PageSize).ToList();

            // Truyền dữ liệu phân trang vào ViewBag
            ViewBag.Pager = pager;

            return View(data); // Trả về dữ liệu phân trang vào View
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

        public async Task<IActionResult> Search(string searchTerm)
        {
            var books = await _dataContext.Books.Where(b => b.BookName.Contains(searchTerm) || b.Description.Contains(searchTerm)).ToListAsync();
            ViewBag.Keyword = searchTerm;
            return View(books);
        }
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
