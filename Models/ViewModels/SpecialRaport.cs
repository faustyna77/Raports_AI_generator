using System.ComponentModel.DataAnnotations;

namespace AI_Raports_Generators.Models.ViewModels
{
    public class SpecialRaport
    {

        [Required]
        public string DocumentType { get; set; }
        [Required]
        public string NameOfCompany { get; set; }
        [Required]
        public string Description { get; set; }
       
        public string? GeneratedResponse { get; set; }



    }
}
