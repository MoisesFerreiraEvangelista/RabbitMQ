using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Threading;

namespace Conexao
{
    public class QueueConnectionFactory
    {
        private ConnectionFactory _connectionFactory;

        private IConnection _connection;

        private IOptions<QueueConfig> _queueConfig;

        private IModel _channel { get; set; }

        public QueueConnectionFactory(IOptions<QueueConfig> queueConfig)
        {
            _queueConfig = queueConfig;
        }

        public IConnection CreateConnection()
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = _queueConfig.Value.HostName,
                UserName = _queueConfig.Value.UserName,
                Password = _queueConfig.Value.Password,
                Port = _queueConfig.Value.Port,
                RequestedHeartbeat = TimeSpan.FromSeconds(15),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(15)
            };

            Console.WriteLine($" [-] Conectando ao RabbitMq em Host: {_connectionFactory.HostName} | User: {_connectionFactory.UserName} | Password: {_connectionFactory.Password}");

            Reconnect();

            return _connection;
        }

        private void Reconnect()
        {
            Cleanup();

            var mres = new ManualResetEventSlim(false);

            while (!mres.Wait(5000))
            {
                try
                {
                    Connect();

                    Console.WriteLine(" [-] Conectado ao RabbitMq");

                    mres.Set();
                }
                catch (Exception)
                {
                    Console.WriteLine(" [*] Falha na conexão com o RabbitMq. Tentando novamente em 5s...");
                }
            }
        }


        private void Connect()
        {
            _connection = _connectionFactory.CreateConnection();
            _connection.ConnectionShutdown += Connection_ConnectionShutdown;
        }

        private void Connection_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine(" [*] Falha na conexão com o RabbitMq!");

            Reconnect();
        }

        public IModel CreateModel()
        {
            if (_connection == null || !_connection.IsOpen)
                CreateConnection();

            _channel = _connection.CreateModel();

            return _channel;
        }


        private void Cleanup()
        {
            try
            {
                if (_channel != null && _channel.IsOpen)
                {
                    _channel = null;
                }

                if (_connection != null && _connection.IsOpen)
                {
                    _connection.ConnectionShutdown -= Connection_ConnectionShutdown;
                    _connection.Close();
                    _connection = null;
                }
            }
            catch (Exception)
            {
            }
        }

        public void Dispose()
        {
            Cleanup();
        }
    }
}

