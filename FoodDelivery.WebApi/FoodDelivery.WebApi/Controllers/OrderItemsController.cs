using FoodDelivery.DataAccess.Context;
using FoodDelivery.DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FoodDelivery.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/orderitems
        [HttpGet]
        public IActionResult GetOrderItems()
        {
            var items = _context.OrderItems.ToList();
            return Ok(items);
        }

        // POST: api/orderitems
        [HttpPost]
        public IActionResult AddOrderItem([FromBody] OrderItem orderItem)
        {
            _context.OrderItems.Add(orderItem);
            _context.SaveChanges();
            return Ok(orderItem);
        }
    }
}
