using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");

// Mock data
var users = new List<User>();
var menu = new List<MenuItem>
{
    new MenuItem { Id = 1, Name = "Pizza", Price = 10.99, Category = "Italian", Vegetarian = false, Rating = 4.5 },
    new MenuItem { Id = 2, Name = "Veggie Burger", Price = 8.49, Category = "American", Vegetarian = true, Rating = 4.8 },
    new MenuItem { Id = 3, Name = "Pasta", Price = 12.29, Category = "Italian", Vegetarian = true, Rating = 4.6 },
    new MenuItem { Id = 4, Name = "Steak", Price = 15.99, Category = "Grill", Vegetarian = false, Rating = 4.7 }
};
var carts = new Dictionary<string, List<MenuItem>>();
var orders = new List<Order>();

// Registration
app.MapPost("/register", (User user) =>
{
    if (users.Any(u => u.Email == user.Email))
        return Results.BadRequest("User already exists");

    users.Add(user);
    return Results.Ok(new { Token = Guid.NewGuid().ToString() });
});

// FEATURE-AUTH: Fake JWT helper
string GenerateFakeToken(string email)
{
    return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{email}:{Guid.NewGuid()}"));
}

// FEATURE-AUTH: Decode fake JWT
string? DecodeFakeToken(string token)
{
    try
    {
        var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
        return decoded.Split(':')[0];
    }
    catch
    {
        return null;
    }
}

// FEATURE-AUTH COMMIT #1: Login endpoint with fake JWT generator
app.MapPost("/login", (LoginRequest req) =>
{
    var user = users.FirstOrDefault(u => u.Email == req.Email && u.Password == req.Password);
    if (user == null)
        return Results.Unauthorized();

    var token = GenerateFakeToken(user.Email);
    return Results.Ok(new
    {
        Token = token,
        Message = "Login successful - fake JWT generated"
    });
});

// FEATURE-AUTH COMMIT #2: Protected /me endpoint to verify token
app.MapGet("/me", (string token) =>
{
    var email = DecodeFakeToken(token);
    if (email == null)
        return Results.Unauthorized();

    return Results.Ok(new { AuthenticatedUser = email });
});

// FEATURE-AUTH COMMIT #3: Added logout endpoint to clear fake token
app.MapPost("/logout", (string token) =>
{
    var email = DecodeFakeToken(token);
    if (email == null)
        return Results.Unauthorized();

    return Results.Ok(new { Message = $"User {email} logged out successfully" });
});

// Profile
app.MapGet("/profile/{email}", (string email) =>
{
    var user = users.FirstOrDefault(u => u.Email == email);
    return user == null ? Results.NotFound() : Results.Ok(user);
});

app.MapPut("/profile/{email}", (string email, User updated) =>
{
    var user = users.FirstOrDefault(u => u.Email == email);
    if (user == null) return Results.NotFound();

    user.Name = updated.Name;
    user.Address = updated.Address;
    user.Phone = updated.Phone;
    user.BirthDate = updated.BirthDate;
    return Results.Ok(user);
});

app.MapGet("/menu", (string? sortBy, string? category) =>
// Combined /menu endpoints with search, sorting, filtering, vegetarian, and top-rated options
app.MapGet("/menu", (string? search, string? sortBy, string? category) =>
{
    if (!users.Any())
        return Results.NotFound("No users found");

    var result = users.Select(u => new
    {
        u.Email,
        u.Name,
        u.Address,
        u.Phone,
        BirthDate = u.BirthDate.ToShortDateString()
    });

    return Results.Ok(result);
});

// FEATURE-PROFILE COMMIT #2: Added delete user profile endpoint
app.MapDelete("/profile/{email}", (string email) =>
{
    var user = users.FirstOrDefault(u => u.Email == email);
    if (user == null)
        return Results.NotFound($"User with email {email} not found.");

    users.Remove(user);
    return Results.Ok(new { Message = $"Profile for {email} deleted successfully." });
});



app.MapGet("/menu", (string? search) =>
{
    IEnumerable<MenuItem> filtered = menu;

    if (!string.IsNullOrWhiteSpace(search))
    {
        filtered = menu.Where(m => m.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                                 || m.Category.Contains(search, StringComparison.OrdinalIgnoreCase));
    }

    var result = filtered.Select(m => new
    {
        m.Id,
        m.Name,
        m.Category,
        m.Vegetarian,
        m.Rating,
        Price = $"${m.Price:0.00}"
    });

    return Results.Ok(result);
});


// Cart
app.MapPost("/cart/{email}/{itemId}", (string email, int itemId) =>
{
    var item = menu.FirstOrDefault(m => m.Id == itemId);
    if (item == null) return Results.NotFound();

    if (!carts.ContainsKey(email)) carts[email] = new List<MenuItem>();
    carts[email].Add(item);
    return Results.Ok(carts[email]);
});

app.MapGet("/cart/{email}", (string email) =>
{
    return carts.ContainsKey(email)
        ? Results.Ok(carts[email])
        : Results.Ok(new List<MenuItem>());
});

// Orders
// Orders
app.MapPost("/orders/{email}", (string email) =>
{
    // MAIN COMMIT #3: Validate delivery time using helper
    if (!FoodDelivery.WebApi.Utils.ValidationHelper.IsValidDeliveryTime(DateTime.Now.AddHours(1)))
        return Results.BadRequest("Invalid delivery time (must be at least 30 minutes ahead)");

    if (!carts.ContainsKey(email) || !carts[email].Any())
        return Results.BadRequest("Cart empty");

    var newOrder = new Order
    {
        Id = orders.Count + 1,
        UserEmail = email,
        Items = carts[email].ToList(),
        Status = "In Process",
        DeliveryTime = DateTime.Now.AddHours(1)
    };

    orders.Add(newOrder);
    carts[email].Clear();

    return Results.Ok(newOrder);
});

app.MapGet("/orders/{email}", (string email) =>
{
    var userOrders = orders.Where(o => o.UserEmail == email).ToList();
    return Results.Ok(userOrders);
});

app.MapPut("/orders/{orderId}/confirm", (int orderId) =>
{
    var order = orders.FirstOrDefault(o => o.Id == orderId);
    if (order == null) return Results.NotFound();
    if (order.Status != "In Process") return Results.BadRequest("Already confirmed");

    order.Status = "Delivered";
    return Results.Ok(order);
});


app.Run();

record User
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public DateTime BirthDate { get; set; }
}

record LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

record MenuItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double Price { get; set; }
    public string Category { get; set; }
    public bool Vegetarian { get; set; }
    public double Rating { get; set; }
}

record Order
{
    public int Id { get; set; }
    public string UserEmail { get; set; }
    public List<MenuItem> Items { get; set; }
    public string Status { get; set; }
    public DateTime DeliveryTime { get; set; }
}
