using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
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

// FEATURE-AUTH: Fake JWT helpers
string GenerateFakeToken(string email)
{
    return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{email}:{Guid.NewGuid()}"));
}

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

// Authentication endpoints
app.MapPost("/login", (LoginRequest req) =>
{
    var user = users.FirstOrDefault(u => u.Email == req.Email && u.Password == req.Password);
    if (user == null)
        return Results.Unauthorized();

    var token = GenerateFakeToken(user.Email);
    return Results.Ok(new { Token = token, Message = "Login successful - fake JWT generated" });
});

app.MapGet("/me", (string token) =>
{
    var email = DecodeFakeToken(token);
    if (email == null)
        return Results.Unauthorized();

    return Results.Ok(new { AuthenticatedUser = email });
});

app.MapPost("/logout", (string token) =>
{
    var email = DecodeFakeToken(token);
    if (email == null)
        return Results.Unauthorized();

    return Results.Ok(new { Message = $"User {email} logged out successfully" });
});

// -------------------- PROFILE --------------------
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

// 📊 Summary of all users
app.MapGet("/profiles/summary", () =>
{
    if (!users.Any())
    {
        return Results.Ok(new
        {
            TotalUsers = 0,
            OldestUser = "N/A",
            YoungestUser = "N/A",
            AverageAge = "N/A"
        });
    }

    var today = DateTime.Today;
    var userAges = users.Select(u => today.Year - u.BirthDate.Year -
        (u.BirthDate.Date > today.AddYears(-(today.Year - u.BirthDate.Year)) ? 1 : 0));

    var summary = new
    {
        TotalUsers = users.Count,
        OldestUser = users.OrderBy(u => u.BirthDate).First().Name,
        YoungestUser = users.OrderByDescending(u => u.BirthDate).First().Name,
        AverageAge = Math.Round(userAges.Average(), 1)
    };

    return Results.Ok(summary);
});

// 🕒 Simulated activity tracking
var userLastActivity = new Dictionary<string, DateTime>();

app.MapPost("/profile/{email}/login", (string email) =>
{
    var user = users.FirstOrDefault(u => u.Email == email);
    if (user == null)
        return Results.NotFound($"User with email {email} not found.");

    userLastActivity[email] = DateTime.Now;
    return Results.Ok(new { Message = $"User {email} logged in at {DateTime.Now}" });
});

app.MapGet("/profile/{email}/activity", (string email) =>
{
    if (!userLastActivity.ContainsKey(email))
        return Results.NotFound($"No recent activity found for {email}.");

    return Results.Ok(new { Email = email, LastLogin = userLastActivity[email], Status = "Active" });
});

app.MapDelete("/profile/{email}", (string email) =>
{
    var user = users.FirstOrDefault(u => u.Email == email);
    if (user == null)
        return Results.NotFound($"User with email {email} not found.");

    users.Remove(user);
    return Results.Ok(new { Message = $"Profile for {email} deleted successfully." });
});

// -------------------- MENU --------------------
app.MapGet("/menu", (string? search, string? sortBy, string? category) =>
{
    IEnumerable<MenuItem> filtered = menu;

    if (!string.IsNullOrWhiteSpace(search))
    {
        filtered = filtered.Where(m =>
            m.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            m.Category.Contains(search, StringComparison.OrdinalIgnoreCase));
    }

    if (!string.IsNullOrWhiteSpace(category))
    {
        filtered = filtered.Where(m => m.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    }

    if (!string.IsNullOrEmpty(sortBy))
    {
        filtered = sortBy.ToLower() switch
        {
            "name_asc" => filtered.OrderBy(m => m.Name),
            "name_desc" => filtered.OrderByDescending(m => m.Name),
            "price_asc" => filtered.OrderBy(m => m.Price),
            "price_desc" => filtered.OrderByDescending(m => m.Price),
            "rating_asc" => filtered.OrderBy(m => m.Rating),
            "rating_desc" => filtered.OrderByDescending(m => m.Rating),
            _ => filtered
        };
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

app.MapGet("/menu/vegetarian", () =>
{
    var vegetarianMenu = menu
        .Where(m => m.Vegetarian)
        .Select(m => new
        {
            m.Id,
            m.Name,
            m.Category,
            m.Rating,
            Price = $"${m.Price:0.00}"
        });

    return Results.Ok(vegetarianMenu);
});

app.MapGet("/menu/top-rated", () =>
{
    var topRated = menu
        .OrderByDescending(m => m.Rating)
        .Take(3)
        .Select(m => new
        {
            m.Id,
            m.Name,
            m.Category,
            m.Rating,
            Price = $"${m.Price:0.00}"
        });

    return Results.Ok(topRated);
});

// -------------------- CART --------------------
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

// -------------------- ORDERS --------------------
app.MapPost("/orders/{email}", (string email) =>
{
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

app.MapGet("/orders/summary", () =>
{
    if (!orders.Any())
        return Results.Ok(new { TotalOrders = 0, Delivered = 0, Cancelled = 0, InProcess = 0 });

    var summary = new
    {
        TotalOrders = orders.Count,
        Delivered = orders.Count(o => o.Status == "Delivered"),
        Cancelled = orders.Count(o => o.Status == "Cancelled"),
        InProcess = orders.Count(o => o.Status == "In Process")
    };

    return Results.Ok(summary);
});

app.MapPut("/orders/{orderId}/confirm", (int orderId) =>
{
    var order = orders.FirstOrDefault(o => o.Id == orderId);
    if (order == null) return Results.NotFound();
    if (order.Status != "In Process") return Results.BadRequest("Already confirmed");

    order.Status = "Delivered";
    return Results.Ok(order);
});

// -------------------- INFO / SUPPORT --------------------
app.MapGet("/status", () =>
{
    var info = new
    {
        Server = "FoodDelivery Backend API",
        Status = "Running",
        Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        ActiveUsers = users.Count,
        MenuItems = menu.Count,
        Orders = orders.Count
    };

    return Results.Ok(info);
});

app.MapGet("/about", () =>
{
    var info = new
    {
        Project = "Food Delivery Backend API",
        Version = "1.0.0",
        Description = "A demo backend for managing food delivery services, built with minimal APIs.",
        Contributors = new[] { "Ulas Tuz" },
        GitHub = "https://github.com/ulasfinance/introtosoftware"
    };

    return Results.Ok(info);
});

app.MapPost("/support", (SupportRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Message))
        return Results.BadRequest("Email and message are required.");

    return Results.Ok(new
    {
        Status = "Received",
        ConfirmationId = Guid.NewGuid(),
        ReceivedAt = DateTime.Now,
        Message = $"Support request from {req.Email} received successfully."
    });
});

record SupportRequest(string Email, string Message);

app.Run();

// -------------------- RECORDS --------------------
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

