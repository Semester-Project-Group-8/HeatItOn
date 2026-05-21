using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

public class OptimizedResults
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id  { get; set; }
    [Required(ErrorMessage = "OptimizedResults Name is required")]
    [MinLength(1, ErrorMessage = "OptimizedResults Name must be at least 1 characters long")]
    public string Name  { get; set; }
    [Required(ErrorMessage = "OptimizedResults ResultsForHours is required")]
    public List<ResultByHour>  ResultsForHours  { get; set; }
}