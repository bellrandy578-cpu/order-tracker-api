using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderContext _context;

        public OrdersController(OrderContext context)
        {
            _context = context;
        }

        // GET: api/Orders
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrderItems()
        {

            var ordersWithHistory = await _context.OrderItems
                .Include(o => o.History)
                .Select(o => new Order
                {
                    Id = o.Id,
                    Product = o.Product,
                    Status = o.Status,
                    OrderDate = o.OrderDate,
                    Amount = o.Amount,
                    History = o.History.Select(h => new OrderHistory
                    {
                        Id = h.Id,
                        OrderId = h.OrderId,
                        Status = h.Status,
                        ChangedAt = h.ChangedAt,
                        ChangedBy = h.ChangedBy
                    }).ToList()
                }).ToListAsync();

            if (ordersWithHistory == null)
            {
                return NotFound();
            }

            return ordersWithHistory;

        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.OrderItems
                .Where(o => o.Id == id)
                .Include(o => o.History)
                .Select(o => new Order
                {
                    Id = o.Id,
                    Product = o.Product,
                    Status = o.Status,
                    OrderDate = o.OrderDate,
                    Amount = o.Amount,
                    History = o.History.Select(h => new OrderHistory
                    {
                        Id = h.Id,
                        OrderId = h.OrderId,
                        Status = h.Status,
                        ChangedAt = h.ChangedAt,
                        ChangedBy = h.ChangedBy
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // GET: api/Orders/5/history
        [HttpGet("{orderId}/history")]
        public async Task<ActionResult<IEnumerable<OrderHistory>>> GetHistory(int orderId)
        {
            var history = await _context.OrderHistoryItems
                .Where(h => h.OrderId == orderId)
                .ToListAsync();

            if (!history.Any())
                return NotFound();

            return Ok(history);
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(long id, Order order)
        {
            if (id != order.Id)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Orders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            _context.OrderItems.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrder", new { id = order.Id }, order);
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteOrder(long id)
        {
            var order = await _context.OrderItems.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.OrderItems.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderExists(long id)
        {
            return _context.OrderItems.Any(e => e.Id == id);
        }
    }
}
