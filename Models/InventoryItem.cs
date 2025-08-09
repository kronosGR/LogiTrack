using System.ComponentModel.DataAnnotations;

namespace LogiTrack.Models
{
    /// <summary>
    /// Represents an inventory item.
    /// </summary>
    public class InventoryItem
    {
        /// <summary>
        /// The unique identifier of the inventory item.
        /// </summary>
        [Key]
        public int ItemId { get; set; }

        /// <summary>
        /// The name of the inventory item.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// The quantity of the inventory item.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// The location of the inventory item.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Displays information about the inventory item.
        /// </summary>
        public void DisplayInfo()
        {
            Console.WriteLine($"Item: {Name} | Quantity: {Quantity} | Location: {Location}");
        }
    }
}