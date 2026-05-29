using System.Net;

namespace Backend.ExceptionHandling.Exceptions;

public class NotFoundException : Exception
{
    public readonly HttpStatusCode StatusCode = HttpStatusCode.NotFound;

    public NotFoundException(string message) : base(message)
    {
    }
}