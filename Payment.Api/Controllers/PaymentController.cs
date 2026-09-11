using Microsoft.AspNetCore.Mvc;
using Payment.Api.Data;
using Payment.Api.DTOs;
using Payment.Api.Models;
using Payment.Api.Services;

namespace Auth.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentDbContext _db;
        private readonly RabbitMqPublisher _publisher;

        public PaymentController(PaymentDbContext db, RabbitMqPublisher publisher)
        {
            _db = db;
            _publisher = publisher;
        }

        [HttpPost]
        public async Task<ActionResult<PaymentResponse>> Process(ProcessPaymentRequest request)
        {
            var status = request.Amount > 0 ? PaymentStatus.Approved : PaymentStatus.Rejected;

            var transaction = new PaymentTransaction
            {
                OrderId = request.OrderId,
                Amount = request.Amount,
                Status = status
            };

            _db.Payments.Add(transaction);
            await _db.SaveChangesAsync();

            if (status == PaymentStatus.Approved)
            {
                await _publisher.PublishPaymentApprovedAsync(transaction.OrderId, transaction.Amount);
            }

            return Ok(new PaymentResponse(transaction.Id, transaction.OrderId, transaction.Amount, transaction.Status.ToString()));
        }
    }
}