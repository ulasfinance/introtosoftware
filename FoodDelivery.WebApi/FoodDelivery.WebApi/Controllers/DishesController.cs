using FoodDelivery.DataAccess.Context;
using FoodDelivery.DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FoodDelivery.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DishesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DishesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/dishes
        [HttpGet]
        public IActionResult GetAllDishes()
        {
            var dishes = _context.Dishes.ToList();
            return Ok(dishes);
        }

        // GET: api/dishes/{id}
        [HttpGet("{id}")]
        public IActionResult GetDish(int id)
        {
            var dish = _context.Dishes.Find(id);
            if (dish == null) return NotFound();
            return Ok(dish);
        }

        // POST: api/dishes
        [HttpPost]
        public IActionResult CreateDish([FromBody] Dish dish)
        {
            _context.Dishes.Add(dish);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetDish), new { id = dish.Id }, dish);
        }

        // PUT: api/dishes/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateDish(int id, [FromBody] Dish updatedDish)
        {
            var dish = _context.Dishes.Find(id);
            if (dish == null) return NotFound();

            dish.Name = updatedDish.Name;
            _context.SaveChanges();

            return NoContent();
        }

        // DELETE: api/dishes/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteDish(int id)
        {
            var dish = _context.Dishes.Find(id);
            if (dish == null) return NotFound();

            _context.Dishes.Remove(dish);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
