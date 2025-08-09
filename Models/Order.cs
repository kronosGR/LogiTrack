using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LogiTrack.Models
{
    /// <summary>
    /// Represents an order.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// The unique identifier of the order.
        /// </summary>
        [Key]
        public int OrderId { get; set; }

        /// <summary>
        /// The name of the customer who placed the order.
        /// </summary>
        [Required]
        public string CustomerName { get; set; }

        /// <summary>
        /// The date the order was placed.
        /// </summary>
        public DateTime DatePlaced { get; set; }

        /// <summary>
        /// A list of items in the order.
        /// </summary>
        public List<OrderItem> OrderItems { get; set; }

        /// <summary>
        /// Initializes a new instance of the Order class.
        /// </summary>
        public Order()
        {
            OrderItems = new List<OrderItem>();
        }

        /// <summary>
        /// Adds an item to the order.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void AddItem(InventoryItem item)
        {
            OrderItems.Add(new OrderItem { InventoryItem = item, Order = this });
        }

        /// <summary>
        /// Removes an item from the order.
        /// </summary>
        /// <param name="itemId">The ID of the item to remove.</param>
        public void RemoveItem(int itemId)
        {
            var itemToRemove = OrderItems.Find(i => i.ItemId == itemId);
            if (itemToRemove != null)
            {
                OrderItems.Remove(itemToRemove);
            }
        }

        /// <summary>
        /// Gets a summary of the order.
        /// </summary>
        /// <returns>A string summary of the order.</returns>
        public string GetOrderSummary()
        {
            return $"Order #{OrderId} for {CustomerName} | Items: {OrderItems.Count} | Placed: {DatePlaced.ToString("MM/dd/yyyy")}";
        }
    }
}