namespace Orders.Api.DTOs;

public record OrderItemResponse(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);

public record OrderResponse(Guid Id, Guid CustomerId, string Status, decimal Total, List<OrderItemResponse> Items);