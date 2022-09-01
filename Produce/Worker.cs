namespace Produce;
using Contracts;
using MassTransit;

public class Worker : BackgroundService
{
    readonly IBus _bus;
    readonly ILogger<Worker> _logger;
    readonly HashSet<string> _clientCodes;
    readonly int _publishPerSecond;

    public Worker(IBus bus, ILogger<Worker> logger, IConfiguration config)
    {
        _bus = bus;
        _logger = logger;
        _clientCodes = config["clientCodes"].Split(",").ToHashSet();
        _publishPerSecond = config.GetValue<int?>("publishPerSecond") ?? 1;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var clientCode in _clientCodes)
            {
                for (var i = 1; i <= _publishPerSecond; i++)
                {
                    await _bus.Publish(
                        new Message
                        {
                            ClientCode = clientCode,
                            Text = $"published: {DateTimeOffset.Now:u}"
                        }
                    );
                }
                _logger.LogInformation($"published for {clientCode}");
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
