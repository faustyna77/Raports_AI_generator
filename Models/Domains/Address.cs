using System.Collections.ObjectModel;

namespace AI_Raports_Generators.Models.Domains
{
    public class Address
    {
        public Address()
        {
            Clients = new Collection<Client>();
        }

        public int Id { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }

        public ICollection<Client> Clients { get; set; }
    }
}
