using System.ComponentModel.DataAnnotations;

namespace AI_Raports_Generators.Models.ViewModels
{
    public class EditPostViewModel
    {
        [Required]
        [Display(Name = "Tytuł posta")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Treść posta")]
        public string Content { get; set; }
    }
}
