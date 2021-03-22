using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Conexao
{
    public class RabbitService : IDisposable
    {
        #region Constructor
        private readonly IOptions<QueueConfig> _queueConfig;
        private readonly IModel _channel;


        public RabbitService(
            IModel channel,
            IOptions<QueueConfig> options
        )
        {
            _queueConfig = options;
            _channel = channel;
        }
        #endregion

        public void Start()
        {
            ListemListeningToQueue();
        }

        private void ListemListeningToQueue()
        {
            _channel.QueueDeclare(
                            queue: _queueConfig.Value.QueueName,
                            durable: true,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null
                        );

            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new EventingBasicConsumer(_channel);

            _channel.BasicConsume(
                queue: _queueConfig.Value.QueueName,
                autoAck: false,
                consumer: consumer
            );

            consumer.Received += OnRequestReceived;

            Log($" [-] Aguardando requisicoes na fila {_queueConfig.Value.QueueName}");
        }

        private void OnRequestReceived(object sender, BasicDeliverEventArgs @event)
        {
            Log("\n ------");           

            var request = Encoding.UTF8.GetString(@event.Body.ToArray());

            Log($"request recebido {request}");

            ConfirmAndRemoveRequest(_channel, @event);
        }

        private void ConfirmAndRemoveRequest(IModel channel, BasicDeliverEventArgs @event) =>
            channel.BasicAck(deliveryTag: @event.DeliveryTag, multiple: false);


        private void Log(string log) => Console.WriteLine(log);

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
        }
    }
}
