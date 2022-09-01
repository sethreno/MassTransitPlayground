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
                        Console.WriteLine($"creating queue for {clientCode}");

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

                                x.ConfigureConsumeTopology = false;
                                x.Consumer<MessageConsumer>(services.BuildServiceProvider());
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

                    // I don't think the following is needed
                    // pretty sure that's the code above eliminates the need
                    // to call ConfigureEndpoints
                    //cfg.ConfigureEndpoints(context);
                }
            );
        });

        //services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
