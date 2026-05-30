using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

public class ResultByHour
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(ErrorMessage = "ResultByHour TimeFrom is required")]
    [DataType(DataType.DateTime, ErrorMessage = "Invalid date")]
    public DateTime TimeFrom { get; set; }

    [Required(ErrorMessage = "ResultByHour TimeTo is required")]
    [DataType(DataType.DateTime, ErrorMessage = "Invalid date")]
    public DateTime TimeTo { get; set; }

    [Required(ErrorMessage = "ResultByHour Results is required")]
    public List<Result> Results { get; set; }
}