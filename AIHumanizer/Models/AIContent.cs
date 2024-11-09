using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIHumanizer.Models
{
    public class AIContent
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        [Required]
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}
