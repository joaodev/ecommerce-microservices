namespace Notification.Api.DTOs;

public record PaymentApprovedEvent(Guid OrderId, decimal Amount, DateTime ApprovedAt);