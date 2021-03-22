using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Conexao
{
    public static class IServiceCollectionExtensions
    {
        public static void ConectRabbitMQ(this IServiceCollection services, IConfiguration config)
        {

            services.Configure<QueueConfig>(config.GetSection(nameof(QueueConfig)));

            // Gera apenas uma inst?ncia do rabbit no ciclo de vida da aplica??o
            services.AddSingleton(service =>
            {
                var queueConfig = service.GetService<IOptions<QueueConfig>>();
                var connectionFactory = new QueueConnectionFactory(queueConfig);
                return connectionFactory.CreateConnection();
            });

            // Gera um canal de exchange para cada solicita??o
            services.AddTransient(services =>
            {
                var connection = services.GetService<IConnection>();

                return connection.CreateModel();
            });
            services.AddTransient<RabbitService>();
        }
    }
}
