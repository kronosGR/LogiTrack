using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LogiTrack.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        [Required]
        public string CustomerName { get; set; }
        public DateTime DatePlaced { get; set; }
        public List<OrderItem> OrderItems { get; set; }

        public Order()
        {
            OrderItems = new List<OrderItem>();
        }

        public void AddItem(InventoryItem item)
        {
            OrderItems.Add(new OrderItem { InventoryItem = item, Order = this });
        }

        public void RemoveItem(int itemId)
        {
            var itemToRemove = OrderItems.Find(i => i.ItemId == itemId);
            if (itemToRemove != null)
            {
                OrderItems.Remove(itemToRemove);
            }
        }

        public string GetOrderSummary()
        {
            return $"Order #{OrderId} for {CustomerName} | Items: {OrderItems.Count} | Placed: {DatePlaced.ToString("MM/dd/yyyy")}";
        }
    }
}