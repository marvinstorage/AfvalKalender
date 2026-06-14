using System.Collections.Generic;

namespace AfvalKalender.Application.Commands;

public interface ICommandValidator<TCommand>
{
    void Validate(TCommand command);
}
