using Contracts;
using MassTransit;

namespace Consume;

public class MessageConsumer : IConsumer<Message>
{
    readonly ILogger<MessageConsumer> _logger;
    readonly int _delayInSeconds;

    public MessageConsumer(ILogger<MessageConsumer> logger, IConfiguration config)
    {
        _logger = logger;
        _delayInSeconds = config.GetValue<int?>("delayInSeconds") ?? 1;
    }

    public Task Consume(ConsumeContext<Message> context)
    {
        var m = context.Message;
        _logger.LogInformation(
            $@"
Thread: {Thread.CurrentThread.ManagedThreadId}
ClientCode: {m.ClientCode}
Received {DateTimeOffset.Now:u}
{m.Text}"
        );

        Task.Delay(_delayInSeconds * 1_000);

        return Task.CompletedTask;
    }
}
