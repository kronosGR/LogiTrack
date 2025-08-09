using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogiTrack.Models
{
    /// <summary>
    /// Represents an item in an order.
    /// </summary>
    public class OrderItem
    {
        /// <summary>
        /// The unique identifier of the order item.
        /// </summary>
        [Key]
        public int OrderItemId { get; set; }

        /// <summary>
        /// The ID of the order this item belongs to.
        /// </summary>
        [ForeignKey("Order")]
        public int OrderId { get; set; }

        /// <summary>
        /// The order this item belongs to.
        /// </summary>
        public Order Order { get; set; }

        /// <summary>
        /// The ID of the inventory item.
        /// </summary>
        [ForeignKey("InventoryItem")]
        public int ItemId { get; set; }

        /// <summary>
        /// The inventory item.
        /// </summary>
        public InventoryItem InventoryItem { get; set; }
    }
}