using Lanka.Common.Domain;
using MediatR;

namespace Lanka.Common.Application.Messaging;
#pragma warning disable CA1040
public interface ICommand : IRequest<Result>, IBaseCommand;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;

public interface IBaseCommand;
