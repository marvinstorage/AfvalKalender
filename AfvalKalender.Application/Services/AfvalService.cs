namespace AfvalKalender.Application.Services;

// Superseded by VerwerkKalenderCommandHandler (Application/Commands/).
// Can be safely deleted once all consumers are migrated.
[Obsolete("Use VerwerkKalenderCommandHandler via ICommandHandler<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>>")]
public sealed class AfvalService { }
