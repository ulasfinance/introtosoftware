namespace FoodDelivery.DataAccess.Entities
{
    public class Rating
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int DishId { get; set; }
        public int Score { get; set; }
    }
}
