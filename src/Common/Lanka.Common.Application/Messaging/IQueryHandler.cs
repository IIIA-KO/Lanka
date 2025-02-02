using Lanka.Common.Domain;
using MediatR;

namespace Lanka.Common.Application.Messaging
{
    public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
        where TQuery : IQuery<TResponse>;
}
