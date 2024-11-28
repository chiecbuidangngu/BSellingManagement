using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace BookSellingManagement.Models.OrderModel
{
    public class OrderModel
    {
        [Key]
        public string OrderId { get; set; }
        public string OrderCode { get; set; }
        public string Username { get; set; }
        public DateTime CreateDate { get; set; }
        public int Status { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }

        public string PaymentMethod { get; set; }
        public int TotalAmount { get; set; }


    }
}

