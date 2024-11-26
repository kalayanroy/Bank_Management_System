using System.ComponentModel.DataAnnotations;

namespace BankMSWeb.Models
{
    public class Account
    {
        [Key]
        public int AccountId { get; set; }
        [Required]
        public int? CustomerId { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string? AccountType { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 6)]
        public string? AccountNo { get; set; }
        [Required]
        public string? Currency { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public decimal? Balance { get; set; } = 0;
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        // Navigation property to Customer
        public Customer? Customer { get; set; }

        // Derived property to get CustomerName directly
        public string? CustomerName => Customer?.Username ?? "N/A";
    }

    public class AccountInfoView
    {
        [Key]
        public int AccountId { get; set; }
        public int CustomerId { get; set; }
        public string? AccountType { get; set; }
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public decimal? TotalDeposits { get; set; }
        public decimal? TotalWithdrawals { get; set; }
        public decimal? TotalLoans { get; set; }
        public decimal? TotalFundTransfer { get; set; }
        public decimal? CurrentBalance { get; set; }
    }
    public class AccountView
    {
        [Key]
        public int AccountId { get; set; }
        public string AccountNo { get; set; }
        public string CustomerName { get; set; }
        public string? AccountType { get; set; }
        public decimal? OpeningBalance { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class DetailsStatements
    {
        public AccountView AccountSummary { get; set; }
        public List<LoanVw> LoanList { get; set; }
        public List<TransactionDetailsVw> TransactionDetailsVws{ get; set; }
    }
}
