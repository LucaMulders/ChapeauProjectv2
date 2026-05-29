using System;
using System.Collections.Generic;

namespace ChapeauProject.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        //change to table (respectfully I disagree I think tablenumber is better so im not gonna do this :P)
        public int TableNumber { get; set; }
        public int GuestID { get; set; }
        public DateTime OrderTimeStamp { get; set; } = DateTime.Now;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}