using System.Collections.Generic;

namespace Frontend.Models;

public class OptimizedResults
{
    public int Id  { get; set; }
    public string? Name  { get; set; }
    public List<ResultList>?  ResultsForHours  { get; set; }
}