using System.ComponentModel.DataAnnotations;

namespace BankMSWeb.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }
        public int AccountId { get; set; }
        public string? TransactionType { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Currency { get; set; }
        public string? Remarks { get; set; }

    }
    public class TransactionVw
    {
        [Key]
        public int TransactionId { get; set; }
        public int AccountId { get; set; }
        public string? AccountNo { get; set; }
        public string? TransactionType { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Currency { get; set; }
        public string? Remarks { get; set; }

    }
    public class TransactionDetailsVw
    {
        [Key]
        public int TransactionId { get; set; }
        public string? TransactionType { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Remarks { get; set; }

    }

    public class TransactionHistory
    {
        [Key]
        public int HistoryId { get; set; }
        public int AccountId { get; set; }
        public DateTime Date { get; set; }
        public string AccountType { get; set; }
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }
        public string? Remarks{ get; set; }

        public Account? Account {  get; set; }
        public string? AccountNo => Account?.AccountNo?? "N/A";
    }
    public class TransactionHistoryVW
    {
        [Key]
        public int HistoryId { get; set; }
        public int AccountId { get; set; }
        public DateTime Date { get; set; }
        public string AccountType { get; set; }
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }
        public string? Remarks { get; set; }

        public Account? Account { get; set; }
        public string? AccountNo => Account?.AccountNo ?? "N/A";
    }
}
