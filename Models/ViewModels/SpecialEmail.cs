using System.ComponentModel.DataAnnotations;

namespace AI_Raports_Generators.Models.ViewModels
{
    public class SpecialEmail
    {


        [Required]
        public string EmailAddress { get; set; }
        [Required]
        public string Topic { get; set; }
        [Required]

        public string Purpose { get; set; }


        public string? GeneratedEmail { get; set; }

    }
}
