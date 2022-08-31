using Produce;
using MassTransit;
using Contracts;
using RabbitMQ.Client;

Microsoft.Extensions.Hosting.IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
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

                    cfg.ConfigureEndpoints(context);
                }
            );
        });

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
