using Produce;
using MassTransit;

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
                    cfg.ConfigureEndpoints(context);
                }
            );
        });

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
