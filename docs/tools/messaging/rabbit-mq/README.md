# RabbitMQ Configuration

RabbitMQ is a widely deployed open-source message broker. It is used in the Lanka project as the transport for MassTransit.

## Usage in Lanka Project

In the Lanka project, RabbitMQ is used for:

* Transporting messages between modules.
* Implementing the Outbox and Inbox patterns for reliable messaging.

## Configuration

### Connection Settings

The RabbitMQ connection settings are configured in the `AddMassTransit` extension method in `InfrastructureConfiguration.cs`.

```csharp
services.AddMassTransit(configure =>
{
    configure.AddConsumersFromNamespaceContaining<CampaignCreatedConsumer>();

    configure.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq://localhost:5672");
        cfg.ConfigureEndpoints(context);
    });
});
```

### Exchange Configuration

MassTransit automatically configures RabbitMQ exchanges based on the defined message contracts.

### Queue Configuration

MassTransit automatically configures RabbitMQ queues based on the defined consumers.

## Setting Up RabbitMQ

1. Run the RabbitMQ Docker container:

    ```bash
    docker run -d --name lanka.rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
    ```

2. Access the RabbitMQ management UI at `http://localhost:15672`.
3. The default username and password are `guest`.