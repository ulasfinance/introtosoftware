using FoodDelivery.DataAccess.Context;
using FoodDelivery.DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FoodDelivery.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RatingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ratings
        [HttpGet]
        public IActionResult GetRatings()
        {
            var ratings = _context.Ratings.ToList();
            return Ok(ratings);
        }

        // POST: api/ratings
        [HttpPost]
        public IActionResult AddRating([FromBody] Rating rating)
        {
            // simple validation: user and dish must exist
            var userExists = _context.Users.Any(u => u.Id == rating.UserId);
            var dishExists = _context.Dishes.Any(d => d.Id == rating.DishId);

            if (!userExists || !dishExists)
                return BadRequest("Invalid UserId or DishId");

            _context.Ratings.Add(rating);
            _context.SaveChanges();

            return Ok(rating);
        }
    }
}
