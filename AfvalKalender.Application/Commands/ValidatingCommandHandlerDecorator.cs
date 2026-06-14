using System.Threading;
using System.Threading.Tasks;

namespace AfvalKalender.Application.Commands;

public class ValidatingCommandHandlerDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult>
{
    private readonly ICommandHandler<TCommand, TResult> _innerHandler;
    private readonly ICommandValidator<TCommand> _validator;

    public ValidatingCommandHandlerDecorator(
        ICommandHandler<TCommand, TResult> innerHandler,
        ICommandValidator<TCommand> validator)
    {
        _innerHandler = innerHandler;
        _validator = validator;
    }

    public async Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default)
    {
        _validator.Validate(command);
        return await _innerHandler.HandleAsync(command, ct);
    }
}
