
    using System.ComponentModel.DataAnnotations;

    namespace AI_Raports_Generators.Models.ViewModels
    {
        public class SendOptionsViewModel
        {
            public bool SendToEmail { get; set; }

            [EmailAddress]
            public string? Email { get; set; }

            public bool SaveToDrive { get; set; }

            public string? GoogleDriveLink { get; set; }
        }
    }

