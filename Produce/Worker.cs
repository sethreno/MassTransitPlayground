namespace Produce;
using Contracts;
using MassTransit;

public class Worker : BackgroundService
{
    readonly IBus _bus;
    readonly HashSet<string> _clientCodes;

    public Worker(IBus bus)
    {
        _bus = bus;
        var args = Environment.GetCommandLineArgs();
        var clientsIndex = Array.IndexOf(args, "--clients");
        var clients = args[clientsIndex + 1];
        _clientCodes = clients.Split(",").ToHashSet();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var clientCode in _clientCodes)
            {
                await _bus.Publish(
                    new Message
                    {
                        ClientCode = clientCode,
                        Text = $"The time is {DateTimeOffset.Now}"
                    }
                );

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
