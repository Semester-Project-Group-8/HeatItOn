using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Image
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string ImageLink { get; set; }
    }
}
