# Messaging in Lanka Project

Messaging is a crucial aspect of the Lanka project, enabling asynchronous communication between different modules and services. This allows for better decoupling, scalability, and resilience. The Lanka project utilizes MassTransit and RabbitMQ to facilitate message-based communication.

This document provides an overview of the messaging tools used in the Lanka project and how they work together.

## Messaging Tools

The Lanka project uses the following tools for messaging:

* **MassTransit:** A free, open-source distributed application framework for .NET. MassTransit makes it easy to create message-based applications and services using .NET.
* **RabbitMQ:** A widely deployed open-source message broker. MassTransit supports RabbitMQ as one of its transport options.

## How They Work Together

MassTransit simplifies the process of working with RabbitMQ by providing a higher-level abstraction for message-based communication.

1. MassTransit is used to define message contracts, consumers, and sagas.
2. MassTransit configures RabbitMQ exchanges and queues based on the defined message contracts.
3. Messages are published to RabbitMQ exchanges using MassTransit's `IPublishEndpoint`.
4. Consumers are configured to receive messages from RabbitMQ queues using MassTransit's `IConsumer`.
5. MassTransit handles message serialization, routing, and error handling.

![Messaging](/docs/images/messaging.jpg)

## Detailed Documentation

* [MassTransit Integration](./mass-transit/README.md)
* [RabbitMQ Configuration](./rabbit-mq/README.md)
