using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Backend.Models
{
    public class ResultList
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime TimeFrom { get; set; }
        [Required]
        public List<Result> Results { get; set; } = new List<Result>();
    }
}