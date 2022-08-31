using Consume;
using MassTransit;
using GreenPipes.Util;
using RabbitMQ.Client;
//using RoutingKeyTopic;

Microsoft.Extensions.Hosting.IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {

        var clientsIndex = Array.IndexOf(args, "--clients");
        var clients = args[clientsIndex + 1];
        var clientCodes = clients.Split(",").ToHashSet();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<Consume.MessageConsumer>();


            x.UsingRabbitMq(
                (context, cfg) =>
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

                    foreach(var clientCode in clientCodes)
                    {
                        cfg.ReceiveEndpoint($"message-{clientCode}", y =>
                        {
                            y.ConfigureConsumeTopology = false;

                            y.Consumer<MessageConsumer>();


                            y.Bind("message", s => 
                            {
                                s.RoutingKey = clientCode;
                                s.ExchangeType = ExchangeType.Direct;
                            });
                        });
                    }

                    //cfg.ConfigureEndpoints(context);
                }
            );
        });

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
