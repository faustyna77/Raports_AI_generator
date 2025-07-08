namespace AI_Raports_Generators.Models.Domains
{
    public class DocumentPosition
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }

        public int DocumentId { get; set; }
        public Document Document { get; set; }
    }
}
