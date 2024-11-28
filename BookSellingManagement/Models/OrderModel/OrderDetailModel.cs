using System.ComponentModel.DataAnnotations.Schema;

namespace BookSellingManagement.Models.OrderModel
{
    public class OrderDetailModel
    {
        public  string Id{ get; set; }
        public string Username { get; set; }
        public string OrderCode { get; set; }
        public string BookId { get; set; }
    
        public int Price { get; set; }
        public int Quantity {  get; set; }


    }
}
