namespace Conexao
{
    public class QueueConfig
    {
        public string HostName { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string QueueName { get; set; }

        public int Port { get; set; } = 5672;

        public int RetryCount { get; set; }
    }
}
