using BookSellingManagement.Models;
using BookSellingManagement.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookSellingManagement.Controllers
{
    public class MyOrderController : Controller
    {
        private readonly DataContext _dataContext;
        public MyOrderController(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Index(int pg = 1)
        {
            const int pageSize = 10;
            if (pg < 1)
            {
                pg = 1;
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var ordersQuery = _dataContext.Orders
                .Where(od => od.Username == userEmail)
                .OrderByDescending(od => od.CreateDate);

            int recsCount = await ordersQuery.CountAsync();
            int recSkip = (pg - 1) * pageSize;

            var orders = await ordersQuery
                .Skip(recSkip)
                .Take(pageSize)
                .ToListAsync();

            var pager = new Paginate(recsCount, pg, pageSize);

            ViewBag.Pager = pager;
            ViewBag.UserEmail = userEmail;

            return View(orders);
        }

        public async Task<IActionResult> CancelOrder(string orderId)
        {
            var order = await _dataContext.Orders
                                           .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == 1) 
            {

                var orderDetails = await _dataContext.OrderDetails
                                                     .Where(od => od.OrderCode == order.OrderCode)
                                                     .ToListAsync();

              
                foreach (var detail in orderDetails)
                {
                    var book = await _dataContext.Books.FirstOrDefaultAsync(b => b.BookId == detail.BookId);
                    if (book != null)
                    {
                        book.SoldQuantity -= detail.Quantity;
                        if (book.SoldQuantity < 0)
                        {
                            book.SoldQuantity = 0; 
                        }
                        _dataContext.Books.Update(book);
                    }
                }

                order.Status = 3;
                await _dataContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đơn hàng đã được hủy thành công.";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ConfirmReceipt(string orderId)
        {
            // Tìm đơn hàng theo orderId
            var order = await _dataContext.Orders
                                           .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }
            if (order.Status == 4) // Đang giao hàng
            {
                // Cập nhật trạng thái sang "Giao thành công"
                order.Status = 5;

                await _dataContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xác nhận đã nhận đơn hàng thành công";
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> OrderDetail(string orderId)
        {
            // Tìm đơn hàng theo OrderId
            var order = await _dataContext.Orders
                                           .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            var orderCode = order.OrderCode;
            var orderDetails = await _dataContext.OrderDetails
                                                  .Where(od => od.OrderCode == orderCode)
                                                  .ToListAsync();

            // Lấy thông tin sách từ bảng Books dựa trên BookId trong OrderDetails
            var books = await _dataContext.Books
                                           .Where(b => orderDetails.Select(od => od.BookId).Contains(b.BookId))
                                           .ToListAsync();

            // Kết nối thông tin chi tiết đơn hàng với thông tin sách, bao gồm cả Image
            var orderItems = orderDetails.Select(od => new
            {
                Book = books.FirstOrDefault(b => b.BookId == od.BookId),
                od.Quantity,
                od.Price
            }).ToList();

            ViewBag.OrderItems = orderItems;
            return View(order);
        }


    }
}
