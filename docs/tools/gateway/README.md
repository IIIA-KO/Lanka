# Gateway in Lanka Project

The Gateway is a crucial component of the Lanka project, acting as a reverse proxy and providing resilience and rate limiting capabilities. It protects the API from abuse and ensures that the system remains available and responsive. The Lanka project utilizes YARP, Polly, and Rate Limiting to achieve these goals.

This document provides an overview of the tools used in the Gateway and how they work together.

## Gateway Tools

The Lanka project uses the following tools in the Gateway:

* **YARP (Yet Another Reverse Proxy):** A reverse proxy toolkit for .NET that is used to route requests to backend services.
* **Polly:** A .NET resilience and fault handling library that allows applications to handle transient faults and improve their reliability.
* **Rate Limiting:** A technique used to control the rate of requests to an API, protecting it from abuse and ensuring that the system remains available and responsive.

## How They Work Together

These tools work together to provide a comprehensive gateway solution:

1. YARP is used to route requests to backend services based on predefined routes and clusters.
2. Polly is used to handle transient faults and improve the resilience of the system.
3. Rate Limiting is used to control the rate of requests to the API, protecting it from abuse.

![Gateway](/docs/images/gateway.jpg)

## Detailed Documentation

* [YARP Integration](./yarp/README.md)
* [Polly Integration](./polly/README.md)
* [Rate Limiting Configuration](./rate-limiting/README.md)