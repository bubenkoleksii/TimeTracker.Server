using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Server.Models
{
    public class User
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string login { get; set; }
        [Required]
        public string password { get; set; }
    }
}