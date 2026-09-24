namespace LibraryRental.Application.Common.Exceptions;

/// <summary>Maps to HTTP 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string name, object key) : base($"{name} '{key}' was not found.") { }
}

/// <summary>Maps to HTTP 403.</summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}

/// <summary>Maps to HTTP 401.</summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}

/// <summary>A Business Rule was violated. Maps to HTTP 400.</summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}

/// <summary>Concurrent modification. Maps to HTTP 409.</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
