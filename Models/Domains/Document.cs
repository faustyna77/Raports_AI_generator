using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AI_Raports_Generators.Models.Domains
{
    public class Document
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public decimal Value { get; set; }

        public int MethodOfPaymentId { get; set; }
        public MethodOfPayment MethodOfPayment { get; set; }

        public DateTime PaymentDate { get; set; }
        public DateTime CreatedDate { get; set; }

        public string Comments { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; }
    }
}
