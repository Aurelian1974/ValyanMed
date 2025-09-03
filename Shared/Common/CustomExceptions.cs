namespace Shared.Common;

/// <summary>
/// Business Rule Exception - pentru valid?ri business specifice
/// Conform planului de refactoring - CRITICAL PRIORITY
/// </summary>
public class BusinessLogicException : Exception
{
    public string Code { get; }
    public Dictionary<string, object> Properties { get; }

    public BusinessLogicException(string message, string code = "BUSINESS_RULE_VIOLATION") 
        : base(message)
    {
        Code = code;
        Properties = new Dictionary<string, object>();
    }

    public BusinessLogicException(string message, string code, Dictionary<string, object> properties) 
        : base(message)
    {
        Code = code;
        Properties = properties ?? new Dictionary<string, object>();
    }

    public BusinessLogicException(string message, Exception innerException, string code = "BUSINESS_RULE_VIOLATION") 
        : base(message, innerException)
    {
        Code = code;
        Properties = new Dictionary<string, object>();
    }
}

/// <summary>
/// Input Validation Exception - pentru valid?ri de input
/// </summary>
public class InputValidationException : Exception
{
    public List<string> Errors { get; }
    public Dictionary<string, List<string>> FieldErrors { get; }

    public InputValidationException(string message) : base(message)
    {
        Errors = new List<string> { message };
        FieldErrors = new Dictionary<string, List<string>>();
    }

    public InputValidationException(IEnumerable<string> errors) : base("Validation failed")
    {
        Errors = errors.ToList();
        FieldErrors = new Dictionary<string, List<string>>();
    }

    public InputValidationException(Dictionary<string, List<string>> fieldErrors) : base("Validation failed")
    {
        FieldErrors = fieldErrors;
        Errors = fieldErrors.SelectMany(kv => kv.Value).ToList();
    }

    public InputValidationException(string message, IEnumerable<string> errors) : base(message)
    {
        Errors = errors.ToList();
        FieldErrors = new Dictionary<string, List<string>>();
    }
}

/// <summary>
/// Resource Not Found Exception - pentru resurse care nu exist?
/// </summary>
public class ResourceNotFoundException : Exception
{
    public string ResourceType { get; }
    public string ResourceId { get; }

    public ResourceNotFoundException(string resourceType, string resourceId) 
        : base($"{resourceType} cu ID '{resourceId}' nu a fost gasit")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }

    public ResourceNotFoundException(string resourceType, string resourceId, string message) 
        : base(message)
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}

/// <summary>
/// Data Conflict Exception - pentru conflicte de date (duplicate, etc.)
/// </summary>
public class DataConflictException : Exception
{
    public string ConflictType { get; }
    public Dictionary<string, object> ConflictData { get; }

    public DataConflictException(string message, string conflictType = "DATA_CONFLICT") 
        : base(message)
    {
        ConflictType = conflictType;
        ConflictData = new Dictionary<string, object>();
    }

    public DataConflictException(string message, string conflictType, Dictionary<string, object> conflictData) 
        : base(message)
    {
        ConflictType = conflictType;
        ConflictData = conflictData ?? new Dictionary<string, object>();
    }
}

/// <summary>
/// External Service Exception - pentru erori de servicii externe
/// </summary>
public class ExternalServiceException : Exception
{
    public string ServiceName { get; }
    public int? StatusCode { get; }
    public string? ResponseContent { get; }

    public ExternalServiceException(string serviceName, string message) 
        : base($"Eroare la serviciul {serviceName}: {message}")
    {
        ServiceName = serviceName;
    }

    public ExternalServiceException(string serviceName, string message, int statusCode, string? responseContent = null) 
        : base($"Eroare la serviciul {serviceName} (Status: {statusCode}): {message}")
    {
        ServiceName = serviceName;
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }

    public ExternalServiceException(string serviceName, string message, Exception innerException) 
        : base($"Eroare la serviciul {serviceName}: {message}", innerException)
    {
        ServiceName = serviceName;
    }
}

/// <summary>
/// Application Security Exception - pentru probleme de securitate/autorizare
/// </summary>
public class ApplicationSecurityException : Exception
{
    public string SecurityContext { get; }
    public string? UserId { get; }
    public string? Action { get; }

    public ApplicationSecurityException(string message, string securityContext = "SECURITY") 
        : base(message)
    {
        SecurityContext = securityContext;
    }

    public ApplicationSecurityException(string message, string securityContext, string? userId, string? action = null) 
        : base(message)
    {
        SecurityContext = securityContext;
        UserId = userId;
        Action = action;
    }
}

/// <summary>
/// Helper pentru crearea rapid? de excep?ii
/// </summary>
public static class ExceptionFactory
{
    public static BusinessLogicException BusinessRule(string message, string? code = null)
        => new(message, code ?? "BUSINESS_RULE_VIOLATION");

    public static InputValidationException Validation(string message)
        => new(message);

    public static InputValidationException Validation(IEnumerable<string> errors)
        => new(errors);

    public static InputValidationException Validation(Dictionary<string, List<string>> fieldErrors)
        => new(fieldErrors);

    public static ResourceNotFoundException NotFound(string resourceType, string resourceId)
        => new(resourceType, resourceId);

    public static ResourceNotFoundException NotFound<T>(string resourceId)
        => new(typeof(T).Name, resourceId);

    public static DataConflictException Conflict(string message, string? conflictType = null)
        => new(message, conflictType ?? "DATA_CONFLICT");

    public static ExternalServiceException ExternalService(string serviceName, string message)
        => new(serviceName, message);

    public static ExternalServiceException ExternalService(string serviceName, string message, int statusCode)
        => new(serviceName, message, statusCode);

    public static ApplicationSecurityException Security(string message, string? context = null)
        => new(message, context ?? "SECURITY");

    public static ApplicationSecurityException Unauthorized(string? userId = null, string? action = null)
        => new("Nu aveti permisiunea pentru aceasta actiune", "UNAUTHORIZED", userId, action);

    public static ApplicationSecurityException Forbidden(string? userId = null, string? action = null)
        => new("Acces interzis", "FORBIDDEN", userId, action);
}

/// <summary>
/// Result Pattern Extensions pentru Exception Handling - SIMPLIFIED
/// </summary>
public static class ExceptionResultExtensions
{
    public static Result<T> ToResult<T>(this Exception exception)
    {
        return exception switch
        {
            InputValidationException ve => Result<T>.Failure(ve.Errors),
            BusinessLogicException bre => Result<T>.Failure(bre.Message),
            ResourceNotFoundException nfe => Result<T>.Failure(nfe.Message),
            DataConflictException ce => Result<T>.Failure(ce.Message),
            ExternalServiceException ese => Result<T>.Failure($"Eroare serviciu extern: {ese.Message}"),
            ApplicationSecurityException se => Result<T>.Failure($"Eroare securitate: {se.Message}"),
            _ => Result<T>.Failure($"Eroare nespecificata: {exception.Message}")
        };
    }
}