using FoodDelivery.DataAccess.Context;
using FoodDelivery.DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FoodDelivery.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/orders
        [HttpGet]
        public IActionResult GetOrders()
        {
            var orders = _context.Orders.ToList();
            return Ok(orders);
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        public IActionResult GetOrder(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        // POST: api/orders
        [HttpPost]
        public IActionResult CreateOrder([FromBody] Order order)
        {
            _context.Orders.Add(order);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
    }
}
