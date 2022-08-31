using Consume;
using Contracts;
using MassTransit;
using RabbitMQ.Client;

Microsoft.Extensions.Hosting.IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
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
                    cfg.Send<Message>(t =>
                    {
                        t.UseRoutingKeyFormatter(context => context.Message.ClientCode);
                    });
                    cfg.Message<Message>(x => x.SetEntityName("message"));
                    cfg.Publish<Message>(x => x.ExchangeType = ExchangeType.Direct);

                    cfg.ReceiveEndpoint(
                        "aag-messages",
                        x =>
                        {
                            x.ConfigureConsumeTopology = false;
                            x.Consumer<MessageConsumer>();
                            x.SetQueueArgument("x-single-active-consumer", true);
                            x.Bind(
                                "message",
                                s =>
                                {
                                    s.RoutingKey = "aag";
                                    s.ExchangeType = ExchangeType.Direct;
                                }
                            );
                        }
                    );

                    cfg.ReceiveEndpoint(
                        "acore-messages",
                        x =>
                        {
                            x.ConfigureConsumeTopology = false;
                            x.Consumer<MessageConsumer>();
                            x.SetQueueArgument("x-single-active-consumer", true);
                            x.Bind(
                                "message",
                                s =>
                                {
                                    s.RoutingKey = "acore";
                                    s.ExchangeType = ExchangeType.Direct;
                                }
                            );
                        }
                    );

                    // I don't think the following is needed
                    // pretty sure that's the code above eliminates the need
                    // to call ConfigureEndpoints
                    //cfg.ConfigureEndpoints(context);
                }
            );
        });

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
