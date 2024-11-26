using System.ComponentModel.DataAnnotations;

namespace BankMSWeb.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? UserGuidId { get; set; }
        public DateTime? CareatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}
