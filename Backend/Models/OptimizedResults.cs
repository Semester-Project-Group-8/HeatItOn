using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class OptimizedResults
{
    [Required]
    public int Id;
    [Required]
    public string Name;
    [Required]
    public List<ResultList>  ResultsForHours;
}