
using BookSellingManagement.Areas.Admin.Reponsitory;
using BookSellingManagement.Models;
using BookSellingManagement.Models.OrderModel;
using BookSellingManagement.Models.ViewModels;
using BookSellingManagement.Reponsitory;
using BookSellingManagement.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
public class CheckoutController : Controller
{
    private readonly DataContext _dataContext;
    private readonly IEmailSender _emailSender;
    public CheckoutController(DataContext context, IEmailSender emailSender)
    {
        _dataContext = context;
        _emailSender = emailSender;
    }
    public IActionResult Index()
    {
        List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
        CartItemViewModel cartVM = new()
        {
            CartItems = cartItems,
            GrandTotal = cartItems.Sum(x => x.Quantity * x.Price)
        };

        return View(cartVM);
    }
    public async Task<IActionResult> Checkout(string addressDetail, string phoneNumber, string fullName, string paymentMethod)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        if (userEmail == null)
        {
            return RedirectToAction("Login", "Account");
        }
        else
        {
            // Kiểm tra nếu không có giá trị nhập vào từ form
            if (string.IsNullOrEmpty(addressDetail) || string.IsNullOrEmpty(phoneNumber))
            {
                TempData["ErrorMessage"] = "Địa chỉ và số điện thoại không thể để trống.";
                return RedirectToAction("Index");
            }

            // Tạo OrderCode với tiền tố DH + năm + số thứ tự
            string yearPrefix = "DH" + DateTime.Now.Year.ToString(); 
            var latestOrder = await _dataContext.Orders
                                                .Where(o => o.OrderCode.StartsWith(yearPrefix))
                                                .OrderByDescending(o => o.OrderCode)
                                                .FirstOrDefaultAsync();

            int codeNumber = 1; 
            if (latestOrder != null)
            {
                string latestCode = latestOrder.OrderCode.Substring(yearPrefix.Length);
                codeNumber = int.Parse(latestCode) + 1;
            }

            string orderCode = yearPrefix + codeNumber.ToString("D3"); // DH2024 + số thứ tự định dạng 3 chữ số

            // Lấy danh sách giỏ hàng từ session
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            // Kiểm tra giỏ hàng trước khi tạo đơn hàng
            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn không có sản phẩm.";
                return RedirectToAction("Index");
            }

            int totalAmount = cartItems.Sum(item => item.Amount);

            // Tạo đơn hàng
            var orderItem = new OrderModel
            {
                OrderId = Guid.NewGuid().ToString(),
                OrderCode = orderCode,
                Username = userEmail,
                Status = paymentMethod == "COD" ? 1 : 2, // Nếu COD thì trạng thái = 1, nếu VNPAY thì trạng thái = 2
                CreateDate = DateTime.Now,
                TotalAmount = totalAmount,
                PhoneNumber = phoneNumber, // Lấy từ form
                Address = addressDetail,
                FullName = fullName,
                PaymentMethod = paymentMethod // Lấy từ form
            };

            _dataContext.Add(orderItem);
            await _dataContext.SaveChangesAsync();

            // Tạo chi tiết đơn hàng từ giỏ hàng và cập nhật SoldQuantity
            foreach (var cart in cartItems)
            {
              
                var orderDetails = new OrderDetailModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = userEmail,
                    OrderCode = orderCode,
                    BookId = cart.BookId,
                    Price = cart.Price,
                    Quantity = cart.Quantity
                };
                _dataContext.Add(orderDetails);

                // Cập nhật SoldQuantity trong bảng sách
                var book = await _dataContext.Books.FirstOrDefaultAsync(b => b.BookId == cart.BookId);
                if (book != null)
                {
                    book.SoldQuantity += cart.Quantity; 
                    _dataContext.Books.Update(book);
                }
            }

            await _dataContext.SaveChangesAsync();
            // Gửi email xác nhận đơn hàng cho người dùng
            string subject = "Xác nhận đơn hàng #" + orderCode;
            string body = $"Chào {fullName},\n\n" +
                          $"Cảm ơn bạn đã đặt hàng tại cửa hàng chúng tôi. Đơn hàng của bạn đã được tiếp nhận với mã đơn hàng: {orderCode}. " +
                          $"Dưới đây là thông tin đơn hàng của bạn:\n\n" +
                          $"Địa chỉ: {addressDetail}\n" +
                          $"Số điện thoại: {phoneNumber}\n" +
                          $"Tổng tiền: {totalAmount} VNĐ\n\n" +
                          $"Chúng tôi sẽ thông báo cho bạn khi đơn hàng được xử lý.\n\n" +
                          "Trân trọng,\n HuTa Book";

           
            await _emailSender.SendEmailAsync(userEmail, subject, body);
           
            HttpContext.Session.Remove("Cart");

            if (paymentMethod == "COD")
            {
                return View("Success");
            }
            else
            {
                return RedirectToAction("VNPayPayment", "Checkout");
            }
        }
    }
    public IActionResult VNPayPayment()
    {
        return View(); 
    }
}
