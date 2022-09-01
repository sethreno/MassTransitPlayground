using Consume;
using Contracts;
using MassTransit;
using RabbitMQ.Client;

var config = new ConfigurationBuilder().AddCommandLine(args).Build();
if (config["clientCodes"] == null)
    throw new ConfigurationException("clientCodes not specified");
var clientCodes = config["clientCodes"].Split(",").ToHashSet();

Microsoft.Extensions.Hosting.IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<Consume.MessageConsumer>();

            x.UsingRabbitMq(
                (ctx, cfg) =>
                {
                    cfg.Host(
                        "host.docker.internal",
                        "/",
                        h =>
                        {
                            h.Username("guest");
                            h.Password("guest");
                        }
                    );
                    cfg.Send<Message>(t =>
                    {
                        t.UseRoutingKeyFormatter(context => context.Message.ClientCode);
                    });
                    cfg.Message<Message>(x => x.SetEntityName("message"));
                    cfg.Publish<Message>(x => x.ExchangeType = ExchangeType.Direct);

                    foreach (var clientCode in clientCodes)
                    {
                        // create a separate queue for each client
                        // messages sent to the "message" queue will be routed
                        // to the client specific queue based on ClientCode
                        cfg.ReceiveEndpoint(
                            $"{clientCode}-messages",
                            x =>
                            {
                                // SAC because we don't want two messages for the same
                                // client to be processed in parallel due to performance
                                // & deadlocking issues in the client db
                                x.SetQueueArgument("x-single-active-consumer", true);

                                // maybe this can be higher?
                                x.PrefetchCount = 1;

                                x.ConfigureConsumeTopology = false;
                                x.Consumer<MessageConsumer>(
                                    services.BuildServiceProvider(),
                                    y =>
                                    {
                                        // not sure we need this
                                        y.ConcurrentMessageLimit = 1;
                                    }
                                );
                                x.Bind(
                                    "message",
                                    s =>
                                    {
                                        s.RoutingKey = clientCode;
                                        s.ExchangeType = ExchangeType.Direct;
                                    }
                                );
                            }
                        );
                    }
                }
            );
        });
    })
    .Build();

await host.RunAsync();
