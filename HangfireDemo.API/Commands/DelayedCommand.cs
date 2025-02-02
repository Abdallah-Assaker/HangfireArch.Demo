using MediatR;

namespace HangfireDemo.API.Commands;

public record DelayedCommand(string Data) : IRequest;