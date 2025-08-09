using Microsoft.AspNetCore.Mvc;
using LogiTrack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace LogiTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly LogiTrackContext _context;
        private readonly IMemoryCache _cache;

        public OrderController(LogiTrackContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        /// <summary>
        /// Retrieves a list of all orders.
        /// </summary>
        /// <returns>A list of orders.</returns>
        /// <response code="200">Orders retrieved successfully.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            if (_cache.TryGetValue("orders", out var cachedOrders))
            {
                return Ok(cachedOrders);
            }

            var orders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.InventoryItem)
                .ToListAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(1));

            _cache.Set("orders", orders, cacheEntryOptions);

            return Ok(orders);
        }


        /// <summary>
        /// Retrieves an order by its ID.
        /// </summary>
        /// <param name="id">The ID of the order.</param>
        /// <returns>The order with the specified ID.</returns>
        /// <response code="200">Order retrieved successfully.</response>
        /// <response code="404">Order not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var cacheKey = $"order_{id}";
            if (_cache.TryGetValue(cacheKey, out var cachedOrder))
            {
                return Ok(cachedOrder);
            }

            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.InventoryItem)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(30));

            _cache.Set(cacheKey, order, cacheEntryOptions);

            return Ok(order);
        }


        /// <summary>
        /// Creates a new order.
        /// </summary>
        /// <param name="order">The order to create.</param>
        /// <returns>The created order.</returns>
        /// <response code="201">Order created successfully.</response>
        [Authorize(Roles = "Manager")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _cache.Remove("orders");

            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, order);
        }

        /// <summary>
        /// Deletes an order by its ID.
        /// </summary>
        /// <param name="id">The ID of the order.</param>
        /// <returns>No content.</returns>
        /// <response code="204">Order deleted successfully.</response>
        /// <response code="404">Order not found.</response>
        [Authorize(Roles = "Manager")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            _cache.Remove("orders");
            _cache.Remove($"order_{id}");

            return NoContent();
        }

        private async Task RehydrateCache()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.InventoryItem)
                .ToListAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(1));

            _cache.Set("orders", orders, cacheEntryOptions);
        }
    }
}