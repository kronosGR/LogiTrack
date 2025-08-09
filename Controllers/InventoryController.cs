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

        [HttpGet]
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

        [HttpGet("{id}")]
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

        [Authorize(Roles = "Manager")]
        [HttpPost]
        public async Task<ActionResult<InventoryItem>> AddInventoryItem(InventoryItem item)
        {
            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();

            // Clear cache after creating a new item
            _cache.Remove("inventoryItems");

            return CreatedAtAction(nameof(GetInventoryItem), new { id = item.ItemId }, item);

        }

        [Authorize(Roles = "Manager")]
        [HttpPut("{id}")]
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


        [Authorize(Roles = "Manager")]
        [HttpDelete("{id}")]
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