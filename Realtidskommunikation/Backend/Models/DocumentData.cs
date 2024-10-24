using Backend.Models;

namespace Backend.Models
{
    public class DocumentData
    {
        public string OrderNumber { get; set; }
        public string OrderDate { get; set; }
        public Company FromCompany { get; set; }
        public Company ToCompany { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public string PaymentTerms { get; set; }
        public string DeliveryTerms { get; set; }
    }
}
