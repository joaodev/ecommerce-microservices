namespace Orders.Api.DTOs;

public record CreateOrderItemRequest(Guid ProductId, int Quantity);

public record CreateOrderRequest(Guid CustomerId, List<CreateOrderItemRequest> Items);