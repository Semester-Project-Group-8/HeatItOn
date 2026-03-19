using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Backend.Models
{
    public class ResultList
    {
        public int Id { get; set; }
        public List<Result> Results { get; set; }
    }
}