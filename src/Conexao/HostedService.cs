using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Conexao
{
    public class HostedService : IHostedService
    {
        #region Constructor
        private readonly RabbitService rabbitRequestService;

        public HostedService(
            RabbitService rabbitRequestService)
        {
            this.rabbitRequestService = rabbitRequestService;
        }
        #endregion
        public Task StartAsync(CancellationToken cancellationToken)
        {            
            rabbitRequestService.Start();

            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
