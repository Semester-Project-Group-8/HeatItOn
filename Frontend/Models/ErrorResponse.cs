using System.Collections.Generic;

namespace Frontend.Data;

public class ErrorResponse
{
    public Dictionary<string, string[]>? Errors { get; set; }
}