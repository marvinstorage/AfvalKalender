using System;

namespace AfvalKalender.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
