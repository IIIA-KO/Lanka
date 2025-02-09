using Microsoft.AspNetCore.Routing;

namespace Lanka.Common.Presentation.Endpoints
{
    public interface IEndpoint
    {
        void MapEndpoint(IEndpointRouteBuilder app);   
    }
}
