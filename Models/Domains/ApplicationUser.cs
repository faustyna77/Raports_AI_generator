using Microsoft.AspNetCore.Identity;
using System.Collections.ObjectModel;

namespace AI_Raports_Generators.Models.Domains
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            Documents = new Collection<Document>();
        }

        public ICollection<Document> Documents { get; set; }
    }
}
