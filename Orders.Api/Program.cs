using Microsoft.EntityFrameworkCore;
using Orders.Api.Data;
using Orders.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("OrdersDb")));
    
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    if (!db.Products.Any())
    {
        db.Products.AddRange(
            new Product { Name = "Teclado mecanico", Price = 250m, StockQuantity = 10 },
            new Product { Name = "Mouse gamer", Price = 120m, StockQuantity = 15 },
            new Product { Name = "Monitor 27\"", Price = 900m, StockQuantity = 5 }
        );
        db.SaveChanges();
    }
}

app.Run();
