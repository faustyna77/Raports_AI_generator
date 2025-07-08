using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace AI_Raports_Generators.Models.Domains
{
    public class Client
    {
        public Client()
        {
            Documents = new Collection<Document>();
        }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int AddressId { get; set; }
        public Address Address { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public ICollection<Document> Documents { get; set; }
    }
}
