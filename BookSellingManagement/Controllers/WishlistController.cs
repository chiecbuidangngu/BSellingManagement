using Microsoft.AspNetCore.Mvc;
using BookSellingManagement.Models;
using BookSellingManagement.Models.OrderModel;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using BookSellingManagement.Repository;
using Microsoft.EntityFrameworkCore;

public class WishlistController : Controller
{
    private readonly DataContext _dataContext;
    private readonly UserManager<AppUserModel> _userManager;

    public WishlistController(DataContext context, UserManager<AppUserModel> userManager)
    {
        _dataContext = context;  
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User); // Lấy ID người dùng hiện tại
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "Bạn phải đăng nhập để xem danh sách yêu thích.";
            return RedirectToAction("Login", "Account");
        }

        // Truy vấn danh sách yêu thích từ database
        var wishlistBooks = await (from w in _dataContext.Wishlists
                                   join b in _dataContext.Books on w.BookId equals b.BookId
                                   where w.UserId == userId
                                   select b).ToListAsync();

        return View(wishlistBooks);
    }




    [HttpPost]
    public async Task<IActionResult> AddToWishlist(string bookId)
    {
        if (string.IsNullOrEmpty(bookId))
        {
            return Json(new { success = false, message = "Sách không hợp lệ." });
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Bạn phải đăng nhập để thêm sách vào danh sách yêu thích." });
        }

        var existingWishlist = await _dataContext.Wishlists
            .FirstOrDefaultAsync(w => w.BookId == bookId && w.UserId == userId);

        if (existingWishlist != null)
        {
            return Json(new { success = false, message = "Sách đã có trong danh sách yêu thích." });
        }

        var wishlistItem = new WishlistModel
        {
            WishlistId = Guid.NewGuid().ToString(),
            BookId = bookId,
            UserId = userId
        };

        _dataContext.Wishlists.Add(wishlistItem);
        await _dataContext.SaveChangesAsync();

        return Json(new { success = true, message = "Sách đã được thêm vào danh sách yêu thích." });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveFromWishlist(string bookId)
    {
        var userId = _userManager.GetUserId(User);

        var wishlistItem = await _dataContext.Wishlists
            .FirstOrDefaultAsync(w => w.BookId == bookId && w.UserId == userId);

        if (wishlistItem != null)
        {
            _dataContext.Wishlists.Remove(wishlistItem);
            await _dataContext.SaveChangesAsync();
            return Json(new { success = true, message = "Sách đã được xóa khỏi danh sách yêu thích." });
        }
        else
        {
            return Json(new { success = false, message = "Không tìm thấy sách trong danh sách yêu thích." });
        }
    }





}
