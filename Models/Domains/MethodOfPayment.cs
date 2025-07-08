using System.Collections.ObjectModel;

namespace AI_Raports_Generators.Models.Domains
{
    public class MethodOfPayment
    {
        public MethodOfPayment()
        {
            Documents = new Collection<Document>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Document> Documents { get; set; }
    }
}
