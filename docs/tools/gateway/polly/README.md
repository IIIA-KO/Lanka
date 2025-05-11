# Polly Integration

Polly is a .NET resilience and fault handling library that allows applications to handle transient faults and improve their reliability. It provides a set of policies that can be used to handle different types of faults, such as retries, circuit breakers, and timeouts.

## Usage in Lanka Project

In the Lanka project, Polly is used to:

* Handle transient faults when communicating with backend services.
* Improve the resilience of the system.
* Prevent cascading failures.

## Configuration

Polly is configured in the `ResilientHttpClientFactory.cs` file.

### Retry Policy

The retry policy is used to retry requests that fail due to transient faults.

### Circuit Breaker Policy

The circuit breaker policy is used to prevent cascading failures. It monitors the success rate of requests to a backend service and, if the success rate falls below a certain threshold, it opens the circuit breaker and stops sending requests to the backend service.

### Timeout Policy

The timeout policy is used to prevent requests from taking too long. If a request takes longer than the configured timeout, the timeout policy cancels the request.

## Code Examples

### Configuring Polly Policies

Polly policies are configured in the `ResiliencePolicyBuilder.cs` file.

### Using Polly Policies with YARP

Polly policies are used with YARP by creating a custom `IForwarderHttpClientFactory` that uses Polly to handle requests.

## Troubleshooting Tips
