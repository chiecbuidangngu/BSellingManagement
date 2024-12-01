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
            const int pageSize = 10; // Số lượng khách hàng mỗi trang

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
                                      UserName = grouped.FirstOrDefault().user.UserName, // Lấy tên người dùng đầu tiên trong nhóm
                                      PhoneNumber = grouped.FirstOrDefault().user.PhoneNumber, // Lấy số điện thoại người dùng đầu tiên trong nhóm
                                      Revenue = grouped.Select(g => g.order.TotalAmount).Distinct().Sum(), // Chỉ tính TotalAmount một lần cho mỗi đơn hàng
                                      TotalOrders = grouped.Select(g => g.order.OrderCode).Distinct().Count(), // Đếm số đơn hàng duy nhất
                                      TotalBooks = grouped.Sum(g => g.detail != null ? g.detail.Quantity : 0) // Tổng số sách trong các chi tiết đơn hàng
                                  });

            // Áp dụng bộ lọc tìm kiếm nếu có từ khóa
            if (!string.IsNullOrEmpty(search))
            {
                customersQuery = customersQuery.Where(c =>
                    c.UserName.Contains(search) ||
                    c.PhoneNumber.Contains(search));

                ViewBag.Search = search; // Lưu từ khóa tìm kiếm để hiển thị lại trên giao diện
            }

            // Tổng số khách hàng sau khi lọc
            int recsCount = customersQuery.Count();

            // Tính toán phân trang
            var pager = new Paginate(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;

            // Lấy dữ liệu phân trang
            var customers = customersQuery
                .OrderBy(c => c.Username) // Sắp xếp theo `Username` hoặc cột khác nếu cần
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
                          where order.Username == username && order.Status != 3 // Lọc các đơn hàng không bị hủy
                          select new
                          {
                              OrderCode = order.OrderCode,
                              CreateDate = order.CreateDate,
                              Status = order.Status,
                              TotalAmount = order.TotalAmount
                          }).ToList();

            return View(orders); // Trả về View hoặc PartialView
        }


        public IActionResult Books(string username)
        {
            var books = (from order in _dataContext.Orders
                         where order.Username == username && order.Status != 3 // Lọc đơn hàng không bị hủy
                         join orderDetail in _dataContext.OrderDetails on order.OrderCode equals orderDetail.OrderCode into orderDetails
                         from detail in orderDetails.DefaultIfEmpty()
                         join book in _dataContext.Books on detail.BookId equals book.BookId // Join với bảng Books để lấy BookName
                         where detail != null
                         select new
                         {
                             BookCode = book.BookCode,          
                             BookName = book.BookName,      
                             Quantity = detail.Quantity,    
                             Image = book.Image             
                         }).ToList();

            return View(books);
        }









    }
}
