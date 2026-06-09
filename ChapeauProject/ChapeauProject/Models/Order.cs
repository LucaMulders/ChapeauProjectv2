using System;
using System.Collections.Generic;
using System.Linq;

namespace ChapeauProject.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public Table Table { get; set; } = new Table();
        public Guest Guest { get; set; } = new Guest();
        public Staff Staff { get; set; } = new Staff();
        public DateTime OrderTimeStamp { get; set; } = DateTime.Now;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        private readonly List<OrderItem> _orderItems = new List<OrderItem>();
        public IReadOnlyList<OrderItem> OrderItems => _orderItems;

        public decimal CalculateTotalPrice()
        {
            return OrderItems.Sum(oi => (oi.MenuItem?.Price ?? 0) * oi.Quantity);
        }

        public void AddItem(MenuItem item)
        {
            var existing = OrderItems.FirstOrDefault(oi => oi.MenuItem?.MenuItemID == item.MenuItemID);
            if (existing != null)
            {
                existing.Quantity++;
            }
            else
            {
                _orderItems.Add(new OrderItem
                {
                    MenuItem          = item,
                    Quantity          = 1,
                    PreparationStatus = PreparationStatus.Pending,
                    Comment           = string.Empty
                });
            }
        }

        public void RemoveItem(int menuItemID)
        {
            var item = OrderItems.FirstOrDefault(oi => oi.MenuItem?.MenuItemID == menuItemID);
            if (item != null)
                _orderItems.Remove(item);
        }

        public void IncreaseQuantity(int menuItemID)
        {
            var item = OrderItems.FirstOrDefault(oi => oi.MenuItem?.MenuItemID == menuItemID);
            if (item != null)
                item.Quantity++;
        }

        public void DecreaseQuantity(int menuItemID)
        {
            var item = OrderItems.FirstOrDefault(oi => oi.MenuItem?.MenuItemID == menuItemID);
            if (item == null) return;

            item.Quantity--;
            if (item.Quantity <= 0)
                _orderItems.Remove(item);
        }

        public void UpdateItemComment(int menuItemID, string comment)
        {
            var item = OrderItems.FirstOrDefault(oi => oi.MenuItem?.MenuItemID == menuItemID);
            if (item != null)
                item.Comment = comment ?? string.Empty;
        }
    }
}
