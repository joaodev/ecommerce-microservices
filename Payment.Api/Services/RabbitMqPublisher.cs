using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Payment.Api.Services
{
    public class RabbitMqPublisher
    {
        private readonly IConfiguration _config;
        
        public RabbitMqPublisher(IConfiguration config)
        {
            _config = config;
        }

        public async Task PublishPaymentApprovedAsync(Guid orderId, decimal amount)
        {
            var factory = new ConnectionFactory
            {
                HostName = _config["RabbitMq:Host"] ?? "localhost",
                UserName = _config["RabbitMq:User"] ?? "guest",
                Password = _config["RabbitMq:Password"] ?? "guest"
            };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: "payment-approved",
                durable: true,
                exclusive: false,
                autoDelete: false);  

            var payload = JsonSerializer.Serialize(new { OrderId = orderId, Amount = amount, ApprovedAt = DateTime.UtcNow });
            var body = Encoding.UTF8.GetBytes(payload);

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: "payment-approved",
                body: body);
        }
    }
}