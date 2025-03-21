using Lanka.Common.Domain;
using MediatR;

namespace Lanka.Common.Application.Messaging;
#pragma warning disable CA1040
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
