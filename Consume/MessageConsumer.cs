using System.Threading.Tasks;
using Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Consume;

public class MessageConsumer : IConsumer<Message>
{
    /*
    readonly ILogger<MessageConsumer> _logger;

    public MessageConsumer(ILogger<MessageConsumer> logger)
    {
        _logger = logger;
    }
    */

    public Task Consume(ConsumeContext<Message> context)
    {
        var m = context.Message;
        //_logger.LogInformation($"Received {m.ClientCode} {m.Text}");
        Console.WriteLine($"Received {m.ClientCode} {m.Text}");

        return Task.CompletedTask;
    }
}
