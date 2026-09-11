using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orders.Api.Data;
using Orders.Api.DTOs;
using Orders.Api.Models;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly OrdersDbContext _db;

    public OrderController(OrdersDbContext db)
    {
        _db = db;
    }

    [HttpGet("products")]
    public async Task<ActionResult<List<Product>>> GetProducts()
    {
        var products = await _db.Products.ToListAsync();
        return Ok(products);
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request)
    {
        if (request.Items.Count == 0)
            return BadRequest("O pedido precisa ter pelo menos um item.");

        var order = new Order { CustomerId = request.CustomerId };

        foreach (var item in request.Items)
        {
            var product = await _db.Products.FindAsync(item.ProductId);
            if (product is null)
                return BadRequest($"Produto {item.ProductId} nao encontrado.");

            if (product.StockQuantity < item.Quantity)
                return BadRequest($"Estoque insuficiente para o produto '{product.Name}'.");

            product.StockQuantity -= item.Quantity;

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });
        }

        order.Total = order.Items.Sum(i => i.UnitPrice * i.Quantity);
        order.Status = OrderStatus.AwaitingPayment;

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return Ok(ToResponse(order));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponse>> GetById(Guid id)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        if (order is null)
            return NotFound();

        return Ok(ToResponse(order));
    }

    private static OrderResponse ToResponse(Order order) => new(
        order.Id,
        order.CustomerId,
        order.Status.ToString(),
        order.Total,
        order.Items.Select(i => new OrderItemResponse(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList()
    );
}