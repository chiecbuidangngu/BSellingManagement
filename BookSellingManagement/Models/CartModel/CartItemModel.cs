using System.ComponentModel.DataAnnotations.Schema;
using BookSellingManagement.Models.Book;
namespace BookSellingManagement.Models
{
    public class CartItemModel
    {
       
        public string BookId { get; set; } = null!;
        public string BookCode { get; set; } = null!;
        public string BookName { get; set; } = null!;
      

        public int Price { get; set; }
       
        public int Quantity { get; set; }

        public int Amount
        {
            get
            {
                return Quantity * Price;
            }
        }
        public string Image { set; get; } = null!;
        public CartItemModel()
        {
        }
            public CartItemModel(BookModel book)
        {
            BookId = book.BookId;
            BookCode = book.BookCode;
            BookName = book.BookName;
            Price = book.Price;
            Quantity = 1;
            Image = book.Image;
        }
    }
}
