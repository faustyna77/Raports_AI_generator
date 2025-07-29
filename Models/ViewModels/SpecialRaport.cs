using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AI_Raports_Generators.Models.ViewModels
{
    public class SpecialRaport
    {

        [Required]
        public List<SelectListItem> DocumentTypes { get; set; } = new();
        public string DocumentType { get; set; }

        [Required]
        public string NameOfCompany { get; set; }
        [Required]
        public string Description { get; set; }
       
        public string? GeneratedResponse { get; set; }
        public SendOptionsViewModel SendOptions { get; set; } = new();




    }
}
