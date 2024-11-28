using BookSellingManagement.Migrations;
using BookSellingManagement.Models;
using BookSellingManagement.Models.Book;
using BookSellingManagement.Models.ViewModels;
using BookSellingManagement.Reponsitory;
using BookSellingManagement.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookSellingManagement.Controllers
{

    public class CartController : Controller
    {
        private readonly DataContext _dataContext;

        public CartController(DataContext _context)
        {
            _dataContext = _context;
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

        public async Task<IActionResult> Add(string BookId)
        {
            BookModel book = await _dataContext.Books.FindAsync(BookId);
            if (book == null) return NotFound();

            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            CartItemModel cartItem = cart.FirstOrDefault(c => c.BookId == BookId);

            if (cartItem == null)
            {
                cart.Add(new CartItemModel(book));
            }
            else
            {
                cartItem.Quantity++;
            }

            HttpContext.Session.SetJson("Cart", cart);

            return Json(new { success = true, message = "Thêm vào giỏ hàng thành công." });

        }
      
        public IActionResult Decrease(string BookId)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            CartItemModel cartItem = cart.FirstOrDefault(c => c.BookId == BookId);

            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;
                }
                else
                {
                    cart.Remove(cartItem); // Xóa nếu số lượng là 1
                }
            }

            UpdateCartSession(cart);
            return RedirectToAction("Index");
        }

        public IActionResult Increase(string BookId)
        {
            // Get the current cart from session
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            // Find the cart item for the specified BookId
            CartItemModel cartItem = cart.FirstOrDefault(c => c.BookId == BookId);

            if (cartItem != null)
            {
                // Get the book's remaining quantity from the database
                var book = _dataContext.Books.FirstOrDefault(b => b.BookId == BookId);

                if (book != null)
                {
                    // Check if the quantity in the cart exceeds the remaining stock
                    if (cartItem.Quantity < book.RemainingQuantity)
                    {
                   
                        cartItem.Quantity++;
                    }
                    else
                    {
                  
                        TempData["ErrorMessage"] = "Số lượng sách trong kho không đủ.";
                    }
                }
                else
                {
             
                    TempData["ErrorMessage"] = "Không tìm thấy sách.";
                }
            }
 
            UpdateCartSession(cart);
            return RedirectToAction("Index");
        }


        public IActionResult Remove(string BookId)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            cart.RemoveAll(b => b.BookId == BookId);

            UpdateCartSession(cart);
            return RedirectToAction("Index");
        }

      

        private void UpdateCartSession(List<CartItemModel> cart)
        {
            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
            }
        }
    }
}
