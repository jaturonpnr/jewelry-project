namespace JewelryFactory.Domain.Exceptions;

/// <summary>
/// Thrown when an optimistic concurrency check fails (RowVersion mismatch).
/// Mapped to HTTP 409 Conflict by the global exception middleware.
/// </summary>
public class ConcurrencyException(string entity, object key)
    : Exception($"{entity} '{key}' was modified by another user. Please reload and try again.");
