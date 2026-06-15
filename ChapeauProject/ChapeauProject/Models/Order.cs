using System;
using System.Collections.Generic;

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
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public void AddItem(MenuItem item)
        {
            OrderItem existing = null;

            foreach (OrderItem oi in OrderItems)
            {
                if (oi.MenuItem != null && oi.MenuItem.MenuItemID == item.MenuItemID)
                {
                    existing = oi;
                    break;
                }
            }

            if (existing != null)
            {
                existing.Quantity++;
            }
            else
            {
                OrderItems.Add(new OrderItem
                {
                    MenuItem = item,
                    Quantity = 1,
                    PreparationStatus = PreparationStatus.Pending,
                    Comment = string.Empty
                });
            }
        }

        public void RemoveItem(int menuItemID)
        {
            OrderItem item = null;

            foreach (OrderItem oi in OrderItems)
            {
                if (oi.MenuItem != null && oi.MenuItem.MenuItemID == menuItemID)
                {
                    item = oi;
                    break;
                }
            }

            if (item != null)
                OrderItems.Remove(item);
        }

        public void IncreaseQuantity(int menuItemID)
        {
            foreach (OrderItem oi in OrderItems)
            {
                if (oi.MenuItem != null && oi.MenuItem.MenuItemID == menuItemID)
                {
                    oi.Quantity++;
                    break;
                }
            }
        }

        public void DecreaseQuantity(int menuItemID)
        {
            OrderItem item = null;

            foreach (OrderItem oi in OrderItems)
            {
                if (oi.MenuItem != null && oi.MenuItem.MenuItemID == menuItemID)
                {
                    item = oi;
                    break;
                }
            }

            if (item == null)
                return;

            item.Quantity--;

            if (item.Quantity <= 0)
                OrderItems.Remove(item);
        }

        public void UpdateItemComment(int menuItemID, string comment)
        {
            foreach (OrderItem oi in OrderItems)
            {
                if (oi.MenuItem != null && oi.MenuItem.MenuItemID == menuItemID)
                {
                    oi.Comment = comment ?? string.Empty;
                    break;
                }
            }
        }
    }
}