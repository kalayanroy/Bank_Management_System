using System.ComponentModel.DataAnnotations;

namespace BankMSWeb.Models
{
    public class Dashboard
    {
        public List<TotalAmountDashboard> totalAmountDashboard { get; set; }
        public List<TransactionDashboard> transactionDashboard { get; set; }
        public List<TransactionChart> transactionChart { get; set; }
        public List<TransactionHistoryDashboard> transactionHistory { get; set; }
    }
    public class TotalAmountDashboard
    {
        [Key]
        public string TransactionType { get; set; }
        public decimal TotalAmount { get; set; }
    }
    public class TransactionDashboard
    {
        [Key]
        public int TransactionId { get; set; }
        public string AccountNo { get; set; }
        public string Username { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
    }
    public class TransactionChart
    {
        [Key]
        public decimal SL { get; set; }
        public string TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal amount { get; set; }
    }
    public class TransactionHistoryDashboard
    {
        [Key]
        public int TransactionId { get; set; }
        public string AccountNo { get; set; }
        public string Username { get; set; }
        public string TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
    }
}
