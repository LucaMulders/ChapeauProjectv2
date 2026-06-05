using System;
using System.Collections.Generic;

namespace ChapeauProject.Models
{
    //NOTE Order is missing a Staff/Employee object
    //NOTE Order is missing behavior methods
    public class Order
    {
        public int OrderID { get; set; }
        public Table Table { get; set; } = new Table();
        public Guest Guest { get; set; } = new Guest();
        public DateTime OrderTimeStamp { get; set; } = DateTime.Now;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}