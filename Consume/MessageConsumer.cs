using Contracts;
using MassTransit;

namespace Consume;

public class MessageConsumer : IConsumer<Message>
{
    readonly ILogger<MessageConsumer> _logger;

    public MessageConsumer(ILogger<MessageConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<Message> context)
    {
        var m = context.Message;
        _logger.LogInformation($"Received {m.ClientCode} at {DateTimeOffset.Now:u} {m.Text}");

        return Task.CompletedTask;
    }
}
