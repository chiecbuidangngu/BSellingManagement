using System.ComponentModel.DataAnnotations.Schema;

namespace BookSellingManagement.Models.ViewModels
{
    public class CartItemViewModel
    {
        public List<CartItemModel> CartItems { get; set; }

        public int GrandTotal {  get; set; }
    }
}
