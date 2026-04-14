using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class OptimizedResults
{
    [Required]
    public int Id  { get; set; }
    [Required]
    public string Name  { get; set; }
    [Required]
    public List<ResultList>  ResultsForHours  { get; set; }
}