using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using BookSellingManagement.Repository;
using Microsoft.EntityFrameworkCore;
using BookSellingManagement.Models;

namespace BookSellingManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Quản trị viên, Nhân viên")]
    public class CustomersController : Controller
    {
        private readonly DataContext _dataContext;
        public CustomersController(DataContext context)
        {
            _dataContext = context;
        }

        public IActionResult Index(int pg = 1, string search = "")
        {
            const int pageSize = 10; 

            if (pg < 1)
            {
                pg = 1;
            }

            // Lấy danh sách khách hàng và tính toán các chỉ số
            var customersQuery = (from order in _dataContext.Orders
                                  where order.Status != 3 // Loại trừ các đơn hàng có trạng thái là 3 (đã hủy)
                                  join user in _dataContext.Users on order.Username equals user.Email // Join với bảng Users
                                  join orderDetail in _dataContext.OrderDetails on order.OrderCode equals orderDetail.OrderCode into orderDetails
                                  from detail in orderDetails.DefaultIfEmpty()
                                  group new { order, detail, user } by order.Username into grouped
                                  select new
                                  {
                                      Username = grouped.Key,
                                      UserName = grouped.FirstOrDefault().user.UserName, 
                                      PhoneNumber = grouped.FirstOrDefault().user.PhoneNumber, 
                                      Revenue = grouped.Select(g => g.order.TotalAmount).Distinct().Sum(), 
                                      TotalOrders = grouped.Select(g => g.order.OrderCode).Distinct().Count(),
                                      TotalBooks = grouped.Sum(g => g.detail != null ? g.detail.Quantity : 0) 
                                  });

        
            if (!string.IsNullOrEmpty(search))
            {
                customersQuery = customersQuery.Where(c =>
                    c.UserName.Contains(search) ||
                    c.PhoneNumber.Contains(search));

                ViewBag.Search = search; 
            }

            // Tổng số khách hàng sau khi lọc
            int recsCount = customersQuery.Count();

        
            var pager = new Paginate(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;

          
            var customers = customersQuery
                .OrderBy(c => c.Username)
                .Skip(recSkip)
                .Take(pager.PageSize)
                .ToList();

            // Gửi thông tin phân trang vào ViewBag
            ViewBag.Pager = pager;

            return View(customers);
        }
        public IActionResult Orders(string username)
        {
            var orders = (from order in _dataContext.Orders
                          where order.Username == username && order.Status != 3 
                          select new
                          {
                              OrderCode = order.OrderCode,
                              CreateDate = order.CreateDate,
                              Status = order.Status,
                              TotalAmount = order.TotalAmount
                          }).ToList();

            return View(orders); 
        }

       public IActionResult Books(string username)
{
    // Lấy danh sách sách đã mua
    var books = (from order in _dataContext.Orders
                 where order.Username == username && order.Status != 3
                 join orderDetail in _dataContext.OrderDetails on order.OrderCode equals orderDetail.OrderCode into orderDetails
                 from detail in orderDetails.DefaultIfEmpty()
                 join book in _dataContext.Books on detail.BookId equals book.BookId
                 join category in _dataContext.Categories on book.CategoryId equals category.CategoryId
                 where detail != null
                 group new { book, detail, category } by new
                 {
                     book.BookCode,
                     book.BookName,
                     book.Image,
                     category.CategoryId,
                     category.CategoryName // Tên thể loại
                 } into groupedBooks
                 select new
                 {
                     BookCode = groupedBooks.Key.BookCode,
                     BookName = groupedBooks.Key.BookName,
                     CategoryId = groupedBooks.Key.CategoryId,
                     CategoryName = groupedBooks.Key.CategoryName,
                     Quantity = groupedBooks.Sum(g => g.detail.Quantity),
                     Image = groupedBooks.Key.Image
                 }).ToList();

    // Xác định danh sách thể loại yêu thích (nhiều thể loại nếu có tổng số lượng bằng nhau)
    var favoriteGenres = books
        .GroupBy(b => new { b.CategoryId, b.CategoryName })
        .Select(g => new
        {
            CategoryId = g.Key.CategoryId,
            CategoryName = g.Key.CategoryName,
            TotalQuantity = g.Sum(b => b.Quantity)
        })
        .ToList();

    // Tìm tổng số lượng lớn nhất
    var maxQuantity = favoriteGenres.Max(g => g.TotalQuantity);

    // Lọc ra các thể loại có tổng số lượng bằng với tổng lớn nhất
    var topGenres = favoriteGenres
        .Where(g => g.TotalQuantity == maxQuantity)
        .ToList();

    // Truyền dữ liệu vào View
    ViewBag.FavoriteGenres = topGenres; // Lưu danh sách thể loại yêu thích vào ViewBag
    return View(books); // Truyền danh sách sách vào View
}



    }
}
