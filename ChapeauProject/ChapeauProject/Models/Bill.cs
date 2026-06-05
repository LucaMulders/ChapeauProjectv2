using System;

namespace ChapeauProject.Models
{
    public class Bill
    {
        public int BillID { get; set; }
        public Order? Order { get; set; }
        public Guest? Guest { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal SubTotalAmount { get; set; }
        public DateTime BillTimeStamp { get; set; }
    }
}
