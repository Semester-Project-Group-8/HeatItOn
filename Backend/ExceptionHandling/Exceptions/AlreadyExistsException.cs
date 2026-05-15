using System.Net;

namespace Backend.ExceptionHandling.Exceptions;

public class AlreadyExistsException : Exception
{
    public readonly HttpStatusCode StatusCode = HttpStatusCode.Conflict;
    public AlreadyExistsException(string message) : base(message)
    {}
}