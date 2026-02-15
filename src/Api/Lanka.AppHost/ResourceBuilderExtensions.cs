using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Lanka.AppHost;

internal static class ResourceBuilderExtensions
{
    extension<T>(IResourceBuilder<T> builder) where T : IResourceWithEndpoints
    {
        internal IResourceBuilder<T> WithScalar()
        {
            return builder.WithOpenApiDocs(
                name: "scalar-ui-docs",
                displayName: "Scalar UI Documentation",
                openApiUiPath: "scalar/v1"
            );
        }

        internal IResourceBuilder<T> WithReplacedHealthCheck(string path)
        {
            foreach (HealthCheckAnnotation annotation in
                     builder.Resource.Annotations.OfType<HealthCheckAnnotation>().ToList())
            {
                builder.Resource.Annotations.Remove(annotation);
            }

            return builder.WithHttpHealthCheck(path);
        }

        private IResourceBuilder<T> WithOpenApiDocs(
            string name,
            string displayName,
            string openApiUiPath
        )
        {
            var commandOptions = new CommandOptions
            {
                UpdateState = context => context.ResourceSnapshot.HealthStatus == HealthStatus.Healthy
                    ? ResourceCommandState.Enabled
                    : ResourceCommandState.Disabled,
                IconName = "Document",
                IconVariant = IconVariant.Filled,
            };
            return builder.WithCommand(name, displayName, executeCommand: _ =>
                {
                    try
                    {
                        EndpointReference endpoint = builder.GetEndpoint("http");
                        string url = $"{endpoint.Url}/{openApiUiPath}";

                        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

                        return Task.FromResult(new ExecuteCommandResult { Success = true });
                    }
                    catch (Exception e)
                    {
                        return Task.FromResult(
                            new ExecuteCommandResult { Success = false, ErrorMessage = e.ToString() });
                    }
                }, commandOptions
            );
        }
    }
}
