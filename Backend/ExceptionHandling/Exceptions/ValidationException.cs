using System.Net;

namespace Backend.ExceptionHandling.Exceptions;

public class ValidationException : Exception
{
    public readonly HttpStatusCode StatusCode = HttpStatusCode.BadRequest;

    public ValidationException(string message) : base(message)
    {
    }
}