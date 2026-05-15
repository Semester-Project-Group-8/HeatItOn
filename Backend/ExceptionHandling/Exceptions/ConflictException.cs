using System.Net;

namespace Backend.ExceptionHandling.Exceptions;

public class ConflictException : Exception
{
    public readonly HttpStatusCode StatusCode =  HttpStatusCode.Conflict;
    public ConflictException(string message) : base(message)
    {
    }
}