using BookSellingManagement.Models;
using BookSellingManagement.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookSellingManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Quản trị viên, Nhân viên")]
    public class WarehouseController : Controller
    {
        private readonly DataContext _dataContext;
        public WarehouseController(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Index(int pg = 1)
        {
            const int pageSize = 10; // Số lượng sách mỗi trang

            if (pg < 1)
            {
                pg = 1;
            }

            // Lấy tất cả sách từ cơ sở dữ liệu và sắp xếp theo BookId
            var books = await _dataContext.Books.OrderBy(b => b.BookId).ToListAsync();

            // Duyệt qua tất cả các sách và tính số lượng đã bán cho mỗi sách
            foreach (var book in books)
            {
                // Tính tổng số lượng đã bán của sách này từ bảng OrderDetails,
                // chỉ lấy các đơn hàng có trạng thái khác 3 (không bị hủy)
                var soldQuantity = await _dataContext.OrderDetails
                                                     .Where(od => od.BookId == book.BookId &&
                                                                  _dataContext.Orders
                                                                              .Any(o => o.OrderCode == od.OrderCode && o.Status != 3))
                                                     .SumAsync(od => od.Quantity);

                // Cập nhật giá trị số lượng đã bán vào cơ sở dữ liệu
                book.SoldQuantity = soldQuantity;
            }

            await _dataContext.SaveChangesAsync();

            // Sắp xếp sách theo RemainingQuantity
            var sortedBooks = books.OrderBy(b => b.RemainingQuantity).ToList();

            // Phân trang
            int recsCount = sortedBooks.Count();
            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize;
            var pagedBooks = sortedBooks.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return View(pagedBooks);
        }


        [HttpPost]
        public IActionResult AddQuantity(string BookId, int quantity)
        {
            // Tìm sách trong kho bằng BookId
            var book = _dataContext.Books.FirstOrDefault(b => b.BookId == BookId);

            if (book != null)
            {
                // Cộng thêm số lượng nhập vào ImportedQuantity
                book.ImportedQuantity += quantity;
                _dataContext.SaveChanges();

                TempData["SuccessMessage"] = "Số lượng sách đã được thêm thành công!";
                return RedirectToAction("Index"); 
            }

            TempData["ErrorMessage"] = "Không tìm thấy sách!";
            return RedirectToAction("Index"); 
        }


    }
}
