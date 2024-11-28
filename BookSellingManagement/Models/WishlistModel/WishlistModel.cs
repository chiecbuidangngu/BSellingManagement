using BookSellingManagement.Models.Book;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookSellingManagement.Models.OrderModel
{
    public class WishlistModel
    {
        [Key]
        public string WishlistId { get; set; }
        public string BookId { get; set; }
        public string UserId { get; set; }

        [ForeignKey ("BookId")]
        public BookModel Book { get; set; }

    }
}

