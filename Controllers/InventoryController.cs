using Microsoft.AspNetCore.Mvc;
using LogiTrack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace LogiTrack.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly LogiTrackContext _context;
        private readonly IMemoryCache _cache;

        public InventoryController(LogiTrackContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        /// <summary>
        /// Retrieves a list of all inventory items.
        /// </summary>
        /// <returns>A list of inventory items.</returns>
        /// <response code="200">Inventory items retrieved successfully.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> GetInventoryItems()
        {
            if (_cache.TryGetValue("inventoryItems", out var cachedItems))
            {
                return Ok(cachedItems);
            }

            var items = await _context.InventoryItems
           .AsNoTracking()
           .ToListAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(30));

            _cache.Set("inventoryItems", items, cacheEntryOptions);

            return Ok(items);
        }

        /// <summary>
        /// Retrieves an inventory item by its ID.
        /// </summary>
        /// <param name="id">The ID of the inventory item.</param>
        /// <returns>The inventory item with the specified ID.</returns>
        /// <response code="200">Inventory item retrieved successfully.</response>
        /// <response code="404">Inventory item not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInventoryItem(int id)
        {
            var item = await _context.InventoryItems
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.ItemId == id);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        /// <summary>
        /// Adds a new inventory item.
        /// </summary>
        /// <param name="item">The inventory item to add.</param>
        /// <returns>The added inventory item.</returns>
        /// <response code="201">Inventory item added successfully.</response>
        [Authorize(Roles = "Manager")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<InventoryItem>> AddInventoryItem(InventoryItem item)
        {
            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();

            // Clear cache after creating a new item
            _cache.Remove("inventoryItems");

            return CreatedAtAction(nameof(GetInventoryItem), new { id = item.ItemId }, item);

        }

        /// <summary>
        /// Updates an existing inventory item.
        /// </summary>
        /// <param name="id">The ID of the inventory item.</param>
        /// <param name="item">The updated inventory item.</param>
        /// <returns>No content.</returns>
        /// <response code="200">Inventory item updated successfully.</response>
        /// <response code="404">Inventory item not found.</response>
        [Authorize(Roles = "Manager")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateInventoryItem(int id, [FromBody] InventoryItem item)
        {
            var existingItem = await _context.InventoryItems.FindAsync(id);
            if (existingItem == null)
            {
                return NotFound();
            }

            existingItem.Name = item.Name;
            existingItem.Quantity = item.Quantity;

            await _context.SaveChangesAsync();

            // Clear cache after updating an item
            _cache.Remove("inventoryItems");

            return Ok(existingItem);
        }


        /// <summary>
        /// Removes an inventory item by its ID.
        /// </summary>
        /// <param name="id">The ID of the inventory item.</param>
        /// <returns>No content.</returns>
        /// <response code="204">Inventory item removed successfully.</response>
        /// <response code="404">Inventory item not found.</response>
        [Authorize(Roles = "Manager")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RemoveInventoryItem(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();

            // Clear cache after deleting an item
            _cache.Remove("inventoryItems");

            return NoContent();
        }
    }
}