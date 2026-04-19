using MediatR;

namespace Auth.Application.Cqrs;

/// <summary>Marker: a write/mutation request returning TResponse.</summary>
public interface ICommand<out TResponse> : IRequest<TResponse> { }

/// <summary>Marker: a write/mutation request with no payload response.</summary>
public interface ICommand : IRequest<Unit> { }

/// <summary>Marker: a read-only request returning TResponse.</summary>
public interface IQuery<out TResponse> : IRequest<TResponse> { }