using System.Text;
using System.Text.Json;
using Notification.Api.DTOs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Notification.Api.Services
{
    public class PaymentApprovedConsumer : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<PaymentApprovedConsumer> _logger;
        private IConnection? _connection;
        private IChannel? _channel;
        
        public PaymentApprovedConsumer(IConfiguration config, ILogger<PaymentApprovedConsumer> logger)
        {
            _config = config;
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _config["RabbitMq:Host"] ?? "localhost",
                UserName = _config["RabbitMq:User"] ?? "guest",
                Password = _config["RabbitMq:Password"] ?? "guest"
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            
            await _channel.QueueDeclareAsync(
                queue: "payment-approved", 
                durable: true, 
                exclusive: false, 
                autoDelete: false, 
                cancellationToken: cancellationToken);
            
            await base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel!);
            
            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                
                _logger.LogInformation("Evento payment-approved recebido: {Message}", message);
                
                var payload = JsonSerializer.Deserialize<PaymentApprovedEvent>(message);
                _logger.LogInformation(
                    "Notificacao enviada para o pedido {OrderId} — valor R$ {Amount}",
                    payload?.OrderId, payload?.Amount);
                
                // Here you can add logic to send notification to the user
                
                await _channel!.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
            };
            
            return _channel!.BasicConsumeAsync(
                queue: "payment-approved", 
                autoAck: false, 
                consumer: consumer, 
                cancellationToken: stoppingToken);
        }
        
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel is not null) await _channel.CloseAsync(cancellationToken);
            if (_connection is not null) await _connection.CloseAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}